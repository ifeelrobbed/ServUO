//UO Black Box - By GoldDraco13

using Server.Mobiles;

namespace Server.UOBlackBox
{
    public static class BBClientShutDown
    {
        public static void Initialize()
        {
            EventSink.Logout += EventSink_Logout;
        }

        private static void EventSink_Logout(LogoutEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;

            SHUTDOWN(pm);
        }

        private static void SHUTDOWN(PlayerMobile player)
        {
            BBServerClient.SendCommand(new BBPacket(player.Name, BBPacket.CommandTypes.SPECIAL_COMMAND, "SHUTDOWN", "", "SET_DATA"));
        }
    }
}
