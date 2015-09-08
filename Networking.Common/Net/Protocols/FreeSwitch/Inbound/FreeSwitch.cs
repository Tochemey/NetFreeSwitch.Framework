﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Networking.Common.Net.Channels;
using Networking.Common.Net.Protocols.FreeSwitch.Command;
using Networking.Common.Net.Protocols.FreeSwitch.Events;
using Networking.Common.Net.Protocols.FreeSwitch.Message;

namespace Networking.Common.Net.Protocols.FreeSwitch.Inbound {
    /// <summary>
    ///     Represent a FreeSwitch node connection
    /// </summary>
    public class FreeSwitch : IDisposable {
        private readonly ITcpChannel _channel;
        private readonly ConcurrentQueue<object> _readItems = new ConcurrentQueue<object>();
        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(0, 1);
        private readonly ConcurrentQueue<CommandAsyncEvent> _requestsQueue;
        private readonly SemaphoreSlim _sendCompletedSemaphore = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _sendQueueSemaphore = new SemaphoreSlim(1, 1);
        private Exception _sendException;

        public FreeSwitch(ITcpChannel channel) {
            _channel = channel;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            MessageReceived = OnMessageReceived;
        }

        public event EventHandler<EslEventArgs> OnBackgroundJob = delegate { };

        public event EventHandler<EslEventArgs> OnCallUpdate = delegate { };

        public event EventHandler<EslEventArgs> OnChannelAnswer = delegate { };

        public event EventHandler<EslEventArgs> OnChannelBridge = delegate { };

        public event EventHandler<EslEventArgs> OnChannelExecute = delegate { };

        public event EventHandler<EslEventArgs> OnChannelExecuteComplete = delegate { };

        public event EventHandler<EslEventArgs> OnChannelHangup = delegate { };

        public event EventHandler<EslEventArgs> OnChannelHangupComplete = delegate { };

        public event EventHandler<EslEventArgs> OnChannelOriginate = delegate { };

        public event EventHandler<EslEventArgs> OnChannelPark = delegate { };

        public event EventHandler<EslEventArgs> OnChannelProgress = delegate { };

        public event EventHandler<EslEventArgs> OnChannelProgressMedia = delegate { };

        public event EventHandler<EslEventArgs> OnChannelState = delegate { };

        public event EventHandler<EslEventArgs> OnChannelUnbridge = delegate { };

        public event EventHandler<EslEventArgs> OnChannelUnPark = delegate { };

        public event EventHandler<EslEventArgs> OnCustom = delegate { };

        public event EventHandler<EslEventArgs> OnDtmf = delegate { };

        public event EventHandler<EslEventArgs> OnReceivedUnHandledEvent = delegate { };

        public event EventHandler<EslEventArgs> OnRecordStop = delegate { };

        public event EventHandler<EslEventArgs> OnSessionHeartbeat = delegate { };

        public event EventHandler<EslUnhandledMessageEventArgs> OnUnhandledMessage = delegate { };

        /// <summary>
        ///     Can be compared with a session in a web server.
        /// </summary>
        public IChannelData ChannelData { get { return _channel.Data; } }

        /// <summary>
        ///     Connection State
        /// </summary>
        public bool Connected { get { return _channel != null && _channel.IsConnected; } }

        /// <summary>
        ///     Channel received a new message
        /// </summary>
        public MessageHandler MessageReceived { get; private set; }

        /// <summary>
        ///     Address to the currently connected client.
        /// </summary>
        public EndPoint RemoteEndPoint { get { return _channel.RemoteEndpoint; } }

        /// <summary>
        ///     Dispose()
        /// </summary>
        public void Dispose() {
            if (_channel == null)
                return;
            _channel.Close();
        }

        /// <summary>
        ///     Send().
        ///     It sends a command to FreeSwitch and awaist for a CommandReply response
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>CommandReply response</returns>
        public async Task<CommandReply> Send(BaseCommand command) {
            if (!Connected)
                return null;
            // Send command
            EnqueueCommand(command);
            await SendAsync(command);
            await _readSemaphore.WaitAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
            object item;
            var gotItem = _readItems.TryDequeue(out item);
            if (!gotItem)
                throw new ChannelException("Was signalled that something have been recieved, but found nothing in the in queue");
            // signal so that more items can be read directly
            if (_readItems.Count > 0)
                _readSemaphore.Release();
            return await ValidateResponse(command, item as EslMessage) as CommandReply;
        }

