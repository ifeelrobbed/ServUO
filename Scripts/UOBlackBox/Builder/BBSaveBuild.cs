//Original Script by ArteGordon
//UO Black Box - Revised - By GoldDraco13

using System.IO;
using System.Collections;
using System.Text;
using Server.Items;
using Server.Commands;

namespace Server.UOBlackBox
{
    public class BBSaveBuild
    {
        private class TileEntry
        {
            public int ID;
            public int X;
            public int Y;
            public int Z;

            public TileEntry(int id, int x, int y, int z)
            {
                ID = id;
                X = x;
                Y = y;
                Z = z;
            }
        }
        private static readonly string SaveBuildDir = Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\Blueprints\\";

        public static void Initialize()
        {
            CommandSystem.Register("BBSaveBuild", AccessLevel.GameMaster, new CommandEventHandler(BBSaveBuild_OnCommand));
        }

        [Usage("BBSaveBuild <MultiFile> [zmin zmax]")]
        [Description("Creates a UO Build text file from the objects within the targeted area!.")]
        public static void BBSaveBuild_OnCommand(CommandEventArgs e)
        {
            if (e == null || e.Mobile == null)
                return;

            if (e.Mobile.AccessLevel < AccessLevel.GameMaster)
            {
                e.Mobile.SendMessage("You do not have rights to perform this command.");
                return;
            }

            if (e.Arguments != null && e.Arguments.Length < 1)
            {
                e.Mobile.SendMessage("Usage:  {0} <MultiFile> [zmin zmax]", e.Command);
                return;
            }
            string filename = e.Arguments[0].ToString();

            int zmin = int.MinValue;
            int zmax = int.MinValue;

            if (e.Arguments.Length > 1)
            {
                int index = 1;
                while (index < e.Arguments.Length)
                {
                    try
                    {
                        zmin = int.Parse(e.Arguments[index++]);
                        zmax = int.Parse(e.Arguments[index++]);
                    }
                    catch
                    {
                        e.Mobile.SendMessage("{0} : Invalid zmin zmax arguments", e.Command);
                        return;
                    }
                }
            }

            string dirname;

            string SaveBuildDirName = SaveBuildDir + "\\" + e.Mobile.Name + "\\";

            if (Directory.Exists(SaveBuildDirName) && filename != null && !filename.StartsWith("/") && !filename.StartsWith("\\"))
            {
                dirname = string.Format("{0}{1}.BBBuild", SaveBuildDirName, filename);
            }
            else
            {
                Directory.CreateDirectory(SaveBuildDirName);

                dirname = string.Format("{0}{1}.BBBuild", SaveBuildDirName, filename);
            }

            if (File.Exists(dirname))
            {
                try
                {
                    string line = "";

                    using (StreamReader op = new StreamReader(dirname, false))
                    {
                        if (op == null)
                        {
                            e.Mobile.SendMessage("Cannot access file {0}", dirname);
                            return;
                        }

                        line = op.ReadLine();
                    }

                    if (line != null && line.Length > 0)
                    {
                        string[] args = line.Split(" ".ToCharArray(), 3);

                        if (args == null || args.Length < 3)
                        {
                            e.Mobile.SendMessage("Cannot overwrite file {0} : not owner", dirname);
                            return;
                        }

                        if (args[2] != e.Mobile.Name)
                        {
                            e.Mobile.SendMessage("Cannot overwrite file {0} : not owner", dirname);
                            return;
                        }
                    }
                    else
                    {
                        e.Mobile.SendMessage("Cannot overwrite file {0} : not owner", dirname);
                        return;
                    }
                }
                catch
                {
                    e.Mobile.SendMessage("Cannot overwrite file {0}", dirname);
                    return;
                }

            }
            DefineMultiArea(e.Mobile, dirname, zmin, zmax);
        }

        public static void DefineMultiArea(Mobile m, string dirname, int zmin, int zmax)
        {
            object[] multiargs = new object[3];
            multiargs[0] = dirname;
            multiargs[1] = zmin;
            multiargs[2] = zmax;

            BoundingBoxPicker.Begin(m, new BoundingBoxCallback(DefineMultiArea_Callback), multiargs);
        }

