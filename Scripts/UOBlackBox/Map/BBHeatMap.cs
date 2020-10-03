using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Server.UOBlackBox
{
    public static class BBHeatMap
    {
        private static string SaveDirectory
        {
            get { return Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\HeatMap\\" + DateTime.Today.DayOfYear + "\\"; }
        }

        private static string LoadDirectory
        {
            get { return Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\HeatMap\\"; }
        }

        public static HeatTypes Heat_Type { get; set; }

        public enum HeatTypes
        {
            None = 0,
            CreatureDeath = 1,
            PlayerDeath = 2,
            PlayerMurder = 3,
            QuestSuccess = 4,
            ResourceSuccess = 5,
            TamingSuccess = 6,
            VendorBuy = 7,
            VendorSell = 8
        }

        public static void Initialize()
        {
            StartUp();

            IsLocked = 0;

            if (CreatureDeaths_Locations == null)
                CreatureDeaths_Locations = new List<string>();
            if (PlayerDeath_Locations == null)
                PlayerDeath_Locations = new List<string>();
            if (PlayerMurdered_Locations == null)
                PlayerMurdered_Locations = new List<string>();
            if (QuestComplete_Locations == null)
                QuestComplete_Locations = new List<string>();
            if (ResourceHarvested_Locations == null)
                ResourceHarvested_Locations = new List<string>();
            if (TameCreature_Locations == null)
                TameCreature_Locations = new List<string>();
            if (VendorBuy_Locations == null)
                VendorBuy_Locations = new List<string>();
            if (VendorSell_Locations == null)
                VendorSell_Locations = new List<string>();
        }

        private static void StartUp()
        {
            EventSink.AfterWorldSave += AfterWorldSave;

            EventSink.Crashed += CatchCrash;
            EventSink.Shutdown += CatchShutDown;

            EventSink.CreatureDeath += CreatureDeath_Record;
            EventSink.PlayerDeath += PlayerDeath_Record;
            EventSink.PlayerMurdered += PlayerMurdered_Record;

            EventSink.QuestComplete += QuestComplete_Record;
            EventSink.ResourceHarvestSuccess += ResourceHarvested_Record;
            EventSink.TameCreature += TameCreature_Record;

            EventSink.ValidVendorPurchase += VendorBuy_Record;
            EventSink.ValidVendorSell += VendorSell_Record;

            ServerStartUp();
        }

        private static void AfterWorldSave(AfterWorldSaveEventArgs e)
        {
            SaveToFile();

            ClearLists();
        }

        private static void CatchCrash(CrashedEventArgs e)
        {
            SaveToFile();
        }

        private static void CatchShutDown(ShutdownEventArgs e)
        {
            SaveToFile();
        }

        private static void ClearLists()
        {
            CreatureDeaths_Locations.Clear();
            PlayerDeath_Locations.Clear();
            PlayerMurdered_Locations.Clear();
            QuestComplete_Locations.Clear();
            ResourceHarvested_Locations.Clear();
            TameCreature_Locations.Clear();
            VendorBuy_Locations.Clear();
            VendorSell_Locations.Clear();
        }

        private static void ServerStartUp()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine("UO BlackBox : Heat Map => [STARTED] @ " + DateTime.Now);

            Console.ResetColor();
        }

        private static int IsLocked { get; set; }

        public static void SaveToFile()
        {
            IsLocked++;

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
            else
            {
                DirectoryInfo DI = new DirectoryInfo(SaveDirectory);

                if (DI.CreationTime.DayOfYear != DateTime.Today.DayOfYear)
                {
                    Directory.Delete(SaveDirectory);

                    Directory.CreateDirectory(SaveDirectory);
                }
            }

            if (IsLocked == 1)
            {
                SaveFile(CreatureDeaths_Locations, SaveDirectory + "CreatureDeaths.txt");

                SaveFile(PlayerDeath_Locations, SaveDirectory + "PlayerDeaths.txt");

                SaveFile(PlayerMurdered_Locations, SaveDirectory + "PlayerMurders.txt");

                SaveFile(QuestComplete_Locations, SaveDirectory + "QuestsCompleted.txt");

                SaveFile(ResourceHarvested_Locations, SaveDirectory + "ResourceHarvested.txt");

                SaveFile(TameCreature_Locations, SaveDirectory + "TameCreature.txt");

                SaveFile(VendorBuy_Locations, SaveDirectory + "VendorBuy.txt");

                SaveFile(VendorSell_Locations, SaveDirectory + "VendorSell.txt");

                IsLocked = 0;
            }
            else if (IsLocked > 3) //to unlock if it ever gets stuck!
            {
                IsLocked = 0;
            }
        }

        private static void SaveFile(List<string> heatmap, string filename)
        {
            using (StreamWriter writer = File.AppendText(filename))
            {
                foreach (var location in heatmap)
                {
                    writer.WriteLine(location);
                }
            }
        }

        public static string LoadFromFile(HeatTypes heatTypes, int day, string map)
        {
            string HeatMapData = "[EMPTY]";

            if (day < DateTime.Today.DayOfYear)
            {
                int Day_Offset = DateTime.Today.DayOfYear - day;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i <= Day_Offset; i++)
                {
                    int get_Day = day + i;

                    if (Directory.Exists(LoadDirectory + get_Day + "\\"))
                    {
                        sb.Append(GetHeatData(heatTypes, get_Day, map));
                    }
                }

                HeatMapData = sb.ToString();
            }
            else
            {
                if (Directory.Exists(LoadDirectory + day + "\\"))
                {
                    HeatMapData = GetHeatData(heatTypes, day, map);
                }
            }

            return HeatMapData;
        }

        private static string GetHeatData(HeatTypes heatTypes, int day, string map)
        {
            switch (heatTypes)
            {
                case HeatTypes.None:
                    return "[NONE]";
                case HeatTypes.CreatureDeath:
                    return LoadFile(LoadDirectory + day + "\\CreatureDeaths.txt", map);
                case HeatTypes.PlayerDeath:
                    return LoadFile(LoadDirectory + day + "\\PlayerDeaths.txt", map);
                case HeatTypes.PlayerMurder:
                    return LoadFile(LoadDirectory + day + "\\PlayerMurders.txt", map);
                case HeatTypes.QuestSuccess:
                    return LoadFile(LoadDirectory + day + "\\QuestsCompleted.txt", map);
                case HeatTypes.ResourceSuccess:
                    return LoadFile(LoadDirectory + day + "\\ResourceHarvested.txt", map);
                case HeatTypes.TamingSuccess:
                    return LoadFile(LoadDirectory + day + "\\TameCreature.txt", map);
                case HeatTypes.VendorBuy:
                    return LoadFile(LoadDirectory + day + "\\VendorBuy.txt", map);
                case HeatTypes.VendorSell:
                    return LoadFile(LoadDirectory + day + "\\VendorSell.txt", map);
                default:
                    return "[DEFAULT]";
            }
        }

        private static string LoadFile(string filename, string map)
        {
            StringBuilder GetFileData = new StringBuilder();

            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    if (line.Contains(map))
                        GetFileData.Append(line + ";");
                }
            }

            if (GetFileData.Length > 0)
                return GetFileData.ToString();
            else
                return "[NO_HEATMAP]";
        }

        public static List<string> CreatureDeaths_Locations { get; set; }

        private static void CreatureDeath_Record(CreatureDeathEventArgs e)
        {
            string Location_Data = e.Creature.Map + "*" + e.Creature.Location.X + "*" + e.Creature.Location.Y;

            if (!CreatureDeaths_Locations.Contains(Location_Data))
                CreatureDeaths_Locations.Add(Location_Data);
        }

        public static List<string> PlayerDeath_Locations { get; set; }

        private static void PlayerDeath_Record(PlayerDeathEventArgs e)
        {
            string Location_Data = e.Mobile.Map + "*" + e.Mobile.Location.X + "*" + e.Mobile.Location.Y;

            if (!PlayerDeath_Locations.Contains(Location_Data))
                PlayerDeath_Locations.Add(Location_Data);
        }

        public static List<string> PlayerMurdered_Locations { get; set; }

        private static void PlayerMurdered_Record(PlayerMurderedEventArgs e)
        {
            string Location_Data = e.Victim.Map + "*" + e.Victim.Location.X + "*" + e.Victim.Location.Y;

            if (!PlayerMurdered_Locations.Contains(Location_Data))
                PlayerMurdered_Locations.Add(Location_Data);
        }

        public static List<string> QuestComplete_Locations { get; set; }

        private static void QuestComplete_Record(QuestCompleteEventArgs e)
        {
            string Location_Data = e.Mobile.Map + "*" + e.Mobile.Location.X + "*" + e.Mobile.Location.Y;

            if (!QuestComplete_Locations.Contains(Location_Data))
                QuestComplete_Locations.Add(Location_Data);
        }

        public static List<string> ResourceHarvested_Locations { get; set; }

        private static void ResourceHarvested_Record(ResourceHarvestSuccessEventArgs e)
        {
            string Location_Data = e.Harvester.Map + "*" + e.Harvester.Location.X + "*" + e.Harvester.Location.Y;

            if (!ResourceHarvested_Locations.Contains(Location_Data))
                ResourceHarvested_Locations.Add(Location_Data);
        }

        public static List<string> TameCreature_Locations { get; set; }

        private static void TameCreature_Record(TameCreatureEventArgs e)
        {
            string Location_Data = e.Creature.Map + "*" + e.Creature.Location.X + "*" + e.Creature.Location.Y;

            if (!TameCreature_Locations.Contains(Location_Data))
                TameCreature_Locations.Add(Location_Data);
        }

        public static List<string> VendorBuy_Locations { get; set; }

        private static void VendorBuy_Record(ValidVendorPurchaseEventArgs e)
        {
            string Location_Data = e.Vendor.Map + "*" + e.Vendor.Location.X + "*" + e.Vendor.Location.Y;

            if (!VendorBuy_Locations.Contains(Location_Data))
                VendorBuy_Locations.Add(Location_Data);
        }

        public static List<string> VendorSell_Locations { get; set; }

        private static void VendorSell_Record(ValidVendorSellEventArgs e)
        {
            string Location_Data = e.Vendor.Map + "*" + e.Vendor.Location.X + "*" + e.Vendor.Location.Y;

            if (!VendorSell_Locations.Contains(Location_Data))
                VendorSell_Locations.Add(Location_Data);
        }
    }
}
