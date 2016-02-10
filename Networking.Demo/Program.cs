using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetFreeSwitch.Framework.FreeSwitch;
using NetFreeSwitch.Framework.FreeSwitch.Commands;
using NetFreeSwitch.Framework.FreeSwitch.Messages;
using NetFreeSwitch.Framework.FreeSwitch.Outbound;
using NLog;
using NetFreeSwitch.Framework.FreeSwitch.Inbound;

namespace Networking.Demo {
    internal class Program {
        private const string Address = "192.168.74.128";
        private const string Password = "ClueCon";
        private const int Port = 8021;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private static OutboundChannelSession client;
        private static FreeSwitchServer server;

        private const int ServerPort = 10000;

        private static void Main(string[] args) {

            // Let us start the outboud server
            server = new FreeSwitchServer();
            server.ClientReady += OnClientReady;

            server.Start(IPAddress.Parse("192.168.74.1"), ServerPort);
            Thread.Sleep(500);

            string evenlist = "BACKGROUND_JOB  CHANNEL_PROGRESS  CHANNEL_PROGRESS_MEDIA  CHANNEL_HANGUP_COMPLETE  CHANNEL_STATE SESSION_HEARTBEAT  CALL_UPDATE RECORD_STOP  CHANNEL_BRIDGE  CHANNEL_UNBRIDGE  CHANNEL_ANSWER  CHANNEL_ORIGINATE CHANNEL_EXECUTE  CHANNEL_EXECUTE_COMPLETE";
            client = new OutboundChannelSession(Address, Port, Password, evenlist);
            client.OnChannelProgressMedia += OnChannelProgressMedia;
            client.OnChannelState += OnChannelState;
            bool connected = Connect().Result;
            if (connected) {
                Thread.Sleep(500);

                Log.Info("Connection Status {0}", client.Connected);
                Log.Info("Authentication Status {0}", client.Authenticated);

                var command = new ApiCommand("sofia profile external gwlist up");
                ApiResponse response = client.SendApi(command).Result;
                if (response == null) Log.Info("No gateways available");
                else if (string.IsNullOrEmpty(response.Body)) Log.Info("No gateways available");
                else
                {
                    List<string> gws = response.Body.Split().ToList();
                    if (gws.Any())
                    {
                        var counter = 1;
                        foreach (var gw in gws) Log.Info("Gateway no {0} : {1}", counter++, gw);
                    }
                }

                command = new ApiCommand("sofia status");
                response = client.SendApi(command).Result;
                Log.Info("sofia status :\n");
                Log.Info("\n\n" + response.Body);

                // Let us make a call and handle it
                string callCommand = "{ignore_early_media=false,originate_timeout=120}sofia/gateway/smsghlocalsip/233247063817 &socket(192.168.74.1:10000 async full)";
                BgApiCommand bgapicommand = new BgApiCommand("originate", callCommand);
                Guid jobId = client.SendBgApi(bgapicommand).Result;
                Log.Info("Job Id {0}", jobId);

                Thread.Sleep(500);

                // EslLogLevels levels = EslLogLevels.INFO;
                // client.SetLogLevel(levels);

                // client.CloseAsync();

                //Thread.Sleep(500);
                //Log.Info("Connection Status {0}", client.Connected);
                //Log.Info("Authentication Status {0}", client.Authenticated);

            }
            
            Console.ReadKey();
        }

        private static async Task OnChannelState(object sender, EslEventArgs e) {
            Log.Info("Client Received Channel State event {0}", e.EventReceived.ToString());
            await Task.Delay(0);
        }


        private static async Task<bool> Connect() {
            await client.ConnectAsync();
            return true;
        }

        private static async Task OnChannelProgressMedia(object sender, EslEventArgs e) {
            Log.Info("Client Received Channel Progress Media event {0}", e.EventReceived.ToString());
            await Task.Delay(0);
        }

        private static void OnClientReady(object sender, InboundClientEventArgs e) {
            var freeswitch = e.Freeswitch;
            freeswitch.OnDisconnectNotice += OnDisconnectNotice;
            Log.Info("Connected client is ready");
        }

        private static async Task OnDisconnectNotice(object sender, EslDisconnectNoticeEventArgs e) {
            Log.Info("Client disconnect {0}",  e.Notice.Msg);
            await Task.Delay(0);
        }
    }
}
