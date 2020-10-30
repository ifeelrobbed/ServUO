//UO Black Box - By GoldDraco13

using System;
using System.Collections.Generic;
using System.Text;
using Server.Commands;
using Server.Mobiles;

namespace Server.UOBlackBox
{
    public static class BBProcessReplyCommand
    {
        /// <summary>
        /// Staff Mobile
        /// </summary>
        private static List<PlayerMobile> Staff_List { get; set; }

        /// <summary>
        /// List to store the Twitch Players
        /// </summary>
        private static List<BaseCreature> Twitch_List { get; set; }

        /// <summary>
        /// Toggle the Twitch Follower Max Count Allowed
        /// </summary>
        private static bool CanHaveUnlimitedFollowers { get; set; }

        /// <summary>
        /// Process Incoming Data from Client / Do Security Check and Login Check!
        /// </summary>
        /// <param name="reply"></param>
        public static void ReceiveRemoteCmd(string reply)
        {
            if (Staff_List == null)
                Staff_List = new List<PlayerMobile>();

            if (reply.Contains("NO_COMMAND") || reply.Contains("SENT_DATA") || reply.Contains("NOT_VALID"))
            {
                //Do Nothing?
            }
            else if (reply.Contains(":"))
            {
                string[] _GetData = reply.Split(':');

                if (_GetData.Length > 4)
                {
                    try
                    {
                        PlayerMobile staff = null;

                        int Converted = Convert.ToInt32(_GetData[1]);

                        BBPacket packet = new BBPacket(_GetData[0], (BBPacket.CommandTypes)Converted, _GetData[2], _GetData[3], _GetData[4]);

                        bool IsStaff = false;

                        foreach (Mobile mobile in World.Mobiles.Values)
                        {
                            if (mobile is PlayerMobile)
                            {
                                PlayerMobile player = mobile as PlayerMobile;

                                if (player.Name == packet.Name && player.AccessLevel > AccessLevel.Player)
                                {
                                    if (!Staff_List.Contains(player))
                                    {
                                        Staff_List.Add(player);

                                        staff = player;
                                    }
                                    else
                                    {
                                        staff = Staff_List[Staff_List.IndexOf(player)];
                                    }

                                    IsStaff = true;

                                    break;
                                }
                            }
                        }

                        bool PassSecurityCheck = false;

                        if (IsStaff && staff != null)
                        {
                            Item Black_Box = staff.Backpack.FindItemByType(typeof(BBControlBox));

                            if (Black_Box == null)
                            {
                                var IsAdded = staff.AddToBackpack(new BBControlBox(staff.Name));

                                if (IsAdded)
                                    Black_Box = staff.Backpack.FindItemByType(typeof(BBControlBox));
                            }

                            BBControlBox Black_BoxControl = Black_Box as BBControlBox;
                            
                            if (Black_BoxControl != null)
                            {
                                if (packet.CommandType != BBPacket.CommandTypes.SPECIAL_COMMAND)
                                {
                                    if (Black_BoxControl.GetUserName() != staff.Name || !Black_BoxControl.IsOn)
                                    {
                                        staff.SendMessage(37, "Your Control Box is turned off, Secure Channel - Failed");

                                        Black_BoxControl.Hue = 37;
                                    }
                                    else
                                    {
                                        Black_BoxControl.Hue = 62;

                                        PassSecurityCheck = true;
                                    }
                                }
                                else
                                {
                                    PassSecurityCheck = true;
                                }

                                if (PassSecurityCheck)
                                {
                                    RunCommand(ref Black_BoxControl, ref staff, packet, PassSecurityCheck);
                                }
                            }
                        }

                        if (!PassSecurityCheck)
                        {
                            BBServerMessage.WriteConsoleColored(ConsoleColor.Red, "UO Black Box : Bad Login Attempt : [BLOCKED] => " + _GetData[0]);

                            BBServerClient.SendCommand(new BBPacket(_GetData[0], BBPacket.CommandTypes.SPECIAL_COMMAND, "SHUTDOWN", "", "SET_DATA"));
                        }
                    }
                    catch (Exception ex)
                    {
                        BBServerMessage.LogPacketCMD("ERROR", "UO Black Box : Exception : " + reply + " : " + ex.Message, true);
                    }
                }
                else
                {
                    BBServerMessage.LogPacketCMD("ERROR", "UO Black Box : Data Split Failed : " + reply, true);
                }
            }
            else
            {
                BBServerMessage.LogPacketCMD("ERROR", "UO Black Box : Incorrect Data : " + reply, true);
            }
        }

