//UO Black Box - By GoldDraco13

using Server.Mobiles;

namespace Server.UOBlackBox
{
    class BBControlBox : Item
    {
        private const string Fixed_Version = "1.0.0.154";

        private PlayerMobile PM { get; set; }

        public bool IsOn { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public string Version { get { return _ver; } set { if (value != Fixed_Version) _ver = Fixed_Version; else _ver = value; } }

        private string _ver { get; set; }

        [CommandProperty(AccessLevel.Administrator)]
        public string UserName { get { return PM.Name; } }

        public string GetUserName()
        {
            return PM.Name;
        }

        [Constructable]
        public BBControlBox(string user) : base(0x9F64)
        {
            Name = "UO Black Box - Turned OFF";

            Hue = 1175;

            PM = SetUser(user);

            IsOn = false;

            _ver = Fixed_Version;
        }

        private PlayerMobile SetUser(string name)
        {
            foreach (Mobile mobile in World.Mobiles.Values)
            {
                if (mobile.Name == name)
                {
                    return mobile as PlayerMobile;
                }
            }
            
            return null;
        }

        public BBControlBox(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
             PlayerMobile pm = from as PlayerMobile;

            if (PM == null || pm.Name != GetUserName() || PM.AccessLevel < AccessLevel.Counselor)
            {
                Delete();
            }
            else
            {
                if (IsChildOf(pm.Backpack))
                {
                    if (BBServerClient.ServerIsRunning)
                    {
                        if (!IsOn)
                        {
                            TurnOn();

                            pm.SendMessage("The Server Is On and Listening!");
                        }
                    }
                    else
                    {
                        TurnOff(true);

                        pm.SendMessage("The Server Is Off and  Not Listening!");
                    }
                }
                else
                {
                    TurnOff(false);
                }
            }
        }

        public void TurnOn()
        {
            IsOn = true;

            if (PM != null)
                PM.SendMessage("UO Black Box - Turned On");

            Name = "UO Black Box - Turned On";

            Hue = 1153;

            Movable = false;
        }

        public void TurnOff(bool InBag)
        {
            IsOn = false;

            if (PM != null)
            {
                if (InBag)
                    PM.SendMessage("UO Black Box - Turned Off");
                else
                    PM.SendMessage("This needs to be in your backpack!");
            }

            Name = "UO Black Box - Turned Off";

            Hue = 1175;

            Movable = true;
        }

        public override void OnDelete()
        {
            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); //version

            writer.Write(PM as Mobile);

            writer.Write(Version);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            PM = reader.ReadMobile() as PlayerMobile;

            if (version > 0)
            {
                Version = reader.ReadString();

                if (Version != Fixed_Version)
                    Version = Fixed_Version;
            }

            TurnOff(true);
        }
    }
}
