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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetFreeSwitch.Framework.Common;
using NetFreeSwitch.Framework.FreeSwitch.Codecs;
using NetFreeSwitch.Framework.FreeSwitch.Commands;
using NetFreeSwitch.Framework.FreeSwitch.Events;
using NetFreeSwitch.Framework.FreeSwitch.Messages;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Buffers;
using NetFreeSwitch.Framework.Net.Channels;
using NLog;

namespace NetFreeSwitch.Framework.FreeSwitch.Outbound {
    /// <summary>
    ///     Connects to FreeSwitch and execute commands.
    /// </summary>
    public class OutboundChannelSession : IDisposable {
        private static bool _authenticated;
        private readonly SocketAsyncEventArgs _args = new SocketAsyncEventArgs();
        private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(0, 1);
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<CommandAsyncEvent> _requestsQueue;
        private readonly SemaphoreSlim _sendCompletedSemaphore = new SemaphoreSlim(0, 1);
        private readonly SemaphoreSlim _sendQueueSemaphore = new SemaphoreSlim(1, 1);
        private TcpChannel _channel;
        private Exception _connectException;
        private Exception _sendException;
        private Socket _socket;


        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch.
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="messageEncoder">FreeSwitch message encoder</param>
        /// <param name="messageDecoder">FreeSwitch message decoder</param>
        /// <param name="freeSwitchEventFilter">FreeSwitch event filters</param>
        /// <param name="connectionTimeout">Connection Timeout</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        public OutboundChannelSession(string address, int port, IMessageEncoder messageEncoder,
            IMessageDecoder messageDecoder, string freeSwitchEventFilter, TimeSpan connectionTimeout, string password) {
            Address = address;
            Port = port;
            MessageEncoder = messageEncoder;
            MessageDecoder = messageDecoder;
            FreeSwitchEventFilter = freeSwitchEventFilter;
            ConnectionTimeout = connectionTimeout;
            Password = password;
            _authenticated = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), MessageEncoder, MessageDecoder);
            _channel.MessageReceived += OnMessageReceived;
            _channel.Disconnected += OnDisconnect;
            _channel.MessageSent += OnSendCompleted;
            _args.Completed += OnConnect;
            OnAuthentication += SendAuthentication;
        }

        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch using in-built message encoder/decoders
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="connectionTimeout">Connection Timeout</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        public OutboundChannelSession(string address, int port, TimeSpan connectionTimeout, string password) {
            Address = address;
            Port = port;
            ConnectionTimeout = connectionTimeout;
            Password = password;
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), MessageEncoder, MessageDecoder);
            _channel.MessageReceived += OnMessageReceived;
            _channel.Disconnected += OnDisconnect;
            _channel.MessageSent += OnSendCompleted;
            _args.Completed += OnConnect;
            OnAuthentication += SendAuthentication;
        }

        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch using in-built message encoder/decoders.
        ///     The connection timeout is set to zero
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        public OutboundChannelSession(string address, int port, string password) {
            Address = address;
            Port = port;
            ConnectionTimeout = TimeSpan.Zero;
            Password = password;
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), MessageEncoder, MessageDecoder);
            _channel.MessageReceived += OnMessageReceived;
            _channel.Disconnected += OnDisconnect;
            _channel.MessageSent += OnSendCompleted;
            _args.Completed += OnConnect;
            OnAuthentication += SendAuthentication;
        }

        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch using in-built message encoder/decoders.
        ///     The connection timeout is set to zero
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        /// <param name="eventfilters">FreeSwitch event list</param>
        public OutboundChannelSession(string address, int port, string password, string eventfilters) {
            Address = address;
            Port = port;
            ConnectionTimeout = TimeSpan.Zero;
            Password = password;
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain " + eventfilters;
            _authenticated = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), MessageEncoder, MessageDecoder);
            _channel.MessageReceived += OnMessageReceived;
            _channel.Disconnected += OnDisconnect;
            _channel.MessageSent += OnSendCompleted;
            _args.Completed += OnConnect;
            OnAuthentication += SendAuthentication;
        }

        /// <summary>
        ///     Default constructor. It uses the default FreeSwitch mod_event_socket settings for connectivity
        /// </summary>
        public OutboundChannelSession() {
            Address = "127.0.0.1";
            Port = 8021;
            ConnectionTimeout = TimeSpan.Zero;
            Password = "ClueCon";
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channel = new TcpChannel(new BufferSlice(new byte[65535], 0, 65535), MessageEncoder, MessageDecoder);
            _channel.MessageReceived += OnMessageReceived;
            _channel.Disconnected += OnDisconnect;
            _channel.MessageSent += OnSendCompleted;
            _args.Completed += OnConnect;
            OnAuthentication += SendAuthentication;
        }

        /// <summary>
        ///     Event Socket Address
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     Authentication State
        /// </summary>
        public bool Authenticated {
            get { return _authenticated; }
            set { _authenticated = value; }
        }

        /// <summary>
        ///     Connection State
        /// </summary>
        public bool Connected => _channel != null && _channel.IsConnected;

        /// <summary>
        ///     Connection Timeout
        /// </summary>
        public TimeSpan ConnectionTimeout { get; }

        /// <summary>
        ///     FreeSwitch Events Filter
        /// </summary>
        public string FreeSwitchEventFilter { get; }

        /// <summary>
        ///     FreeSwitch Message Decoder
        /// </summary>
        public IMessageDecoder MessageDecoder { get; }

        /// <summary>
        ///     FreeSwich Message Encoder
        /// </summary>
        public IMessageEncoder MessageEncoder { get; }

        /// <summary>
        ///     Event Socket Password
        /// </summary>
        public string Password { get; }

        /// <summary>
        ///     Event Socket Port
        /// </summary>
        public int Port { get; }

        /// <summary>
        ///     Dispose()
        /// </summary>
        public void Dispose() {
            if (_channel == null)
                return;
            _channel.Close();
            _channel = null;
        }

        public event AsyncEventHandler<EslEventArgs> OnBackgroundJob;

        public event AsyncEventHandler<EslEventArgs> OnCallUpdate;

        public event AsyncEventHandler<EslEventArgs> OnChannelAnswer;

        public event AsyncEventHandler<EslEventArgs> OnChannelBridge;

        public event AsyncEventHandler<EslEventArgs> OnChannelExecute;

        public event AsyncEventHandler<EslEventArgs> OnChannelExecuteComplete;

        public event AsyncEventHandler<EslEventArgs> OnChannelHangup;

        public event AsyncEventHandler<EslEventArgs> OnChannelHangupComplete;

        public event AsyncEventHandler<EslEventArgs> OnChannelOriginate;

        public event AsyncEventHandler<EslEventArgs> OnChannelPark;

        public event AsyncEventHandler<EslEventArgs> OnChannelProgress;

        public event AsyncEventHandler<EslEventArgs> OnChannelProgressMedia;

        public event AsyncEventHandler<EslEventArgs> OnChannelState;

        public event AsyncEventHandler<EslEventArgs> OnChannelUnbridge;

        public event AsyncEventHandler<EslEventArgs> OnChannelUnPark;

        public event AsyncEventHandler<EslEventArgs> OnCustom;

        public event AsyncEventHandler<EslDisconnectNoticeEventArgs> OnDisconnectNotice;

        public event AsyncEventHandler<EslEventArgs> OnDtmf;

        public event AsyncEventHandler<EslEventArgs> OnReceivedUnHandledEvent;

        public event AsyncEventHandler<EslEventArgs> OnRecordStop;

        public event AsyncEventHandler<EslRudeRejectionEventArgs> OnRudeRejection;

        public event AsyncEventHandler<EslEventArgs> OnSessionHeartbeat;

        public event AsyncEventHandler<EslUnhandledMessageEventArgs> OnUnhandledMessage;

        protected event AsyncEventHandler<EventArgs> OnAuthentication;

        /// <summary>
        ///     Disconnect from FreeSwitch mod_event_socket
        /// </summary>
        /// <returns></returns>
        /// <summary>
        ///     Wait for all messages to be sent and close the connection
        /// </summary>
        /// <returns>Async task</returns>
        public async Task CloseAsync() {
            await ShutdownGracefully();
            await _channel.CloseAsync();
            _authenticated = false;
            _channel = null;
        }

        /// <summary>
        ///     ConnectAsync().
        ///     It used to connect to FreeSwitch mod_event_socket as a client.
        /// </summary>
        /// <returns>Async task</returns>
        public async Task ConnectAsync() {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already connected");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 600));

            // configured address
            var address = Address.IsValidIPv4() ? IPAddress.Parse(Address) : Address.ToIPAddress();

            _args.RemoteEndPoint = new IPEndPoint(address, Port);
            var isPending = _socket.ConnectAsync(_args);
            if (!isPending)
                return;
            await _connectSemaphore.WaitAsync(ConnectionTimeout);
            if (_connectException != null)
                throw _connectException;
        }

        /// <summary>
        ///     Send().
        ///     It sends a command to FreeSwitch and awaist for a CommandReply response
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>CommandReply response</returns>
        public async Task<CommandReply> Send(BaseCommand command) {
            if (!Connected
                || !Authenticated)
                return null;
            // Send command
            var event2 = EnqueueCommand(command);
            if (_log.IsDebugEnabled) _log.Debug("command to be sent <<{0}>>", command.ToString());

            await SendAsync(command);
            return await event2.Task as CommandReply;
        }

        /// <summary>
        ///     SendApi().
        ///     It is used to send an api command to FreeSwitch.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>ApiResponse response</returns>
        public async Task<ApiResponse> SendApi(ApiCommand command) {
            if (!Connected
                || !Authenticated)
                return null;
            // Send the command
            var event2 = EnqueueCommand(command);
            await SendAsync(command);
            return await event2.Task as ApiResponse;
        }

        /// <summary>
        ///     SendBgApi().
        ///     It is used to send commands to FreeSwitch that will be executed in the background. It will return a UUID.
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>Job ID</returns>
        public async Task<Guid> SendBgApi(BgApiCommand command) {
            if (!Connected
                || !Authenticated)
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
            var command = new LogCommand(level);
            await SendAsync(command);
        }

        /// <summary>
        ///     ShutdownGracefully().
        ///     Send an exit command to FreeSwitch
        /// </summary>
        /// <returns>Async task</returns>
        public async Task ShutdownGracefully() {
            // Let us send exit command to FreeSwitch
            var exit = new ExitCommand();
            await SendAsync(exit);
        }

        /// <summary>
        ///     SubscribeToEvents()
        ///     It is used to subscribe to FreeSwitch events. Returns true when successful.
        /// </summary>
        /// <returns>boolean</returns>
        public async Task<bool> SubscribeToEvents() {
            if (!Connected
                || !Authenticated)
                return false;
            var command = new EventCommand(FreeSwitchEventFilter);
            var reply = await Send(command);
            return reply != null && reply.IsSuccessful;
        }

        /// <summary>
        ///     Authenticate().
        ///     It helps to send auth command to FreeSwitch using the provided password.
        /// </summary>
        /// <returns>Async task</returns>
        protected async Task Authenticate() {
            var command = new AuthCommand(Password);
            // Send the command
            var event2 = EnqueueCommand(command);
            await SendAsync(command);
            var response = await event2.Task as CommandReply;
            if (response == null) {
                await CloseAsync();
                return;
            }
            if (!response.IsSuccessful) {
                await CloseAsync();
                return;
            }
            _authenticated = true;
            if (!string.IsNullOrEmpty(FreeSwitchEventFilter)) await SubscribeToEvents();
        }

        /// <summary>
        ///     Used to dispatch .NET events
        /// </summary>
        protected void DispatchEvents(EslEventType eventType, EslEventArgs ea) {
            AsyncEventHandler<EslEventArgs> handler = null;
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
            try {
                handler(this, ea);
            }
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

        protected async Task HandleResponse(object item) {
            if (item == null) return;
            var @event = item as EslEvent;
            if (@event != null) {
                PopEvent(@event);
                return;
            }

            var reply = item as CommandReply;
            if (reply != null) {
                if (_requestsQueue.Count <= 0) return;
                CommandAsyncEvent event2;
                if (!_requestsQueue.TryDequeue(out event2)) return;
                event2?.Complete(reply);
                return;
            }

            var response = item as ApiResponse;
            if (response != null) {
                if (_requestsQueue.Count <= 0) return;
                CommandAsyncEvent event2;
                if (!_requestsQueue.TryDequeue(out event2)) return;
                event2?.Complete(response);
                return;
            }

            var notice = item as DisconnectNotice;
            if (notice != null) {
                if (OnDisconnectNotice != null)
                    await OnDisconnectNotice(this, new EslDisconnectNoticeEventArgs(notice)).ConfigureAwait(false);
                await _channel.CloseAsync();
                return;
            }

            var rejection = item as RudeRejection;
            if (rejection != null) {
                if (OnRudeRejection != null)
                    await OnRudeRejection(this, new EslRudeRejectionEventArgs(rejection)).ConfigureAwait(false);
                return;
            }

            var logdata = item as LogData;
            if (logdata != null) {
                //todo handle log/data
                return;
            }

            var msg = item as EslMessage;
            if (OnUnhandledMessage != null)
                await OnUnhandledMessage(this, new EslUnhandledMessageEventArgs(msg)).ConfigureAwait(false);
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
                    DispatchEvents(EslEventType.CHANNEL_HANGUP_COMPLETE,
                        new EslEventArgs(new ChannelHangup(@event.Items)));
                    break;
                case "CHANNEL_PROGRESS":
                    DispatchEvents(EslEventType.CHANNEL_PROGRESS, new EslEventArgs(new ChannelProgress(@event.Items)));
                    break;
                case "CHANNEL_PROGRESS_MEDIA":
                    DispatchEvents(EslEventType.CHANNEL_PROGRESS_MEDIA,
                        new EslEventArgs(new ChannelProgressMedia(@event.Items)));
                    break;
                case "CHANNEL_EXECUTE":
                    DispatchEvents(EslEventType.CHANNEL_EXECUTE, new EslEventArgs(new ChannelExecute(@event.Items)));
                    break;
                case "CHANNEL_EXECUTE_COMPLETE":
                    DispatchEvents(EslEventType.CHANNEL_EXECUTE_COMPLETE,
                        new EslEventArgs(new ChannelExecuteComplete(@event.Items)));
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
            if (message == null) throw new ArgumentNullException(nameof(message));
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
        ///     Connection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnect(object sender, SocketAsyncEventArgs e) {
            if (e.SocketError != SocketError.Success) {
                _connectException = new SocketException((int) e.SocketError);
                _socket = null;
            }
            else
                _channel.Assign(_socket);
            _connectSemaphore.Release();
        }

        /// <summary>
        ///     Disconnection event handler
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void OnDisconnect(ITcpChannel arg1, Exception arg2) {
            _socket = null;
            if (_sendCompletedSemaphore.CurrentCount == 0) {
                _sendException = arg2;
                _sendCompletedSemaphore.Release();
            }
        }

        /// <summary>
        ///     Fired when a decoded message is received by the channel.
        /// </summary>
        /// <param name="channel">Receiving channel</param>
        /// <param name="message">Decoded message received</param>
        private async void OnMessageReceived(ITcpChannel channel, object message) {
            var decodedMessage = message as EslDecodedMessage;
            // Handle decoded message.
            if (decodedMessage?.Headers == null
                || !decodedMessage.Headers.HasKeys())
                return;
            var headers = decodedMessage.Headers;
            object response = null;
            var contentType = headers["Content-Type"];
            if (string.IsNullOrEmpty(contentType)) return;
            contentType = contentType.ToLowerInvariant();
            switch (contentType) {
                case "auth/request":
                    if (OnAuthentication != null) await OnAuthentication(this, EventArgs.Empty).ConfigureAwait(false);
                    break;
                case "command/reply":
                    var reply = new CommandReply(headers, decodedMessage.OriginalMessage);
                    response = reply;
                    break;
                case "api/response":
                    var apiResponse = new ApiResponse(decodedMessage.BodyText);
                    response = apiResponse;
                    break;
                case "text/event-plain":
                    var parameters = decodedMessage.BodyLines.AllKeys.ToDictionary(key => key,
                        key => decodedMessage.BodyLines[key]);
                    var @event = new EslEvent(parameters);
                    response = @event;
                    break;
                case "log/data":
                    var logdata = new LogData(headers, decodedMessage.BodyText);
                    response = logdata;
                    break;
                case "text/rude-rejection":
                    await _channel.CloseAsync();
                    var reject = new RudeRejection(decodedMessage.BodyText);
                    response = reject;
                    break;
                case "text/disconnect-notice":
                    var notice = new DisconnectNotice(decodedMessage.BodyText);
                    response = notice;
                    break;
                default:
                    // Here we are handling an unknown message
                    var msg = new EslMessage(decodedMessage.Headers, decodedMessage.OriginalMessage);
                    response = msg;
                    break;
            }

            await HandleResponse(response).ConfigureAwait(false);
        }

        /// <summary>
        ///     Completed Send request event handler. Good for logging.
        /// </summary>
        /// <param name="channel">the Tcp channel used to send the request</param>
        /// <param name="sentMessage">the message sent</param>
        private void OnSendCompleted(ITcpChannel channel, object sentMessage) {
            if (_log.IsDebugEnabled) {
                var cmd = sentMessage as BaseCommand;
                _log.Debug("command sent to freeSwitch <<{0}>>", cmd.ToString());
            }

            _sendCompletedSemaphore.Release();
        }

        private async Task SendAuthentication(object sender, EventArgs e) {
            await Authenticate();
        }
    }
}