        /// <summary>
        ///     SendApi().
        ///     It is used to send an api command to FreeSwitch.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>ApiResponse response</returns>
        public async Task<ApiResponse> SendApi(ApiCommand command) {
            if (!Connected)
                return null;
            // Send the command
            EnqueueCommand(command);
            await SendAsync(command);
            await _readSemaphore.WaitAsync(TimeSpan.FromMilliseconds(-1), CancellationToken.None);
            object item;
            var gotItem = _readItems.TryDequeue(out item);
            if (!gotItem)
                throw new ChannelException("Was signalled that something have been recieved, but found nothing in the in queue");
            // signal so that more items can be read directly
            if (_readItems.Count > 0)
                _readSemaphore.Release();
            return await ValidateResponse(command, item as EslMessage) as ApiResponse;
        }

        /// <summary>
        ///     SendBgApi().
        ///     It is used to send commands to FreeSwitch that will be executed in the background. It will return a UUID.
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>Job ID</returns>
        public async Task<Guid> SendBgApi(BgApiCommand command) {
            if (!Connected)
                return Guid.Empty;
            // Send command
            var reply = await Send(command);
            if (reply == null) return Guid.Empty;
            if (!reply.IsSuccessful) return Guid.Empty;
            var jobId = reply["Job-UUID"];
            Guid jobUuid;
            return Guid.TryParse(jobId, out jobUuid) ? jobUuid : Guid.Empty;
        }

        /// <summary>
        ///     SendLog(). It is used to request for FreeSwitch log.
        /// </summary>
        /// <param name="level">The log level to specify</param>
        /// <returns></returns>
        public async Task SetLogLevel(EslLogLevels level) {
            LogCommand command = new LogCommand(level);
            await SendAsync(command);
        }

        /// <summary>
        ///     ShutdownGracefully().
        ///     Send an exit command to FreeSwitch
        /// </summary>
        /// <returns>Async task</returns>
        public async Task ShutdownGracefully() {
            // Let us send exit command to FreeSwitch
            ExitCommand exit = new ExitCommand();
            await SendAsync(exit);
        }

        /// <summary>
        ///     Used to dispatch .NET events
        /// </summary>
        protected void DispatchEvents(EslEventType eventType, EslEventArgs ea) {
            EventHandler<EslEventArgs> handler = null;
            switch (eventType) {
                case EslEventType.BACKGROUND_JOB:
                    handler = OnBackgroundJob;
                    break;
                case EslEventType.CALL_UPDATE:
                    handler = OnCallUpdate;
                    break;
                case EslEventType.CHANNEL_BRIDGE:
                    handler = OnChannelBridge;
                    break;
                case EslEventType.CHANNEL_HANGUP:
                    handler = OnChannelHangup;
                    break;
                case EslEventType.CHANNEL_HANGUP_COMPLETE:
                    handler = OnChannelHangupComplete;
                    break;
                case EslEventType.CHANNEL_PROGRESS:
                    handler = OnChannelProgress;
                    break;
                case EslEventType.CHANNEL_PROGRESS_MEDIA:
                    handler = OnChannelProgressMedia;
                    break;
                case EslEventType.CHANNEL_EXECUTE:
                    handler = OnChannelExecute;
                    break;
                case EslEventType.CHANNEL_EXECUTE_COMPLETE:
                    handler = OnChannelExecuteComplete;
                    break;
                case EslEventType.CHANNEL_UNBRIDGE:
                    handler = OnChannelUnbridge;
                    break;
                case EslEventType.SESSION_HEARTBEAT:
                    handler = OnSessionHeartbeat;
                    break;
                case EslEventType.DTMF:
                    handler = OnDtmf;
                    break;
                case EslEventType.RECORD_STOP:
                    handler = OnRecordStop;
                    break;
                case EslEventType.CUSTOM:
                    handler = OnCustom;
                    break;
                case EslEventType.CHANNEL_STATE:
                    handler = OnChannelState;
                    break;
                case EslEventType.CHANNEL_ANSWER:
                    handler = OnChannelAnswer;
                    break;
                case EslEventType.CHANNEL_ORIGINATE:
                    handler = OnChannelOriginate;
                    break;
                case EslEventType.CHANNEL_PARK:
                    handler = OnChannelPark;
                    break;
                case EslEventType.CHANNEL_UNPARK:
                    handler = OnChannelUnPark;
                    break;
                case EslEventType.UN_HANDLED_EVENT:
                    handler = OnReceivedUnHandledEvent;
                    break;
            }
            if (handler == null) return;
            try { handler(this, ea); }
            catch (Exception) {
                // ignored
            }
        }

