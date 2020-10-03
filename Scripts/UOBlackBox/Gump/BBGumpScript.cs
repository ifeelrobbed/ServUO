//UO Black Box - By GoldDraco13

using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.UOBlackBox
{
    public class BBGumpScript
    {
        private static string SaveDirectory
        {
            get { return Directory.GetCurrentDirectory() + "\\UOBlackBox\\OUTPUT\\Gumps\\"; }
        }

        public BBGumpScript(PlayerMobile pm, string name, List<BBGumpItem> Elements)
        {
            if (!Directory.Exists(SaveDirectory + pm.Name + "\\"))
                Directory.CreateDirectory(SaveDirectory + pm.Name + "\\");
            
            Console.WriteLine("UO Black Box [Gump Editor] Saving to => " + SaveDirectory + pm.Name + "\\");

            try
            {
                if (name != "" && Elements.Count > 0)
                    WriteGump(pm, name, Elements);
            }
            catch
            {
                pm.SendMessage("[Report] => Problem with writing gump script!");
            }
        }

        private static void WriteGump(PlayerMobile pm, string name, List<BBGumpItem> Elements)
        {
            using (StreamWriter writer = File.CreateText(SaveDirectory + pm.Name + "\\" + name + ".cs"))
            {
                writer.WriteLine(@" //UO Black Box - By GoldDraco13");
                writer.WriteLine(@"//Gump Editor[Generic Script]");
                writer.WriteLine(@"");
                writer.WriteLine(@"using Server.Gumps;");
                writer.WriteLine(@"using Server.Mobiles;");
                writer.WriteLine(@"using Server.Network;");
                writer.WriteLine(@"");
                writer.WriteLine(@"namespace Server.UOBlackBox");
                writer.WriteLine(@"{");
                writer.WriteLine(@"    class " + name + " : Gump");
                writer.WriteLine(@"    {");
                writer.WriteLine(@"        public " + name + "() : base(0, 0)");
                writer.WriteLine(@"        {");
                writer.WriteLine(@"            //Main Gump Controls");
                writer.WriteLine(@"            Disposable = true;");
                writer.WriteLine(@"            Closable = true;");
                writer.WriteLine(@"            Resizable = true;");
                writer.WriteLine(@"            Dragable = true;");
                writer.WriteLine(@"");
                writer.WriteLine(@"            AddPage(0);");
                writer.WriteLine(@"");

                foreach (BBGumpItem G_Item in Elements)
                {
                    writer.WriteLine("            " + GetElementString((int)G_Item.GumpType, G_Item));
                }

                writer.WriteLine(@"        }");
                writer.WriteLine(@"");
                writer.WriteLine(@"        //*WARNING* Below is for reference only, you'll need to edit the script to suit your needs before using!");
                writer.WriteLine(@"");
                writer.WriteLine(@"        public override void OnResponse(NetState sender, RelayInfo info)");
                writer.WriteLine(@"        {");
                writer.WriteLine(@"            //PlayerMobile pm = sender.Mobile as PlayerMobile;");
                writer.WriteLine(@"");
                writer.WriteLine(@"            //switch (info.ButtonID)");
                writer.WriteLine(@"            //{");
                writer.WriteLine(@"                //case 0:");
                writer.WriteLine(@"                    //{");
                writer.WriteLine("                        //pm.SendMessage(\"Case: 0\");");
                writer.WriteLine(@"                        //break;");
                writer.WriteLine(@"                    //}");
                writer.WriteLine(@"                //default:");
                writer.WriteLine(@"                    //{");
                writer.WriteLine("                        //pm.SendMessage(\"Case: Default\");");
                writer.WriteLine(@"                        //break;");
                writer.WriteLine(@"                    //}");
                writer.WriteLine(@"            }");
                writer.WriteLine(@"        }");
                writer.WriteLine(@"    }");
                writer.WriteLine(@"}");
            }
        }

        private static string GetElementString(int element, BBGumpItem G_Item)
        {
            switch (element)
            {
                case 0: return "AddBackground(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ", " + G_Item.GumpID1 + ");";
                case 1: return "AddAlphaRegion(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ");";
                case 2: return "AddImage(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpID1 + ");";
                case 3: return "AddImageTiled(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ", " + G_Item.GumpID1 + ");";
                case 4: return "AddLabel(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpHue + ", " + G_Item.GumpText + ");";
                case 5: return "AddLabelCropped(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ", " + G_Item.GumpHue + ", " + G_Item.GumpText + ");";
                case 6: return "AddTextEntry(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ", " + G_Item.GumpHue + ", 0, " + G_Item.GumpText + ", " + G_Item.GumpText.Length + ");";
                case 7: return "AddHtml(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpWidth + ", " + G_Item.GumpHeight + ", " + G_Item.GumpText + ", false, false);";
                case 8: return "AddItem(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpID1 + ", " + G_Item.GumpHue + ");";
                case 9: return "AddButton(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpID1 + ", " + G_Item.GumpID2 + ", 0, GumpButtonType.Reply, 0);";
                case 10: return "AddRadio(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpID1 + ", " + G_Item.GumpID2 + ", true, 0);";
                case 11: return "AddCheck(" + G_Item.GumpPositionX + ", " + G_Item.GumpPositionY + ", " + G_Item.GumpID1 + ", " + G_Item.GumpID2 + ", true, 0);";
                default: return "";
            }
        }
    }
}
