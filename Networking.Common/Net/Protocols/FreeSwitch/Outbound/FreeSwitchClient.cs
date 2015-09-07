using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Networking.Common.Net.Protocols.FreeSwitch.Command;
using Networking.Common.Net.Protocols.FreeSwitch.Events;
using Networking.Common.Net.Protocols.FreeSwitch.Message;

namespace Networking.Common.Net.Protocols.FreeSwitch.Outbound {
    /// <summary>
    ///     Connects to FreeSwitch and execute commands.
    /// </summary>
    public class FreeSwitchClient {
        private readonly ChannelTcpClient _channelTcpClient;
        private static bool _authenticated;
        private static bool _connected;
        private ConcurrentQueue<CommandAsyncEvent> _requestsQueue;
        private readonly EventsListenInfo _eventsListener;

        public event EventHandler<EslEventArgs> OnChannelBridge = delegate { };
        public event EventHandler<EslEventArgs> OnChannelUnbridge = delegate { };
        public event EventHandler<EslEventArgs> OnCallUpdate = delegate { };
        public event EventHandler<EslEventArgs> OnChannelProgress = delegate { };
        public event EventHandler<EslEventArgs> OnChannelProgressMedia = delegate { };
        public event EventHandler<EslEventArgs> OnChannelState = delegate { };
        public event EventHandler<EslEventArgs> OnChannelPark = delegate { };
        public event EventHandler<EslEventArgs> OnChannelUnPark = delegate { };
        public event EventHandler<EslEventArgs> OnChannelExecute = delegate { };
        public event EventHandler<EslEventArgs> OnChannelExecuteComplete = delegate { };
        public event EventHandler<EslEventArgs> OnChannelHangup = delegate { };
        public event EventHandler<EslEventArgs> OnChannelHangupComplete = delegate { };
        public event EventHandler<EslEventArgs> OnSessionHeartbeat = delegate { };
        public event EventHandler<EslEventArgs> OnBackgroundJob = delegate { };
        public event EventHandler<EslEventArgs> OnDtmf = delegate { };
        public event EventHandler<EslEventArgs> OnRecordStop = delegate { };
        public event EventHandler<EslEventArgs> OnCustom = delegate { };
        public event EventHandler<EslEventArgs> OnChannelAnswer = delegate { };
        public event EventHandler<EslEventArgs> OnChannelOriginate = delegate { };
        public event EventHandler<EslEventArgs> OnReceivedUnHandledEvent = delegate { };


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
        public FreeSwitchClient(string address, int port, IMessageEncoder messageEncoder, IMessageDecoder messageDecoder, string freeSwitchEventFilter, TimeSpan connectionTimeout, string password) {
            Address = address;
            Port = port;
            MessageEncoder = messageEncoder;
            MessageDecoder = messageDecoder;
            FreeSwitchEventFilter = freeSwitchEventFilter;
            ConnectionTimeout = connectionTimeout;
            Password = password;
            _authenticated = false;
            _connected = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channelTcpClient = new ChannelTcpClient(MessageEncoder, MessageDecoder);
            _eventsListener = new EventsListenInfo();
        }

        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch using in-built message encoder/decoders
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="connectionTimeout">Connection Timeout</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        public FreeSwitchClient(string address, int port, TimeSpan connectionTimeout, string password) {
            Address = address;
            Port = port;
            ConnectionTimeout = connectionTimeout;
            Password = password;
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _connected = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channelTcpClient = new ChannelTcpClient(MessageEncoder, MessageDecoder);
            _eventsListener = new EventsListenInfo();
        }

        /// <summary>
        ///     This constructor sets user defined values to connect to FreeSwitch using in-built message encoder/decoders.
        ///     The connection timeout is set to zero
        /// </summary>
        /// <param name="address">FreeSwitch mod_event_socket IP address or hostname</param>
        /// <param name="port">FreeSwitch mod_event_socket Port number</param>
        /// <param name="password">FreeSwitch mod_event_socket Password</param>
        public FreeSwitchClient(string address, int port, string password) {
            Address = address;
            Port = port;
            ConnectionTimeout = TimeSpan.Zero;
            Password = password;
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _connected = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channelTcpClient = new ChannelTcpClient(MessageEncoder, MessageDecoder);
            _eventsListener = new EventsListenInfo();
        }