        /// <summary>
        ///     Add the command request to the waiting list.
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns></returns>
        protected CommandAsyncEvent EnqueueCommand(BaseCommand command) {
            var event2 = new CommandAsyncEvent(command);
            _requestsQueue.Enqueue(event2);
            return event2;
        }

        /// <summary>
        ///     FreeSwitch Events listener hook
        /// </summary>
        protected void PopEvent(EslEvent @event) {
            if (string.IsNullOrEmpty(@event.EventName)) return;
            switch (@event.EventName.ToUpper()) {
                case "CHANNEL_HANGUP":
                    DispatchEvents(EslEventType.CHANNEL_HANGUP, new EslEventArgs(new ChannelHangup(@event.Items)));
                    break;
                case "CHANNEL_HANGUP_COMPLETE":
                    DispatchEvents(EslEventType.CHANNEL_HANGUP_COMPLETE, new EslEventArgs(new ChannelHangup(@event.Items)));
                    break;
                case "CHANNEL_PROGRESS":
                    DispatchEvents(EslEventType.CHANNEL_PROGRESS, new EslEventArgs(new ChannelProgress(@event.Items)));
                    break;
                case "CHANNEL_PROGRESS_MEDIA":
                    DispatchEvents(EslEventType.CHANNEL_PROGRESS_MEDIA, new EslEventArgs(new ChannelProgressMedia(@event.Items)));
                    break;
                case "CHANNEL_EXECUTE":
                    DispatchEvents(EslEventType.CHANNEL_EXECUTE, new EslEventArgs(new ChannelExecute(@event.Items)));
                    break;
                case "CHANNEL_EXECUTE_COMPLETE":
                    DispatchEvents(EslEventType.CHANNEL_EXECUTE_COMPLETE, new EslEventArgs(new ChannelExecuteComplete(@event.Items)));
                    break;
                case "CHANNEL_BRIDGE":
                    DispatchEvents(EslEventType.CHANNEL_BRIDGE, new EslEventArgs(new ChannelBridge(@event.Items)));
                    break;
                case "CHANNEL_UNBRIDGE":
                    DispatchEvents(EslEventType.CHANNEL_UNBRIDGE, new EslEventArgs(new ChannelUnbridge(@event.Items)));
                    break;
                case "BACKGROUND_JOB":
                    DispatchEvents(EslEventType.BACKGROUND_JOB, new EslEventArgs(new BackgroundJob(@event.Items)));
                    break;
                case "SESSION_HEARTBEAT":
                    DispatchEvents(EslEventType.SESSION_HEARTBEAT, new EslEventArgs(new SessionHeartbeat(@event.Items)));
                    break;
                case "CHANNEL_STATE":
                    DispatchEvents(EslEventType.CHANNEL_STATE, new EslEventArgs(new ChannelStateEvent(@event.Items)));
                    break;
                case "DTMF":
                    DispatchEvents(EslEventType.DTMF, new EslEventArgs(new Dtmf(@event.Items)));
                    break;
                case "RECORD_STOP":
                    DispatchEvents(EslEventType.RECORD_STOP, new EslEventArgs(new RecordStop(@event.Items)));
                    break;
                case "CALL_UPDATE":
                    DispatchEvents(EslEventType.CALL_UPDATE, new EslEventArgs(new CallUpdate(@event.Items)));
                    break;
                case "CUSTOM":
                    DispatchEvents(EslEventType.CUSTOM, new EslEventArgs(new Custom(@event.Items)));
                    break;
                case "CHANNEL_ANSWER":
                    DispatchEvents(EslEventType.CHANNEL_ANSWER, new EslEventArgs(@event));
                    break;
                case "CHANNEL_ORIGINATE":
                    DispatchEvents(EslEventType.CHANNEL_ORIGINATE, new EslEventArgs(@event));
                    break;
                case "CHANNEL_PARK":
                    DispatchEvents(EslEventType.CHANNEL_PARK, new EslEventArgs(new ChannelPark(@event.Items)));
                    break;
                case "CHANNEL_UNPARK":
                    DispatchEvents(EslEventType.CHANNEL_UNPARK, new EslEventArgs(@event));
                    break;
                default:
                    DispatchEvents(EslEventType.UN_HANDLED_EVENT, new EslEventArgs(@event));
                    break;
            }
        }

