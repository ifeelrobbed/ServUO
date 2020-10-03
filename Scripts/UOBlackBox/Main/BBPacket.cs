//UO Black Box - By GoldDraco13

using System;

namespace Server.UOBlackBox
{
    public class BBPacket : IComparable<BBPacket>
    {
        public int Passcode { get; set; }

        public string Name { get; set; }

        public CommandTypes CommandType { get; set; }

        public enum CommandTypes
        {
            GENERIC_COMMAND = 0,
            SPECIAL_COMMAND = 1,
            RETURN_COMMAND = 2,
            STAFF_COMMAND = 3,
            MAP_COMMAND = 4,
            GUMP_COMMAND = 5,
            BUILDER_COMMAND = 6,
            TWITCH_COMMAND = 7,
            HEATMAP_COMMAND = 8
        }

        public string Command { get; set; }

        public string Data { get; set; }

        public string Args { get; set; }

        public BBPacket(string name, CommandTypes type, string command, string data = "", string args = "")
        {
            Passcode = 1172;
            Name = name;
            CommandType = type;
            Command = command;
            Data = data;
            Args = args;
        }

        public object this[int index]
        {
            get
            {
                if (index == 0)
                    return Name;
                else if (index == 1)
                    return CommandType;
                else if (index == 2)
                    return Command;
                else if (index == 3)
                    return Data;
                else if (index == 4)
                    return Args;
                else
                    return null;
            }
            set
            {
                if (index == 0)
                    Name = (string)value;
                else if (index == 1)
                    CommandType = (CommandTypes)value;
                else if (index == 2)
                    Command = (string)value;
                else if (index == 3)
                    Data = (string)value;
                else if (index == 4)
                    Args = (string)value;
            }
        }

        public int CompareTo(BBPacket cp)
        {
            if ((int)CommandType > (int)cp.CommandType)
                return 1;
            else if ((int)CommandType < (int)cp.CommandType)
                return -1;
            else
                return 0;
        }
    }
}
