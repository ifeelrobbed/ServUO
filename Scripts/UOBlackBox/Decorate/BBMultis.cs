//UO Black Box - By GoldDraco13

using Server.Items;

namespace Server.UOBlackBox
{
    public class BBMultis : BaseMulti
    {
        [Constructable]
        public BBMultis(int itemVal) : base(itemVal)
        {
            Name = ("BB - " + Name);
            Visible = true;
        }

        public BBMultis(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
