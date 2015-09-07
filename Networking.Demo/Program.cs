using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Networking.Common.Net.Protocols.FreeSwitch.Command;
using Networking.Common.Net.Protocols.FreeSwitch.Message;
using Networking.Common.Net.Protocols.FreeSwitch.Outbound;
using NLog;

namespace Networking.Demo {
    internal class Program {
        private const string Address = "192.168.254.246";
        private const string Password = "ClueCon";
        private const int Port = 8021;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private static FreeSwitchClient client;

        private static void Main(string[] args) {
            client = new FreeSwitchClient(Address, Port, Password);
            client.Connect();

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


            Console.ReadKey();
        }
    }
}