//UO Black Box - By GoldDraco13

using Server.Mobiles;
using Server.Items;
using System.IO;

namespace Server.UOBlackBox
{
    [Flipable(0x14F0, 0x14EF)]
    public class BBBlueprint : Item
    {
        private static string fileName = "";

        [Constructable]
        public BBBlueprint(string name) : base()
        {
            string file = name;
            fileName = file + ".BBBuild";

            Name = name + " - Blueprint";
            ItemID = 0x14F0;
            Hue = 91;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from is PlayerMobile)
            {
                PlayerMobile PM = from as PlayerMobile;

                try
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\Blueprints\\" + PM.Name + "\\" + fileName))
                    {
                        if (!IsChildOf(PM.Backpack))
                        {
                            LoadFromBlueprints(PM, Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\Blueprints\\" + PM.Name + "\\" + fileName, X, Y, Z, Map, Name);
                        }
                        else
                        {
                            LoadFromBlueprints(PM, Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\Blueprints\\" + PM.Name + "\\" + fileName, PM.X, PM.Y, PM.Z, PM.Map, Name);
                        }
                    }
                }
                catch
                {
                    PM.SendMessage("[Report] => Problem with Blueprint file loading!");
                }
            }
        }

        public BBBlueprint(Serial serial): base(serial)
        {
        }

        public static void LoadFromBlueprints(PlayerMobile pm, string filename, int X, int Y, int Z, Map map, string boxName)
        {
            if (filename == null)
            {
                return;
            }

            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line;
                    int linenumber = 0;

                    while ((line = sr.ReadLine()) != null)
                    {
                        linenumber++;

                        if (line.Length == 0) continue;

                        string[] specs = line.Split(';');

                        if (specs == null || specs.Length < 1) continue;

                        string[] args = specs[0].Trim().Split(' ');

                        if (args != null && args.Length >= 5)
                        {
                            int itemid = -1;
                            int x = 0;
                            int y = 0;
                            int z = 0;
                            int visible = 0;
                            int hue = -1;
                            int GetHue = 0;

                            try
                            {
                                itemid = int.Parse(args[0]);
                                x = int.Parse(args[1]);
                                y = int.Parse(args[2]);
                                z = int.Parse(args[3]);
                                visible = int.Parse(args[4]);

                                if (args.Length > 5)
                                {
                                    hue = int.Parse(args[5]);

                                    if (hue >= 0)
                                        GetHue = hue;
                                }

                                Static item = new Static(itemid)
                                {
                                    Name = boxName,
                                    Hue = GetHue
                                };

                                if (visible > 0)
                                    item.Visible = true;
                                else
                                    item.Visible = false;

                                Point3D pnt = new Point3D(X + x, Y + y, Z + z);
                                item.MoveToWorld(pnt, map);
                            }
                            catch
                            {
                            }
                        }
                    }
                    sr.Close();
                }
            }
            else
            {
                pm.SendMessage("File Missing");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);

            writer.Write(fileName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            fileName = reader.ReadString();
        }
    }
}
