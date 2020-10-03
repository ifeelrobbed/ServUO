//UO Black Box - By GoldDraco13

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using Server.OneTime.Events;

namespace Server.UOBlackBox
{
    public static class BBServerClient
    {
        static readonly string Host = "tcp://localhost";
        static readonly int Port = 8090;

        static TcpChannel _Channel { get; set; }

        static IUOBlackBoxService.IUOBlackBoxService _Client { get; set; }
        static UOBlackBoxService.UOBlackBoxService _RemoteService { get; set; }

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

                Counter = 0;
                IsLocked = 0;

                OneTimeMilliEvent.MilliTimerTick += SendCommandTick;

                UOBlackBoxService.UOBlackBoxService.CommandCall += CallServer;

                ServerIsRunning = true;
            }
            catch(Exception ex)
            {
                BBServerMessage.LogPacketCMD("ERROR", "UO Black Box Service : Failed : " + ex.Message, true);
            }
        }

        private static void CallServer(object sender, EventArgs e)
        {
            Counter = 50; //TODO: Testing to see if this works well before deciding to edit/change/remove it!

            SendCommand(new BBPacket("SERVER", BBPacket.CommandTypes.GENERIC_COMMAND, "Check_Client_Commands", "", "GET_COMMAND"));
        }

        private static int Counter { get; set; }

        private static void SendCommandTick(object sender, EventArgs e)
        {
            if (Counter > 99)
            {
                SendCommand(new BBPacket("SERVER", BBPacket.CommandTypes.GENERIC_COMMAND, "Check_Client_Commands", "", "GET_COMMAND"));

                Counter = 0;
            }
            else
            {
                Counter++;
            }
        }

        private static int IsLocked { get; set; }

        public static void SendCommand(BBPacket packet)
        {
            try
            {
                if (packet.Name == "SERVER")
                    IsLocked++;
                else
                    IsLocked = 1;

                if (IsLocked == 1)
                {
                    _Client = (IUOBlackBoxService.IUOBlackBoxService)Activator.GetObject(typeof(IUOBlackBoxService.IUOBlackBoxService), Host + ":" + Port + "/RegisterCommand");

                    //ReceiveRemoteCmd(_Client.RegisterCommand(packet.Passcode, packet.Name, (int)packet.CommandType, packet.Command, packet.Data, packet.Args)); //Old Ref (Don't Remove)

                    if (packet.Name != "SERVER")
                        BBServerMessage.LogPacketCMD(packet.Name, "SEND_DATA : " + packet.Command, false);

                    Task<string> RetreiveCMD = Task.Run(() => _Client.RegisterCommand(packet.Passcode, packet.Name, (int)packet.CommandType, packet.Command, packet.Data, packet.Args));

                    Task.WaitAll(RetreiveCMD);

                    System.Runtime.CompilerServices.TaskAwaiter<string> _Reply = RetreiveCMD.GetAwaiter();

                    string Result = _Reply.GetResult();

                    ReceiveRemoteCmd(Result);

                    IsLocked = 0;
                }
                else if (IsLocked > 50)
                {
                    IsLocked = 0; //to auto unlock as there might be a freeze up happening!
                }
                else
                {
                    //do nothing
                }
            }
            catch (Exception ex)
            {
                BBServerMessage.LogPacketCMD("ERROR", "UO Black Box Service : Failed : " +ex.Message, true);

                IsLocked = 0;
            }
        }

        private static void ReceiveRemoteCmd(string reply)
        {
            BBProcessReplyCommand.ReceiveRemoteCmd(reply);
        }
    }
}
