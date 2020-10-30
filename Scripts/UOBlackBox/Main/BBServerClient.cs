//UO Black Box - By GoldDraco13

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Server.UOBlackBox
{
    public static class BBServerClient
    {
        static readonly string Host = "tcp://localhost";
        static readonly int Port = 8090;
        
        static UOBlackBoxService.UOBlackBoxService _RemoteService { get; set; }

        static TcpChannel _Channel { get; set; }

        public static bool ServerIsRunning { get; set; }

        public static void Initialize()
        {
            try
            {
                _RemoteService = new UOBlackBoxService.UOBlackBoxService();

                _Channel = new TcpChannel(Port);

                ChannelServices.RegisterChannel(_Channel, false);

                RemotingConfiguration.RegisterWellKnownServiceType(typeof(UOBlackBoxService.UOBlackBoxService), "RegisterCommand", WellKnownObjectMode.Singleton);

                BBServerMessage.WriteConsoleColored(ConsoleColor.DarkGray, "UO Black Box ~ Remote Service : Started @ " + DateTime.Now);

                // Show the URIs associated with the channel.
                ChannelDataStore data = (ChannelDataStore)_Channel.ChannelData;
                foreach (string uri in data.ChannelUris)
                {
                    if (uri.Contains(":"))
                    {
                        string[] GetConnection = uri.Substring(6).Split(':');

                        if (GetConnection.Length > 0)
                            BBServerMessage.WriteConsoleColored(ConsoleColor.Cyan, "UO Black Box ~ Remote Service : Listening @ [IP = " + GetConnection[0] + "] : [PORT = " + GetConnection[1] + "]");
                    }
                }

                UOBlackBoxService.UOBlackBoxService.CommandCall += CallServer;

                ServerIsRunning = true;
            }
            catch(Exception ex)
            {
                BBServerMessage.LogPacketCMD("ERROR", "UO Black Box Service : Failed : " + ex.Message, true);
            }
        }

        private static void CallServer(object sender, UOBlackBoxService.CommandCallArgs e)
        {
            ReceiveRemoteCmd(e.Command_Arg);
        }

        public static void SendCommand(BBPacket packet)
        {
            if (_RemoteService.ReturnDataList != null)
                _RemoteService.ReturnDataList.Add($"{packet.Name}:{(int)packet.CommandType}:{packet.Command}:{packet.Data}:{packet.Args}");
        }

        private static void ReceiveRemoteCmd(string reply)
        {
            BBProcessReplyCommand.ReceiveRemoteCmd(reply);
        }
    }
}