        /// <summary>
        /// Process Command from Client 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="packet"></param>
        /// <param name="passed"></param>
        private static void RunCommand(ref BBControlBox box, ref PlayerMobile Staff_Player, BBPacket packet, bool passed)
        {
            if (packet.Name != "")
            {
                if (passed)
                {
                    switch (packet.CommandType)
                    {
                        case BBPacket.CommandTypes.GENERIC_COMMAND:
                            {
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);
                                break;
                            }
                        case BBPacket.CommandTypes.SPECIAL_COMMAND:
                            {
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                if (Staff_Player.AccessLevel != AccessLevel.Player)
                                {
                                    if (packet.Command.Contains("LOGIN"))
                                    {
                                        BBServerMessage.WriteConsoleColored(ConsoleColor.Green, "UO Black Box - " + packet.Name + " : " + packet.Command + " @ " + DateTime.Now);

                                        Staff_Player.SendMessage(62, "UO Black Box - [Turned On]");
                                        Staff_Player.SendMessage(62, "Secure Channel - [Success]");

                                        if (box.Version != packet.Data)
                                            Staff_Player.SendMessage(37, "*WARNING* Bad Script Version => [Reinstall Server Scripts]");
                                        
                                        box.TurnOn();
                                    }
                                    else
                                    {
                                        BBServerMessage.WriteConsoleColored(ConsoleColor.DarkGreen, "UO Black Box - " + packet.Name + " : " + packet.Command + " @ " + DateTime.Now);

                                        Staff_Player.SendMessage(62, "UO Black Box - [Turned Off]");
                                        Staff_Player.SendMessage(62, "Secure Channel - [Removed]");

                                        box.TurnOff(true);
                                    }
                                }
                                else
                                {
                                    BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                    BBServerClient.SendCommand(new BBPacket(Staff_Player.Name, BBPacket.CommandTypes.SPECIAL_COMMAND, "SHUTDOWN", "", "SET_DATA"));
                                }

                                break;
                            }
                        case BBPacket.CommandTypes.RETURN_COMMAND:
                            {
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);
                                break;
                            }
                        case BBPacket.CommandTypes.STAFF_COMMAND:
                            {
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);
                                break;
                            }
                        case BBPacket.CommandTypes.MAP_COMMAND:
                            {
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                string MapName = "";
                                string LocX = "0";
                                string LocY = "0";
                                string MapZ = "0";

                                if (packet.Command.Contains(";"))
                                {
                                    string[] GetLocData = packet.Command.Split(';');

                                    MapName = GetLocData[0];
                                    LocX = GetLocData[1];
                                    LocY = GetLocData[2];

                                    try
                                    {
                                        MapZ = GetMapZ(MapName, Convert.ToInt32(LocX), Convert.ToInt32(LocY));
                                    }
                                    catch
                                    {
                                        MapZ = "0";
                                    }
                                }

                                if (packet.Data != "")
                                {
                                    switch (packet.Data)
                                    {
                                        case "IsTeleport":
                                            {
                                                try
                                                {
                                                    Point3D pt = new Point3D(Convert.ToInt32(LocX), Convert.ToInt32(LocY), Convert.ToInt32(MapZ));

                                                    switch (MapName)
                                                    {
                                                        case "Felucca":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.Felucca);
                                                                break;
                                                            }
                                                        case "Trammel":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.Trammel);
                                                                break;
                                                            }
                                                        case "Ilshenar":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.Ilshenar);
                                                                break;
                                                            }
                                                        case "Malas":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.Malas);
                                                                break;
                                                            }
                                                        case "Tokuno":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.Tokuno);
                                                                break;
                                                            }
                                                        case "TerMur":
                                                            {
                                                                Staff_Player.MoveToWorld(pt, Map.TerMur);
                                                                break;
                                                            }
                                                        default:
                                                            break;
                                                    }
                                                }
                                                catch
                                                {
                                                    Staff_Player.SendMessage("Bad Location, Failed to move to that location!");
                                                }

                                                break;
                                            }
                                        case "IsGate":
                                            {
                                                string CMD = ("add BBTravel " + Staff_Player.Name + " " + MapName + " " + LocX + " " + LocY + " " + MapZ);
                                                CommandSystem.Handle(Staff_Player, (CommandSystem.Prefix + CMD), Network.MessageType.Regular);
                                                break;
                                            }
                                        case "IsSpecial":
                                            {
                                                string CMD = ("add BBTravel " + Staff_Player.Name + " " + MapName + " " + LocX + " " + LocY + " " + MapZ + " " + true);
                                                CommandSystem.Handle(Staff_Player, (CommandSystem.Prefix + CMD), Network.MessageType.Regular);
                                                break;
                                            }
                                        case "MARK":
                                            {
                                                BBServerClient.SendCommand(new BBPacket(Staff_Player.Name, BBPacket.CommandTypes.MAP_COMMAND, Staff_Player.Map + "," + Staff_Player.X + "," + Staff_Player.Y + "," + Staff_Player.Z, "MARK", "SET_DATA"));

                                                Staff_Player.SendMessage(53, Staff_Player.Map.Name + " : " + Staff_Player.X + " : " + Staff_Player.Y + " : " + Staff_Player.Z + " - Marked!");
                                                break;
                                            }
                                        case "GET_PLAYERS":
                                            {
                                                int count = 0;

                                                StringBuilder sb = new StringBuilder();

                                                sb.Append(("$" + Staff_Player.Name + "," + Staff_Player.Map.Name + "," + Staff_Player.X + "," + Staff_Player.Y + "," + Staff_Player.Z + ";"));

                                                foreach (Mobile mob in World.Mobiles.Values)
                                                {
                                                    if (mob is PlayerMobile)
                                                    {
                                                        PlayerMobile pm = mob as PlayerMobile;

                                                        if (pm.Name != packet.Name && pm.Map != Map.Internal)
                                                        {
                                                            if (packet.Command == pm.Map.Name.ToString())
                                                            {
                                                                sb.Append(("$" + pm.Name + "," + pm.Map.Name + "," + pm.X + "," + pm.Y + "," + pm.Z + ";"));

                                                                count++;
                                                            }
                                                        }
                                                    }
                                                }
                                                BBServerClient.SendCommand(new BBPacket(Staff_Player.Name, BBPacket.CommandTypes.MAP_COMMAND, sb.ToString(), "PLAYERS", "SET_DATA"));

                                                Staff_Player.SendMessage("Found " + count + " Players in " + packet.Command + " ...Sending");
                                                break;
                                            }
                                        case "GOTO_PLAYER":
                                            {
                                                if (MapName != Map.Internal.ToString())
                                                {
                                                    string CMD = ("self set map " + MapName + " x " + LocX + " y " + LocY + " z " + MapZ);

                                                    CommandSystem.Handle(Staff_Player, (CommandSystem.Prefix + CMD), Network.MessageType.Regular);
                                                }

                                                break;
                                            }
                                        default:
                                            {
                                                CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);

                                                break;
                                            }
                                    }
                                    break;
                                }
                                else
                                {
                                    CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);

                                    break;
                                }
                            }
                        case BBPacket.CommandTypes.GUMP_COMMAND:
                            {
                                if (packet.Data.Contains("Gump Name"))
                                    break;

                                Item[] GetBoxes = Staff_Player.Backpack.FindItemsByType(typeof(BBGumpBox));

                                bool IsLoaded = false;

                                BBGumpBox gumpBox = null;

                                foreach (Item _Box in GetBoxes)
                                {
                                    if (_Box.Hue == 1153)
                                    {
                                        gumpBox = _Box as BBGumpBox;

                                        IsLoaded = true;
                                    }
                                }

                                if (!IsLoaded)
                                {
                                    if (!packet.Data.Contains("LOAD_GUMP"))
                                    {
                                        gumpBox = new BBGumpBox
                                        {
                                            Name = packet.Data + " - Gump Box",
                                            Hue = 1153
                                        };

                                        Staff_Player.Backpack.AddItem(gumpBox);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (packet.Data.Contains("LOAD_GUMP"))
                                    {
                                        gumpBox.LoadBox();

                                        BBServerClient.SendCommand(new BBPacket(Staff_Player.Name, BBPacket.CommandTypes.GUMP_COMMAND, gumpBox.GetGumpName + ";" + gumpBox.GetGumpElements, "LOAD_GUMP", "SET_DATA"));

                                        break;
                                    }
                                }

                                if (gumpBox.Name.Contains(packet.Data))
                                {
                                    if (packet.Command.Contains("_"))
                                    {
                                        string[] GetData = packet.Command.Split('_');

                                        try
                                        {
                                            if (packet.Command.Contains("INCREASE"))
                                            {
                                                gumpBox.IncreaseLayer(Convert.ToInt32(GetData[2]));
                                            }
                                            else if (packet.Command.Contains("DECREASE"))
                                            {
                                                gumpBox.DecreaseLayer(Convert.ToInt32(GetData[2]));
                                            }
                                            else if (packet.Command.Contains("REMOVE"))
                                            {
                                                gumpBox.DeleteElement(Convert.ToInt32(GetData[1]));
                                            }
                                            else if (packet.Command.Contains("MOVE"))
                                            {
                                                gumpBox.MoveGump(Convert.ToInt32(GetData[1]), Convert.ToInt32(GetData[2]), Convert.ToInt32(GetData[3]));
                                            }
                                            else if (packet.Command.Contains("SELECT"))
                                            {
                                                gumpBox.SelectGump(Convert.ToInt32(GetData[2]));
                                            }
                                            else if (packet.Command.Contains("PUBLISH"))
                                            {
                                                gumpBox.PublishGump();
                                            }
                                            else if (packet.Command.Contains("CLOSE"))
                                            {
                                                gumpBox.CloseAll();
                                            }
                                        }
                                        catch
                                        {
                                            Staff_Player.SendMessage("Bad Command : Try Again!");
                                        }
                                    }
                                    else
                                    {
                                        string[] GetData = packet.Command.Split(';');

                                        try
                                        {
                                            gumpBox.DropItem(new BBGumpItem(Staff_Player,
                                                                            Convert.ToInt32(GetData[0]),
                                                                            Convert.ToInt32(GetData[1]),
                                                                            Convert.ToInt32(GetData[2]),
                                                                            Convert.ToInt32(GetData[3]),
                                                                            GetData[4],
                                                                            Convert.ToInt32(GetData[5]),
                                                                            Convert.ToInt32(GetData[6])));
                                        }
                                        catch
                                        {
                                            Staff_Player.SendMessage("Bad Element : Try Again!");
                                        }
                                    }
                                }
                                
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                break;
                            }
                        case BBPacket.CommandTypes.BUILDER_COMMAND:
                            {
                                CommandSystem.Handle(Staff_Player, CommandSystem.Prefix + packet.Command, Network.MessageType.Regular);
                                
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                break;
                            }
                        case BBPacket.CommandTypes.TWITCH_COMMAND:
                            {
                                if (Twitch_List == null)
                                    Twitch_List = new List<BaseCreature>();

                                packet.Command = packet.Command.ToLower();

                                int MOD = 1;

                                if (packet.Data.Contains(";"))
                                {
                                    var GetData = packet.Data.Split(';');

                                    packet.Data = GetData[0];

                                    MOD = Convert.ToInt32(GetData[1]) > 1? Convert.ToInt32(GetData[1]) : 1;
                                }

                                if (packet.Command.Contains("!spawn"))
                                {
                                    string mob = packet.Command.Substring(6).TrimStart(' ');

                                    BaseCreature TwitchMobile = null;

                                    bool ObtainedMob = false;

                                    bool IsAlreadySpawned = false;

                                    foreach (Mobile mobile in World.Mobiles.Values)
                                    {
                                        if (mobile.Name.ToLower().Contains(mob) && !ObtainedMob)
                                        {
                                            if (mobile is BaseCreature)
                                            {
                                                BaseCreature bc = mobile as BaseCreature;

                                                foreach (BaseCreature baseCreature in Twitch_List)
                                                {
                                                    if (baseCreature.Name.ToLower() == bc.Name.ToLower())
                                                        IsAlreadySpawned = true;
                                                }

                                                if (bc.Tamable && !Twitch_List.Contains(bc) && bc.Combatant == null)
                                                {
                                                    ObtainedMob = true;

                                                    TwitchMobile = bc;
                                                }
                                            }
                                        }
                                    }

                                    if (TwitchMobile != null && !IsAlreadySpawned)
                                    {
                                        if (CanHaveUnlimitedFollowers)
                                        {
                                            TwitchMobile.ControlSlots = 0;
                                            TwitchMobile.MinTameSkill = 0;
                                        }

                                        int ControlCount = 0;

                                        foreach (Mobile mobile in Staff_Player.AllFollowers)
                                        {
                                            if (mobile is BaseCreature)
                                            {
                                                BaseCreature bc = mobile as BaseCreature;

                                                ControlCount += bc.ControlSlots;
                                            }
                                        }

                                        if (TwitchMobile.ControlSlots + ControlCount <= Staff_Player.FollowersMax && TwitchMobile.MinTameSkill < Staff_Player.Skills.AnimalTaming.Value)
                                        {
                                            TwitchMobile.Hits = TwitchMobile.Hits / MOD;

                                            TwitchMobile.RawStr = TwitchMobile.RawStr / MOD;
                                            TwitchMobile.RawInt = TwitchMobile.RawInt / MOD;
                                            TwitchMobile.RawDex = TwitchMobile.RawDex / MOD;

                                            TwitchMobile.ManaMaxSeed = TwitchMobile.ManaMaxSeed / 100;
                                            TwitchMobile.StamMaxSeed = TwitchMobile.StamMaxSeed / 100;

                                            TwitchMobile.DamageMax = TwitchMobile.DamageMax / MOD;

                                            bool IsPlayerSet = TwitchMobile.SetControlMaster(Staff_Player);

                                            if (IsPlayerSet)
                                            {
                                                TwitchMobile.ControlOrder = OrderType.Follow;
                                                TwitchMobile.ControlTarget = TwitchMobile.ControlMaster;
                                                TwitchMobile.Loyalty = BaseCreature.MaxLoyalty;

                                                TwitchMobile.Hue = Utility.RandomBrightHue();
                                                TwitchMobile.Name = packet.Data;
                                                TwitchMobile.NameHue = TwitchMobile.Hue;

                                                TwitchMobile.MoveToWorld(Staff_Player.Location, Staff_Player.Map);

                                                PlaySpawningEffect(TwitchMobile, TwitchMobile.Hue);

                                                Twitch_List.Add(TwitchMobile);

                                                Staff_Player.SendMessage(53, packet.Data + ", has spawned!");

                                                TwitchMobile.Say("[" + packet.Data + "]");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!fight"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        foreach (BaseCreature bc in Twitch_List)
                                        {
                                            if (bc.Name.ToLower() == packet.Data.ToLower())
                                            {
                                                bc.ControlTarget = bc.ControlMaster.Combatant;

                                                bc.ControlOrder = OrderType.Attack;

                                                bc.Say("[" + packet.Data + "] I'm going to fight " + Staff_Player.Combatant.Name + "!");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!guard"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        foreach (BaseCreature bc in Twitch_List)
                                        {
                                            if (bc.Name == packet.Data)
                                            {
                                                bc.ControlTarget = bc.ControlMaster.Combatant;

                                                bc.ControlOrder = OrderType.Guard;

                                                bc.Say("[" + packet.Data + "] I'm going to guard " + Staff_Player.Name + "!");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!follow"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        foreach (BaseCreature bc in Twitch_List)
                                        {
                                            if (bc.Name == packet.Data)
                                            {
                                                bc.ControlTarget = bc.ControlMaster;

                                                bc.ControlOrder = OrderType.Follow;

                                                bc.Say("[" + packet.Data + "] I'm going to follow " + Staff_Player.Name + "!");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!stop"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        foreach (BaseCreature bc in Twitch_List)
                                        {
                                            if (bc.Name == packet.Data)
                                            {
                                                bc.ControlTarget = bc.ControlMaster;

                                                bc.ControlOrder = OrderType.Stop;

                                                bc.Say("[" + packet.Data + "] I'm going to stop here!");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!stay"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        foreach (BaseCreature bc in Twitch_List)
                                        {
                                            if (bc.Name == packet.Data)
                                            {
                                                bc.ControlTarget = bc.ControlMaster;

                                                bc.ControlOrder = OrderType.Stay;

                                                bc.Say("[" + packet.Data + "] I'm going to stay here!");
                                            }
                                        }
                                    }
                                }
                                else if (packet.Command.Contains("!leave"))
                                {
                                    if (Twitch_List != null && Twitch_List.Count > 0)
                                    {
                                        int FollowerCount = Twitch_List.Count;

                                        for (int i = 0; i < FollowerCount; i++)
                                        {
                                            if (Twitch_List[i].Name == packet.Data)
                                            {
                                                PlaySpawningEffect(Twitch_List[i], Twitch_List[i].Hue);

                                                Twitch_List[i].Delete();
                                            }
                                        }
                                    }

                                    Staff_Player.SendMessage(47, packet.Data + ", has left!");

                                    Staff_Player.Say("Thanks for playing, " + packet.Data);
                                }
                                else if (packet.Command.Contains("!drink"))
                                {
                                    Staff_Player.SendMessage(53, packet.Data + ", sent a drink!");

                                    Staff_Player.Say("*takes a drink*");
                                    Staff_Player.Animate(34, 5, 1, true, false, 0);
                                    Staff_Player.PlaySound(0x1E0);

                                    if (Staff_Player.BAC < 60)
                                    {
                                        Staff_Player.BAC++;
                                        Staff_Player.Say("*feeling good*");
                                    }
                                    else
                                    {
                                        Staff_Player.Say("*feeling drunk*");
                                    }
                                }
                                else if (packet.Command.Contains("!bless"))
                                {
                                    Staff_Player.SendMessage(53, packet.Data + ", sent a blessing!");

                                    int SparkleHue1 = 1153;
                                    int SparkleHue2 = Utility.RandomBrightHue();
                                    int SparkleHue3 = Utility.RandomBrightHue();

                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X, Staff_Player.Y + 1, Staff_Player.Z), Staff_Player.Map, 0x373A, 15, SparkleHue1, 0);
                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X + 1, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x373A, 15, SparkleHue2, 0);
                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z - 1), Staff_Player.Map, 0x373A, 15, SparkleHue3, 0);

                                    Effects.PlaySound(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x213);

                                    Staff_Player.Say("*feeling blessed*");
                                }
                                else if (packet.Command.Contains("!smite"))
                                {
                                    Staff_Player.SendMessage(53, packet.Data + ", sent a curse!");

                                    Effects.SendBoltEffect(Staff_Player, true, 0);
                                    Effects.PlaySound(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x307);

                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z + 1), Staff_Player.Map, 0x374A, 15);
                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X + 1, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x374A, 15);
                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X + 1, Staff_Player.Y + 1, Staff_Player.Z), Staff_Player.Map, 0x374A, 15);
                                    Effects.SendLocationEffect(new Point3D(Staff_Player.X, Staff_Player.Y + 1, Staff_Player.Z), Staff_Player.Map, 0x374A, 15);

                                    Effects.PlaySound(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x1E0);

                                    Staff_Player.Say("*feeling cursed*");
                                }
                                else if (packet.Command.Contains("!claim"))
                                {
                                    Staff_Player.SendMessage(53, packet.Data + ", claimed a hidden reward!");

                                    Effects.SendBoltEffect(Staff_Player, true, 0);
                                    Effects.PlaySound(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x307);

                                    Effects.PlaySound(new Point3D(Staff_Player.X, Staff_Player.Y, Staff_Player.Z), Staff_Player.Map, 0x543); //(Tado)0x544 => in fiddler (-1 to align)
                                }
                                else if (packet.Command.Contains("!unlimited"))
                                {
                                    CanHaveUnlimitedFollowers = true;

                                    Staff_Player.SendMessage(62, "UnLimited Followers : Turned On");
                                }
                                else if (packet.Command.Contains("!limited"))
                                {
                                    CanHaveUnlimitedFollowers = false;

                                    Staff_Player.SendMessage(62, "Limited Followers : Turned On");
                                }
                                else if (packet.Command.Contains("!kill"))
                                {
                                    int FollowerCount = Twitch_List.Count;

                                    for (int i = 0; i < FollowerCount; i++)
                                    {
                                        PlaySpawningEffect(Twitch_List[i], Twitch_List[i].Hue);

                                        Twitch_List[i].Delete();
                                    }

                                    Staff_Player.SendMessage(47, "All Followers : Terminated!");

                                    if (Staff_Player.AllFollowers.Count == 0)
                                        Staff_Player.Followers = 0;
                                }
                                else
                                {
                                    Staff_Player.SendMessage(62, packet.Data + " => " + packet.Command.TrimStart('!'));
                                }
                                
                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                break;
                            }
                        case BBPacket.CommandTypes.HEATMAP_COMMAND:
                            {
                                if (packet.Command.Contains("GETMAP"))
                                {
                                    BBHeatMap.SaveToFile();

                                    string[] GetData = packet.Command.Split('*');

                                    if (GetData.Length > 1)
                                    {
                                        int type = Convert.ToInt32(GetData[1]);
                                        int day = Convert.ToInt32(GetData[2]);

                                        string command = BBHeatMap.LoadFromFile((BBHeatMap.HeatTypes)type, day, packet.Data);

                                        BBServerClient.SendCommand
                                        (
                                            new BBPacket
                                            (
                                                Staff_Player.Name,
                                                BBPacket.CommandTypes.HEATMAP_COMMAND,
                                                command,
                                                "HEATMAP_DATA",
                                                "SET_DATA"
                                            )
                                        );
                                    }
                                }

                                BBServerMessage.LogPacketCMD(packet.Name, packet.Command, false);

                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// This is from the GMHideStone.cs - ref ServUO pub 57 // Effects for Spawning/Leaving Twitch Players
        /// </summary>
        /// <param name="m"></param>
        /// <param name="effHue"></param>
        private static void PlaySpawningEffect(Mobile m, int effHue)
        {
            if (effHue > 0)
                effHue--; //Adjust the frigging hue to match true effect color 

            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z + 4), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y, m.Z - 4), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z + 4), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y + 1, m.Z - 4), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 11), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 7), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X + 1, m.Y + 1, m.Z + 3), m.Map, 0x3728, 13, effHue, 0);
            Effects.SendLocationEffect(new Point3D(m.X, m.Y, m.Z + 1), m.Map, 0x3728, 13, effHue, 0);
            Effects.PlaySound(new Point3D(m.X, m.Y, m.Z), m.Map, 0x228);
        }

        /// <summary>
        /// Returns the Z surface for Target Map Location2D
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="locX"></param>
        /// <param name="locY"></param>
        /// <returns></returns>
        private static string GetMapZ(string mapName, int locX, int locY)
        {
            int GetZ = 0;

            switch (mapName)
            {
                case "Felucca":
                    {
                        LandTile landTile = Map.Felucca.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.Felucca.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }

                        return (GetZ + 1).ToString();
                    }
                case "Trammel":
                    {
                        LandTile landTile = Map.Trammel.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.Trammel.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }
                        return (GetZ + 1).ToString();
                    }
                case "Ilshenar":
                    {
                        LandTile landTile = Map.Ilshenar.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.Ilshenar.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }
                        return (GetZ + 1).ToString();
                    }
                case "Malas":
                    {
                        LandTile landTile = Map.Malas.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.Malas.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }
                        return (GetZ + 1).ToString();
                    }
                case "Tokuno":
                    {
                        LandTile landTile = Map.Tokuno.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.Tokuno.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }
                        return (GetZ + 1).ToString();
                    }
                case "TerMur":
                    {
                        LandTile landTile = Map.TerMur.Tiles.GetLandTile(locX, locY);
                        GetZ = landTile.Z;

                        StaticTile[] GetSurface = Map.TerMur.Tiles.GetStaticTiles(locX, locY);

                        if (GetSurface.Length > 0)
                        {
                            foreach (var staticTile in GetSurface)
                            {
                                if (staticTile.Z > GetZ + 1)
                                    GetZ = staticTile.Z;
                            }
                        }
                        return (GetZ + 1).ToString();
                    }
                default:
                    {
                        return "0";
                    }
            }
        }
    }
}
