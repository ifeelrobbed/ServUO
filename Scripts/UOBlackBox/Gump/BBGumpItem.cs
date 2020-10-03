//UO Black Box - By GoldDraco13

using System;
using Server.Gumps;

namespace Server.UOBlackBox
{
    public class BBGumpItem : Item, IComparable<BBGumpItem>
    {
        public Mobile MobPlayer { get; set; }

        private BBGump BBGUMP { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpID1 { get; set; }
        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpID2 { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpPositionX { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpPositionY { get; set; }
        
        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpWidth { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpHeight { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public string GumpText { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int GumpHue { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public GumpTypes GumpType { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int BBLayer { get; set; }

        public enum GumpTypes
        {
            Background = 0,
            AlphaRegion = 1,
            Image = 2,
            ImageTiled = 3,
            Label = 4,
            LabelCropped = 5,
            TextEntry = 6,
            Html = 7,
            Item = 8,
            Button = 9,
            Radio = 10,
            Check = 11
        }

        [Constructable]
        public BBGumpItem(Mobile from, int id1 = 0, int id2 = 0, int w = 0, int h = 0, string text = "", int hue = 0, int type = 0) : base(0x2831)
        {
            MobPlayer = from;

            Name = "UO Black Box - " + (GumpTypes)type;
            Visible = true;
            Movable = false;
            Hue = 2 + (type * 10);

            GumpID1 = id1;
            GumpID2 = id2;

            GumpWidth = w;
            GumpHeight = h;

            GumpText = text;
            GumpHue = hue;
            GumpType = (GumpTypes)type;

            GumpPositionX = 0;
            GumpPositionY = 0;
        }

        public BBGumpItem(Serial serial) : base(serial)
        {
        }

        public int CompareTo(BBGumpItem other)
        {
            if (BBLayer > other.BBLayer)
                return 1;
            else if (BBLayer < other.BBLayer)
                return -1;
            else
                return 0;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (MobPlayer != null)
            {
                if (from.Name == MobPlayer.Name)
                {
                    from.SendGump(new PropertiesGump(from, this));
                }
            }
        }

        public void RefreshGump(Mobile from)
        {
            if (from.Name == MobPlayer.Name)
            {
                BBGUMP = new BBGump(GumpID1, GumpID2, GumpWidth, GumpHeight, GumpText, GumpHue, GumpType.ToString(), GumpPositionX, GumpPositionY);

                from.SendGump(BBGUMP);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); //version

            writer.Write(MobPlayer);

            writer.Write(GumpID1);
            writer.Write(GumpID2);

            writer.Write(GumpPositionX);
            writer.Write(GumpPositionY);

            writer.Write(GumpWidth);
            writer.Write(GumpHeight);

            writer.Write(GumpText);

            writer.Write(GumpHue);

            writer.Write((int)GumpType);

            writer.Write(BBLayer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            MobPlayer = reader.ReadMobile();

            GumpID1 = reader.ReadInt();
            GumpID2 = reader.ReadInt();

            GumpPositionX = reader.ReadInt();
            GumpPositionY = reader.ReadInt();

            GumpWidth = reader.ReadInt();
            GumpHeight = reader.ReadInt();

            GumpText = reader.ReadString(); 

            GumpHue = reader.ReadInt();

            GumpType = (GumpTypes)reader.ReadInt();

            BBLayer = reader.ReadInt();
        }
    }
}
