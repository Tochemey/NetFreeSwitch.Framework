.NET FreeSwitch Framework
========================================

## **Overview**
This framework helps interact with the FreeSwitch via its mod_event_socket. For more information about the mod_event_socket refer to [FreeSwitch web site](https://freeswitch.org/confluence/display/FREESWITCH/mod_event_socket). It is a bit different from the previous [library](https://github.com/Tochemey/FreeSwitch.ModEventSocket) I wrote in term of the organisation of the code base and the performance gained while using it. It is fully asynchronous. Also it offers more flexibility for extension by any other developer who picks the source code. 

This framework got genesis based upon the marvelous ground work done by Jonas Gauffin in its [framework](https://github.com/jgauffin/Griffin.Framework).

## **Features**
The framework in its current state can be used to interact with FreeSwitch easily in:
* Inbound mode[Event_Socket_Inbounds](https://freeswitch.org/confluence/display/FREESWITCH/mod_event_socket#mod_event_socket-Inbound)
* Outbound mode[Event_Socket_Outbound](https://wiki.freeswitch.org/wiki/Event_Socket_Outbound)
* One good thing it has is that you can implement your own FreeSwitch message encoder and decoder if you do not want to use the built-in ones

## **License**
[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt)

## **Installation and usage**
Currently you just need to clone it or download it in your project since it a visual studio project. Also you can copy the files into your project and continue fom there.

## **Example**
```c#
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NetFreeSwitch.Framework.FreeSwitch;
    using NetFreeSwitch.Framework.FreeSwitch.Commands;
    using NetFreeSwitch.Framework.FreeSwitch.Messages;
    using NetFreeSwitch.Framework.FreeSwitch.Outbound;
    using NLog;

    namespace Networking.Demo {
        internal class Program {
            private const string Address = "192.168.254.246";
            private const string Password = "ClueCon";
            private const int Port = 8021;
            public static Logger Log = LogManager.GetCurrentClassLogger();
            private static OutboundChannelSession client;

            private static void Main(string[] args) {
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


                EslLogLevels levels = EslLogLevels.INFO;
                client.SetLogLevel(levels);

                client.CloseAsync();

                Thread.Sleep(500);
                Log.Info("Connection Status {0}", client.Connected);
                Log.Info("Authentication Status {0}", client.Authenticated);

                Console.ReadKey();
            }
        }
    }

```