using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetFreeSwitch.Framework.FreeSwitch;
using NetFreeSwitch.Framework.FreeSwitch.Commands;
using NetFreeSwitch.Framework.FreeSwitch.Messages;
using NetFreeSwitch.Framework.FreeSwitch.Outbound;
using NLog;
using NetFreeSwitch.Framework.FreeSwitch.Inbound;

namespace Networking.Demo {
    internal class Program {
        private const string Address = "127.0.0.1";
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

            server.Start(System.Net.IPAddress.Any, ServerPort);
            Thread.Sleep(500);

            client = new OutboundChannelSession(Address, Port, Password);
            client.ConnectAsync();

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
                if (gws != null
                    && gws.Any())
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
            string callCommand = "{ignore_early_media=true,originate_timeout=120}user/1000@127.0.0.1 &socket(127.0.0.1:10000 async full)";
            BgApiCommand bgapicommand = new BgApiCommand("originate", callCommand);
            Guid jobId = client.SendBgApi(bgapicommand).Result;
            Log.Info("Job Id {0}", jobId);


           // EslLogLevels levels = EslLogLevels.INFO;
           // client.SetLogLevel(levels);

           // client.CloseAsync();

            //Thread.Sleep(500);
            //Log.Info("Connection Status {0}", client.Connected);
            //Log.Info("Authentication Status {0}", client.Authenticated);

            Console.ReadKey();
        }

        private static void OnClientReady(object sender, InboundClientEventArgs e)
        {
            Log.Info("Connected client is ready");
        }
    }
}