        /// <summary>
        ///     Default constructor. It uses the default FreeSwitch mod_event_socket settings for connectivity
        /// </summary>
        public FreeSwitchClient() {
            Address = "127.0.0.1";
            Port = 8021;
            ConnectionTimeout = TimeSpan.Zero;
            Password = "ClueCon";
            MessageEncoder = new FreeSwitchEncoder();
            MessageDecoder = new FreeSwitchDecoder();
            FreeSwitchEventFilter = "plain ALL";
            _authenticated = false;
            _connected = false;
            _requestsQueue = new ConcurrentQueue<CommandAsyncEvent>();
            _channelTcpClient = new ChannelTcpClient(MessageEncoder, MessageDecoder);
        }

        /// <summary>
        ///     Connect().
        ///     It used to connect to FreeSwitch mod_event_socket as a client.
        /// </summary>
        /// <returns></returns>
        public async Task Connect() {
            await _channelTcpClient.ConnectAsync(IPAddress.Parse(Address), Port, ConnectionTimeout);
            _connected = _channelTcpClient.IsConnected;
            if (_connected) {
                // Here we wait to receive an auth/request response from the server upon successful connection
                EslDecodedMessage request = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
                if (request == null
                    || request.Headers == null
                    || !request.Headers.HasKeys()) await _channelTcpClient.CloseAsync();

                // Let us get the auth/request data to be sure of
                NameValueCollection headers = request.Headers;
                string contentType = headers["Content-Type"];
                if (contentType == null
                    || string.IsNullOrEmpty(contentType)) await _channelTcpClient.CloseAsync();
                if (contentType.ToLower().Equals("auth/request")) {
                    // Now we can send an authentication request
                    AuthCommand authCommand = new AuthCommand(Password);
                    await _channelTcpClient.SendAsync(authCommand);

                    // Now let us receive the response
                    EslDecodedMessage response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
                    CommandReply reply = GetCommandReply(response);
                    if (reply == null) await _channelTcpClient.CloseAsync();
                    _authenticated = reply.IsSuccessful;
                    //if (!string.IsNullOrEmpty(FreeSwitchEventFilter))
                    //    if (await SubscribeToEvents()) _eventsListener.Start(Eavesdrop);
                }
            }
        }

        /// <summary>
        ///     Disconnect from FreeSwitch mod_event_socket
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect() {
            if (_connected && _channelTcpClient != null) {
                // Let us clean
                Clean();
                await _channelTcpClient.CloseAsync();
            }
        }

        /// <summary>
        ///     SendApi().
        ///     It is used to send an api command to FreeSwitch.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>ApiResponse response</returns>
        public async Task<ApiResponse> SendApi(ApiCommand command) {
            if (!_authenticated) return null;
            // Send the command
            EnqueueCommand(command);
            await _channelTcpClient.SendAsync(command);
            // Receive the response
            var response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
            return await ValidateResponse(command, GetApiResponse(response)) as ApiResponse;
        }

        /// <summary>
        ///     SubscribeToEvents()
        ///     It is used to subscribe to FreeSwitch events. Returns true when successful.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SubscribeToEvents() {
            if (!_authenticated) return false;
            EventCommand command = new EventCommand(FreeSwitchEventFilter);
            EnqueueCommand(command);
            await _channelTcpClient.SendAsync(command);
            var response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
            var reply = await ValidateResponse(command, GetCommandReply(response)) as CommandReply;
            return reply != null && reply.IsSuccessful;
        }

        /// <summary>
        ///     Send().
        ///     It sends a command to FreeSwitch and awaist for a CommandReply response
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>CommandReply response</returns>
        public async Task<CommandReply> Send(BaseCommand command) {
            if (!_authenticated) return null;
            // Send command
            EnqueueCommand(command);
            await _channelTcpClient.SendAsync(command);
            var response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
            return await ValidateResponse(command, GetCommandReply(response)) as CommandReply;
        }

