//UO Black Box - By GoldDraco13

using System;

namespace Server.UOBlackBox
{
    public class BBCleanUp
    {
        public void Initialize()
        {
            EventSink.Logout += new LogoutEventHandler(OnLogout);
        }

        private void OnLogout(LogoutEventArgs e)
        {
            int cnt = 0;

            foreach (Item item in World.Items.Values)
            {
                if (item.Name != null)
                {
                    if (item.Name.Contains("Black Box Portal") || item.Name.Contains("Black Box SoulStone"))
                    {
                        BBTravel BBT = item as BBTravel;

                        if (BBT.Owner == e.Mobile.Name)
                        {
                            item.Delete();

                            cnt++;
                        }
                    }

                    if (item.Name.Contains("Black Box Portal Exit"))
                    {
                        BBTravelEnd BBT = item as BBTravelEnd;

                        if (BBT.Owner == e.Mobile.Name)
                        {
                            item.Delete();

                            cnt++;
                        }
                    }
                }
            }
        }
    }
}
