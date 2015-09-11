/*
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetFreeSwitch.Framework.FreeSwitch.Commands;
using NetFreeSwitch.Framework.FreeSwitch.Events;
using NetFreeSwitch.Framework.FreeSwitch.Messages;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Channels;

namespace NetFreeSwitch.Framework.FreeSwitch.Inbound {
    /// <summary>
    ///     ErrorHandler
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="exception"></param>
    public delegate void ErrorHandler(ITcpChannel channel, Exception exception);

    /// <summary>
    ///     Represent a FreeSwitch node connection
    /// </summary>
    public class FreeSwitch : ICallHandler {
        private readonly ITcpChannel _channel;
        private readonly ConcurrentQueue<object> _readItems = new ConcurrentQueue<object>();
        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(0, 1);
        private readonly ConcurrentQueue<CommandAsyncEvent> _requestsQueue;
        private readonly SemaphoreSlim _sendCompletedSemaphore = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _sendQueueSemaphore = new SemaphoreSlim(1, 1);
        private ConnectedCall _connectedCall;
        private Exception _sendException;

        public FreeSwitch(ITcpChannel channel) {
            _channel = channel;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            MessageReceived = OnMessageReceived;
            Error = OnError;
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

        public event EventHandler<EslDisconnectNoticeEventArgs> OnDisconnectNotice = delegate { };

        public event EventHandler<EslEventArgs> OnDtmf = delegate { };

        public event EventHandler<EslEventArgs> OnReceivedUnHandledEvent = delegate { };

        public event EventHandler<EslEventArgs> OnRecordStop = delegate { };

        public event EventHandler<EslEventArgs> OnSessionHeartbeat = delegate { };

        public event EventHandler<EslUnhandledMessageEventArgs> OnUnhandledMessage = delegate { };

        public event EventHandler<EslRudeRejectionEventArgs> OnRudeRejection = delegate { };

        /// <summary>
        ///     Can be compared with a session in a web server.
        /// </summary>
        public IChannelData ChannelData { get { return _channel.Data; } }

        /// <summary>
        ///     Connection State
        /// </summary>
        public bool Connected { get { return _channel != null && _channel.IsConnected; } }

        /// <summary>
        ///     Connected Call data
        /// </summary>
        public ConnectedCall ConnectedCall { get { return _connectedCall; } }

        /// <summary>
        ///     Something failed during processing.
        /// </summary>
        public ErrorHandler Error { get; private set; }

        /// <summary>
        ///     Channel received a new message
        /// </summary>
        public MessageHandler MessageReceived { get; private set; }

        /// <summary>
        ///     Address to the currently connected client.
        /// </summary>
        public EndPoint RemoteEndPoint { get { return _channel.RemoteEndpoint; } }

        public async Task<CommandReply> Answer() { return await ExecuteApplication("answer", true); }

        public async Task<CommandReply> BindDigitAction(Guid id, string command, bool eventLock = false) { return await ExecuteApplication("bind_digit_action", command, eventLock); }

        public async Task<CommandReply> Bridge(Guid id, string brigdeText) { return await ExecuteApplication("bridge", brigdeText, false); }

        /// <summary>
        ///     Close()
        /// </summary>
        public async Task Close() {
            if (_channel == null)
                return;
            _connectedCall = null;
            await _channel.CloseAsync();
        }

        /// <summary>
        ///     Connect(). It is used to get the connected call data.
        /// </summary>
        /// <returns>Async Task</returns>
        public async Task Connect() {
            ConnectCommand command = new ConnectCommand();
            CommandReply reply = await Send(command);
            if (reply != null) {
                string eventName = reply["Event-Name"];
                if (!string.IsNullOrEmpty(eventName)
                    && eventName.Equals("CHANNEL_DATA")) {
                    var properties = new Dictionary<string, string>();
                    reply.CopyParameters(ref properties);
                    _connectedCall = new ConnectedCall(properties);
                }
            }
        }

        /// <summary>
        ///     DivertEvents(). It is to allow events that an embedded script would expect to get in the inputcallback to be
        ///     diverted to the event socket.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DivertEvents() {
            DivertEventsCommand command = new DivertEventsCommand(true);
            CommandReply reply = await Send(command);
            return reply != null && reply.IsSuccessful;
        }

        public async Task<CommandReply> Hangup(Guid id, string reason) { return await ExecuteApplication("hangup", reason, true); }

        /// <summary>
        ///     MyEvents(). Receive only events from this channel.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MyEvents() {
            Guid guid = Guid.Parse(ConnectedCall.UniqueID);
            MyEventsCommand command = new MyEventsCommand(guid);
            CommandReply reply = await Send(command);
            return reply != null && reply.IsSuccessful;
        }

        public async Task<CommandReply> PlayAudioFile(Guid id, string filePath, bool eventLock) { return await ExecuteApplication("playback", filePath, eventLock); }

        public async Task<CommandReply> PreAnswer(Guid id) { return await ExecuteApplication("pre_answer", false); }

        public async Task<CommandReply> Record(Guid id, string path, int? timeLimit, int? silenceThreshhold, int? silenceHits, bool eventLock) { return await ExecuteApplication("record", path + " " + (timeLimit.HasValue ? timeLimit.ToString() : "") + " " + (silenceThreshhold.HasValue ? silenceThreshhold.ToString() : "") + " " + (silenceHits.HasValue ? silenceHits.ToString() : ""), eventLock); }

        /// <summary>
        ///     Resume().
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Resume() {
            ResumeCommand command = new ResumeCommand();
            CommandReply reply = await Send(command);
            return reply != null && reply.IsSuccessful;
        }

        public async Task<CommandReply> RingReady(bool eventLock) { return await ExecuteApplication("ring_ready", true); }

        public async Task<CommandReply> Say(Guid id, string language, EslSayTypes type, EslSayMethods method, EslSayGenders gender, string text, int loop, bool eventLock) { return await ExecuteApplication("say", language + " " + type + " " + method.ToString().Replace("_", "/") + " " + gender + " " + text, loop, eventLock); }

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

        public async Task<CommandReply> SetDigitActionRealm(Guid id, string realm, bool eventLock) { return await ExecuteApplication("digit_action_set_realm", (realm ?? "all"), eventLock); }

        /// <summary>
        ///     SendLog(). It is used to request for FreeSwitch log.
        /// </summary>
        /// <param name="level">The log level to specify</param>
        /// <returns></returns>
        public async Task SetLogLevel(EslLogLevels level) {
            LogCommand command = new LogCommand(level);
            await SendAsync(command);
        }

        public async Task<CommandReply> SetVariable(Guid id, string name, string value) {
            if (string.IsNullOrEmpty(value))
                return await ExecuteApplication("unset", name, false);
            return await ExecuteApplication("set", name + "=" + value, false);
        }

        public async Task<CommandReply> SetVariable(Guid id, string name, string value, bool eventLock) {
            if (string.IsNullOrEmpty(value))
                return await ExecuteApplication("unset", name, eventLock);
            return await ExecuteApplication("set", name + "=" + value, eventLock);
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

        public async Task<CommandReply> Sleep(Guid id, int milliSeconds) { return await ExecuteApplication("sleep", Convert.ToString(milliSeconds), true); }

        public async Task<CommandReply> Sleep(Guid id, int milliSeconds, bool eventLock) { return await ExecuteApplication("sleep", Convert.ToString(milliSeconds), eventLock); }

        public async Task<CommandReply> Speak(Guid id, string engine, string voice, string text, string timerName, int loop, bool eventLock) { return await ExecuteApplication("speak", engine + "|" + voice + "|" + text + (!string.IsNullOrEmpty(timerName) ? "|" + timerName : ""), loop, eventLock); }

        public async Task<CommandReply> StartDtmf(Guid id, bool eventLock) { return await ExecuteApplication("start_dtmf", string.Empty, eventLock); }

        public async Task<CommandReply> StopDtmf(bool eventLock = false) { return await ExecuteApplication("stop_dtmf", eventLock); }

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
        ///     Execute an application with arguments against FreeSwitch
        /// </summary>
        /// <param name="applicationName">The application name</param>
        /// <param name="applicationArguments">The application arguments</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns></returns>
        protected async Task<CommandReply> ExecuteApplication(string applicationName, string applicationArguments, bool eventLock) {
            SendMsgCommand command = new SendMsgCommand(Guid.Parse(ConnectedCall.UniqueID), SendMsgCommand.CALL_COMMAND, applicationName, applicationArguments, eventLock);
            return await Send(command);
        }

        /// <summary>
        ///     Execute an application against FreeSwitch
        /// </summary>
        /// <param name="applicationName">The application name</param>
        /// <param name="eventLock">Asynchronous status</param>
        /// <returns></returns>
        protected async Task<CommandReply> ExecuteApplication(string applicationName, bool eventLock) {
            SendMsgCommand command = new SendMsgCommand(applicationName, string.Empty, eventLock);
            return await Send(command);
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

        /// <summary>
        ///     Execute an application against FreeSwitch
        /// </summary>
        /// <param name="applicationName">Application name</param>
        /// <param name="applicationArguments">Application arguments</param>
        /// <param name="loop">The number of times to execute the app</param>
        /// <param name="eventLock">Asynchronous state</param>
        /// <returns>Async CommandReply</returns>
        private async Task<CommandReply> ExecuteApplication(string applicationName, string applicationArguments, int loop, bool eventLock) {
            SendMsgCommand command = new SendMsgCommand(Guid.Parse(ConnectedCall.UniqueID), SendMsgCommand.CALL_COMMAND, applicationName, applicationArguments, eventLock, loop);
            return await Send(command);
        }

        private void OnError(ITcpChannel channel, Exception exception) { throw new NotImplementedException(); }

        private async void OnMessageReceived(ITcpChannel channel, object message) {
            // Here we validate the channel.
            if (!channel.ChannelId.Equals(_channel.ChannelId)) return;
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
                case "text/disconnect-notice":
                    _connectedCall = null;
                    await _channel.CloseAsync();
                    var notice = new DisconnectNotice(decodedMessage.BodyText);
                    if (OnDisconnectNotice != null) OnDisconnectNotice(this, new EslDisconnectNoticeEventArgs(notice));
                    break;
                case "text/rude-rejection":
                    _connectedCall = null;
                    await _channel.CloseAsync();
                    RudeRejection reject = new RudeRejection(decodedMessage.BodyText);
                    if (OnRudeRejection != null) OnRudeRejection(this, new EslRudeRejectionEventArgs(reject));
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