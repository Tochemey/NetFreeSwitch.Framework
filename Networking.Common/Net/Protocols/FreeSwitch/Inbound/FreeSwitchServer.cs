using System;
using System.Collections.Concurrent;
using System.Net;
using Networking.Common.Net.Channels;

namespace Networking.Common.Net.Protocols.FreeSwitch.Inbound {
    /// <summary>
    /// </summary>
    public class FreeSwitchServer {
        private readonly ConcurrentDictionary<string, FreeSwitch> _clients = new ConcurrentDictionary<string, FreeSwitch>();
        private readonly FreeSwitchListener _listener;

        public FreeSwitchServer() {
            var config = new ChannelTcpListenerConfiguration(() => new FreeSwitchDecoder(), () => new FreeSwitchEncoder());
            _listener = new FreeSwitchListener(config) {
                MessageReceived = OnClientMessage
            };
            _listener.ClientConnected += OnClientConnect;
            _listener.ClientDisconnected += OnClientDisconnect;
        }

        /// <summary>
        ///     Port that we got assigned (or specified)
        /// </summary>
        public int LocalPort { get { return _listener.LocalPort; } }

        public void Start(IPAddress address, int port) { _listener.Start(address, port); }

        public void Stop() {
            _listener.Stop();
            _clients.Clear();
        }

        private async void OnClientConnect(object sender, ClientConnectedEventArgs e) {
            ITcpChannel channel = e.Channel;
            FreeSwitch freeSwitch = new FreeSwitch(channel);
            await freeSwitch.Connect();
            bool ready = await freeSwitch.Resume() && await freeSwitch.MyEvents() && await freeSwitch.DivertEvents();
            if (!ready) {
                await freeSwitch.Close();
                return;
            }
            try { _clients.TryAdd(channel.ChannelId, freeSwitch); }
            catch (Exception) {
                if (channel != null) channel.Close();
            }
        }

        private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs e) {
            ITcpChannel channel = e.Channel;
            Exception exception = e.Exception;
            FreeSwitch freeSwitch;
            if (exception != null) {
                if (_clients.TryGetValue(channel.ChannelId, out freeSwitch)) freeSwitch.Error(channel, exception);
                return;
            }

            try { _clients.TryRemove(channel.ChannelId, out freeSwitch); }
            catch (ArgumentNullException) {
                // ignored
            }
        }

        private void OnClientMessage(ITcpChannel channel, object message) {
            // Let us get the actual channel to send the message to
            try {
                FreeSwitch freeSwitch;
                if (_clients.TryGetValue(channel.ChannelId, out freeSwitch)) freeSwitch.MessageReceived(channel, message);
            }
            catch (Exception) {
                // ignored
            }
        }
    }
}