        /// <summary>
        ///     SendBgApi().
        ///     It is used to send commands to FreeSwitch that will be executed in the background. It will return a UUID.
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns></returns>
        public async Task<Guid> SendBgApi(BgApiCommand command) {
            if (!_authenticated) return Guid.Empty;
            // Send command
            EnqueueCommand(command);
            await _channelTcpClient.SendAsync(command);
            var response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
            var reply = await ValidateResponse(command, GetCommandReply(response)) as CommandReply;
            if (reply == null) return Guid.Empty;
            if (!reply.IsSuccessful) return Guid.Empty;
            string jobId = reply["Job-UUID"];
            Guid jobUuid;
            return Guid.TryParse(jobId, out jobUuid) ? jobUuid : Guid.Empty;
        }

        /// <summary>
        ///     Get a command reply from the decoded response sent.
        /// </summary>
        /// <param name="decodedMessage">decoded message</param>
        /// <returns>Command Reply</returns>
        protected CommandReply GetCommandReply(EslDecodedMessage decodedMessage) {
            if (decodedMessage == null
                || decodedMessage.Headers == null
                || !decodedMessage.Headers.HasKeys()) return null;
            NameValueCollection headers = decodedMessage.Headers;
            string contentType = headers["Content-Type"];
            if (!string.IsNullOrEmpty(contentType)
                && contentType.ToLower().Equals("command/reply")) return new CommandReply(headers, decodedMessage.OriginalMessage);
            return null;
        }

        /// <summary>
        ///     Get an api/response from the decoded response sent.
        /// </summary>
        /// <param name="decodedMessage">decoded message</param>
        /// <returns>Api Response</returns>
        protected ApiResponse GetApiResponse(EslDecodedMessage decodedMessage) {
            if (decodedMessage == null
                || decodedMessage.Headers == null
                || !decodedMessage.Headers.HasKeys())
                return null;
            NameValueCollection headers = decodedMessage.Headers;
            string contentType = headers["Content-Type"];
            if (!string.IsNullOrEmpty(contentType)
                && contentType.ToLower().Equals("api/response")) return new ApiResponse(decodedMessage.BodyText);
            return null;
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
        /// <returns></returns>
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
        ///     Used to release resources
        /// </summary>
        protected void Clean() {
            Authenticated = false;
            _requestsQueue = null;
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
            catch {}
        }

        /// <summary>
        ///     FreeSwitch Events listener hook
        /// </summary>
        protected async void Eavesdrop() {
            // Set our thread information
            _eventsListener.ListenThread.IsBackground = true;
            _eventsListener.ListenThread.Name = string.Format("EventsListenerThread:{0}", Port);

            // Mark us as running
            _eventsListener.IsRunning = Connected && Authenticated;
            while (_eventsListener.IsRunning) {
                var response = await _channelTcpClient.ReceiveAsync<EslDecodedMessage>();
                NameValueCollection headers = response.Headers;
                string contentType = headers["Content-Type"];
                if (string.IsNullOrEmpty(contentType)) continue;
                contentType = contentType.Trim().ToLower();
                if (contentType.Equals("text/event-plain")) {
                    var eventData = response.BodyLines.AllKeys.ToDictionary(key => key, key => response.BodyLines[key]);
                    EslEvent @event = new EslEvent(eventData);
                    if (!string.IsNullOrEmpty(@event.EventName)) {
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
                }
            }
        }

        /// <summary>
        ///     Connection State
        /// </summary>
        public bool Connected { get { return _connected; } }

        /// <summary>
        ///     Authentication State
        /// </summary>
        public bool Authenticated { get { return _authenticated; } set { _authenticated = value; } }

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
        ///     Event Socket Address
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        ///     Event Socket Port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        ///     Event Socket Password
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        ///     FreeSwich Message Encoder
        /// </summary>
        public IMessageEncoder MessageEncoder { get; private set; }

        /// <summary>
        ///     FreeSwitch Message Decoder
        /// </summary>
        public IMessageDecoder MessageDecoder { get; private set; }

        /// <summary>
        ///     FreeSwitch Events Filter
        /// </summary>
        public string FreeSwitchEventFilter { get; private set; }

        /// <summary>
        ///     Connection Timeout
        /// </summary>
        public TimeSpan ConnectionTimeout { get; private set; }
    }
}