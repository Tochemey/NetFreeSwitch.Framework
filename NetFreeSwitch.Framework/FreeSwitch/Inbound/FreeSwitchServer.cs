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
using System.Net;
using NetFreeSwitch.Framework.FreeSwitch.Codecs;
using NetFreeSwitch.Framework.Net;
using NetFreeSwitch.Framework.Net.Channels;
using NetFreeSwitch.Framework.Net.Protocols;
using NLog;

namespace NetFreeSwitch.Framework.FreeSwitch.Inbound {
    /// <summary>
    /// FreeSwitch Server.
    /// </summary>
    public class FreeSwitchServer {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, FreeSwitch> _clients = new ConcurrentDictionary<string, FreeSwitch>();
        private readonly FreeSwitchListener _listener;
        public event EventHandler<InboundClientEventArgs> ClientReady;

        public FreeSwitchServer() {
            var config = new ChannelTcpListenerConfiguration(() => new FreeSwitchDecoder(), () => new FreeSwitchEncoder());
            _listener = new FreeSwitchListener(config);
            _listener.MessageReceived += OnClientMessage;
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
            FreeSwitch freeSwitch = new FreeSwitch(ref channel);
            try {
                var added = _clients.TryAdd(channel.ChannelId, freeSwitch);
                if (added) {
                    await freeSwitch.Connect();
                    bool ready = await freeSwitch.Resume() && await freeSwitch.MyEvents() && await freeSwitch.DivertEvents();
                    if (!ready) {
                        await freeSwitch.Close();
                        return;
                    }
                }
                if (ClientReady != null) ClientReady(this, new InboundClientEventArgs(freeSwitch));
            }
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
            EslDecodedMessage decoded = message as EslDecodedMessage;
            _log.Debug("Received a new message from Client [#{0}--Address={1}] ", channel.ChannelId, channel.RemoteEndpoint);
            _log.Debug("Message :[{0}]", decoded.OriginalMessage);
            try {
                FreeSwitch freeSwitch;
                var fetched = _clients.TryGetValue(channel.ChannelId, out freeSwitch);
                if (fetched) freeSwitch.MessageReceived(channel, message);
            }
            catch (Exception) {
                // ignored
            }
        }
    }
}