        /// <summary>
        ///     SendAsync(). It is used internally to send command to FreeSwitch
        /// </summary>
        /// <param name="message">the command to send</param>
        /// <returns>Async task</returns>
        protected async Task SendAsync(object message) {
            if (message == null) throw new ArgumentNullException("message");
            if (_sendException != null) {
                var ex = _sendException;
                _sendException = null;
                throw new AggregateException(ex);
            }
            await _sendQueueSemaphore.WaitAsync();
            _channel.Send(message);
            await _sendCompletedSemaphore.WaitAsync();
            _sendQueueSemaphore.Release();
        }

        /// <summary>
        ///     Valiates every response received seeing that we can send commands to FreeSwitch asynchronously and wait for
        ///     responses.
        ///     However some responses may be for another command previously sent. In that regard, every command has a sequence
        ///     number
        ///     attached to it that helps differentiate between them and easily map their responses.
        /// </summary>
        /// <param name="command">The original command send</param>
        /// <param name="response">The actual response received</param>
        /// <returns>EslMessage</returns>
        protected async Task<EslMessage> ValidateResponse(BaseCommand command, EslMessage response) {
            if (response == null) return null;
            if (_requestsQueue.Count <= 0) return null;
            CommandAsyncEvent event2;
            if (!_requestsQueue.TryDequeue(out event2)) return null;
            if (event2 == null) return null;
            if (!event2.Command.Equals(command)) return null;
            event2.Complete(response);
            return await event2.Task;
        }

        private void OnMessageReceived(ITcpChannel channel, object message) {
            if (channel.ChannelId.Equals(_channel.ChannelId)) {
                var decodedMessage = message as EslDecodedMessage;
                // Handle decoded message.
                if (decodedMessage == null) return;
                if (decodedMessage.Headers == null
                    || !decodedMessage.Headers.HasKeys())
                    return;
                var headers = decodedMessage.Headers;
                var contentType = headers["Content-Type"];
                if (string.IsNullOrEmpty(contentType)) return;
                contentType = contentType.ToLowerInvariant();
                switch (contentType) {
                    case "command/reply":
                        var reply = new CommandReply(headers, decodedMessage.OriginalMessage);
                        _readItems.Enqueue(reply);
                        if (_readSemaphore.CurrentCount == 0)
                            _readSemaphore.Release();
                        break;
                    case "api/response":
                        var apiResponse = new ApiResponse(decodedMessage.BodyText);
                        _readItems.Enqueue(apiResponse);
                        if (_readSemaphore.CurrentCount == 0)
                            _readSemaphore.Release();
                        break;
                    case "text/event-plain":
                        var parameters = decodedMessage.BodyLines.AllKeys.ToDictionary(key => key, key => decodedMessage.BodyLines[key]);
                        var @event = new EslEvent(parameters);
                        PopEvent(@event);
                        break;
                    case "log/data":
                        var logdata = new LogData(headers, decodedMessage.BodyText);
                        break;
                    default:
                        // Here we are handling an unknown message
                        var msg = new EslMessage(decodedMessage.Headers, decodedMessage.OriginalMessage);
                        if (OnUnhandledMessage != null) OnUnhandledMessage(this, new EslUnhandledMessageEventArgs(msg));
                        break;
                }
            }
        }
    }
}