        private static void DefineMultiArea_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
        {
            object[] multiargs = (object[])state;

            if (from != null && multiargs != null && map != null)
            {
                string dirname = (string)multiargs[0];
                int zmin = (int)multiargs[1];
                int zmax = (int)multiargs[2];
                bool includeitems = true;
                bool includestatics = true;
                bool includeinvisible = true;
                bool includemultis = true;

                ArrayList itemlist = new ArrayList();
                ArrayList staticlist = new ArrayList();
                ArrayList tilelist = new ArrayList();

                int sx = (start.X > end.X) ? end.X : start.X;
                int sy = (start.Y > end.Y) ? end.Y : start.Y;
                int ex = (start.X < end.X) ? end.X : start.X;
                int ey = (start.Y < end.Y) ? end.Y : start.Y;

                if (includeitems)
                {
                    IPooledEnumerable eable = map.GetItemsInBounds(new Rectangle2D(sx, sy, ex - sx + 1, ey - sy + 1));

                    foreach (Item item in eable)
                    {
                        if (item.Parent == null && (zmin == int.MinValue || (item.Location.Z >= zmin && item.Location.Z <= zmax)))
                        {
                            if ((includeinvisible || item.Visible) && (item.ItemID <= 65535))
                            {
                                itemlist.Add(item);
                            }
                        }
                    }
                    eable.Free();

                    int searchrange = 100;

                    eable = map.GetItemsInBounds(new Rectangle2D(sx - searchrange, sy - searchrange, ex - sy + searchrange * 2 + 1, ey - sy + searchrange * 2 + 1));

                    foreach (Item item in eable)
                    {
                        if (item.Parent == null)
                        {
                            if (item is BaseMulti && includemultis)
                            {
                                MultiComponentList mcl = ((BaseMulti)item).Components;
                                if (mcl != null && mcl.List != null)
                                {
                                    for (int i = 0; i < mcl.List.Length; i++)
                                    {
                                        MultiTileEntry t = mcl.List[i];

                                        int x = t.m_OffsetX + item.X;
                                        int y = t.m_OffsetY + item.Y;
                                        int z = t.m_OffsetZ + item.Z;
                                        int itemID = t.m_ItemID & 0xFFFF;

                                        if (x >= sx && x <= ex && y >= sy && y <= ey && (zmin == int.MinValue || (z >= zmin && z <= zmax)))
                                        {
                                            tilelist.Add(new TileEntry(itemID, x, y, z));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    eable.Free();
                }

                if (includestatics)
                {
                    for (int x = sx; x < ex; x++)
                    {
                        for (int y = sy; y < ey; y++)
                        {
                            StaticTile[] statics = map.Tiles.GetStaticTiles(x, y, false);

                            for (int j = 0; j < statics.Length; j++)
                            {
                                if ((zmin == int.MinValue || (statics[j].Z >= zmin && statics[j].Z <= zmax)))
                                {
                                    staticlist.Add(new TileEntry(statics[j].ID & 0xFFFF, x, y, statics[j].Z));
                                }
                            }
                        }
                    }
                }

                int nstatics = staticlist.Count;
                int nitems = itemlist.Count;
                int ntiles = tilelist.Count;

                int ntotal = nitems + nstatics + ntiles;

                int ninvisible = 0;
                int nmultis = ntiles;

                foreach (Item item in itemlist)
                {
                    int x = item.X - from.X;
                    int y = item.Y - from.Y;
                    int z = item.Z - from.Z;

                    if (item.ItemID > 65535)
                    {
                        nmultis++;
                    }
                    if (!item.Visible)
                    {
                        ninvisible++;
                    }
                }

                try
                {
                    StreamWriter op = new StreamWriter(dirname, false);
                    StringBuilder sb = new StringBuilder();

                    string convertString = "#:" + dirname + ":"; 
                    sb.Append(convertString);

                    if (sb != null)
                    {
                        op.WriteLine("1 version {0}", from.Name);
                        convertString = ("1 version " + from.Name + "?");
                        sb.Append(convertString);

                        op.WriteLine("{0} num components", ntotal);
                        convertString = (ntotal + " num components?");
                        sb.Append(convertString);

                        foreach (Item item in itemlist)
                        {
                            int x = item.X - from.X;
                            int y = item.Y - from.Y;
                            int z = item.Z - from.Z;
                            int Vis = item.Visible ? 1 : 0;

                            if (item.Hue > 0)
                            {
                                op.WriteLine("{0} {1} {2} {3} {4} {5}", item.ItemID, x, y, z, item.Visible ? 1 : 0, item.Hue);
                                convertString = (item.ItemID + " " + x + " " + y + " " + z + " " + Vis + " " + item.Hue + "?");
                                sb.Append(convertString);
                            }
                            else
                            {
                                op.WriteLine("{0} {1} {2} {3} {4}", item.ItemID, x, y, z, item.Visible ? 1 : 0);
                                convertString = (item.ItemID + " " + x + " " + y + " " + z + " " + Vis + "?");
                                sb.Append(convertString);
                            }
                        }

                        if (includestatics)
                        {
                            foreach (TileEntry s in staticlist)
                            {
                                int x = s.X - from.X;
                                int y = s.Y - from.Y;
                                int z = s.Z - from.Z;
                                int ID = s.ID;
                                op.WriteLine("{0} {1} {2} {3} {4}", ID, x, y, z, 1);
                                convertString = (ID + " " + x + " " + y + " " + z + " " + "1" + "?");
                                sb.Append(convertString);
                            }
                        }

                        if (includemultis)
                        {
                            foreach (TileEntry s in tilelist)
                            {
                                int x = s.X - from.X;
                                int y = s.Y - from.Y;
                                int z = s.Z - from.Z;
                                int ID = s.ID;
                                op.WriteLine("{0} {1} {2} {3} {4}", ID, x, y, z, 1);
                                convertString = (ID + " " + x + " " + y + " " + z + " " + "1" + "?");
                                sb.Append(convertString);
                            }
                        }
                    }
                    op.Close();
                    //BlackBoxSender.SendBBCMD(sb.ToString(), from); //TODO: Return copy to Black Box
                }
                catch
                {
                    from.SendMessage("Error writing multi file {0}", dirname);
                    return;
                }
                from.SendMessage(66, "UO Black Box - UO Builder Results:");
                from.SendMessage(66, "Included {0} items", nitems);
                from.SendMessage(66, "{0} multis", nmultis);
                from.SendMessage(66, "{0} invisible", ninvisible);
                from.SendMessage(66, "Included {0} statics", nstatics);
                from.SendMessage(66, "File Saved!");
            }
        }
    }
}
