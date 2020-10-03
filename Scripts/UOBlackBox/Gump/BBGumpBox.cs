//UO Black Box - By GoldDraco13

using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.UOBlackBox
{
    public class BBGumpBox : Container
    {
        public List<BBGumpItem> GumpItems { get; set; }

        public string GetGumpName { get { RefreshAllGumps(); return Name; } }

        public int GetGumpElements { get { return GumpItems.Count; } }

        [Constructable]
        public BBGumpBox() : base(0x2DF3)
        {
            Hue = 1175;

            GumpItems = new List<BBGumpItem>();
        }

        public void LoadBox()
        {
            Item[] getGumpItems = FindItemsByType(typeof(BBGumpItem));

            if (getGumpItems != null && getGumpItems.Length > 0)
            {
                GumpItems = new List<BBGumpItem>();

                foreach (Item item in getGumpItems)
                {
                    if (item is BBGumpItem)
                    {
                        BBGumpItem BBG = item as BBGumpItem;

                        GumpItems.Add(BBG);
                    }
                }

                GumpItems.Sort();
            }
        }

        public BBGumpBox(Serial serial) : base(serial)
        {
        }

        public override int DefaultGumpID
        {
            get
            {
                return 0x9CDB; 
            }
        }

        public override void DropItem(Item dropped)
        {
            GumpItems.Add(dropped as BBGumpItem);

            RefreshAllGumps();

            base.DropItem(dropped);
        }

        public void DeleteElement(int GumpIndex)
        {
            if (GumpIndex < GumpItems.Count && GumpIndex >= 0)
            {
                BBGumpItem item = GumpItems[GumpIndex];

                GumpItems.RemoveAt(GumpIndex);

                item.Delete();

                if (GumpItems.Count <= 0)
                {
                    PlayerMobile pm = RootParent as PlayerMobile;

                    pm.CloseAllGumps();

                    Delete();
                }
                else
                {
                    RefreshAllGumps();
                }
            }
        }

        public void IncreaseLayer(int layer)
        {
            MoveElement(layer, -1);
        }

        public void DecreaseLayer(int layer)
        {
            MoveElement(layer, 1);
        }

        public void MoveElement(int layer, int direction)
        {
            if (GumpItems == null || GumpItems.Count < 0)
                return; 
            
            int oldIndex = layer;
            
            int newIndex = layer + direction;
            
            if (newIndex < 0 || newIndex >= GumpItems.Count)
                return;

            BBGumpItem element = GumpItems[oldIndex];
            
            GumpItems.Remove(element);
            GumpItems.Insert(newIndex, element);
            RefreshAllGumps();
        }

        public void MoveGump(int GumpIndex, int x, int y)
        {
            BBGumpItem gumpItem = GumpItems[GumpIndex];

            gumpItem.GumpPositionX += x;

            gumpItem.GumpPositionY += y;

            RefreshAllGumps();
        }

        public void SelectGump(int layer)
        {
            for (int i = 0; i < GumpItems.Count; i++)
            {
                if (i == layer)
                {
                    GumpItems[i].GumpHue = 53;
                }
                else if (GumpItems[i].GumpHue == 53)
                {
                    GumpItems[i].GumpHue = 0;
                }
            }

            RefreshAllGumps();
        }

        public void RefreshAllGumps()
        {
            PlayerMobile pm = RootParent as PlayerMobile;

            pm.CloseAllGumps();
            
            for (int i = 0; i < GumpItems.Count; i++)
            {
                GumpItems[i].RefreshGump(pm);
                GumpItems[i].BBLayer = i;
            }
        }

        public void PublishGump()
        {
            PlayerMobile pm = RootParent as PlayerMobile;

            string[] getName = Name.Split('-');

            BBGumpScript bbGumpScript = new BBGumpScript(pm, getName[0].TrimEnd(' '), GumpItems);
        }

        public void CloseAll()
        {
            PlayerMobile pm = RootParent as PlayerMobile;

            pm.CloseAllGumps();

            Hue = 1175;
        }

        public override int DefaultDropSound
        {
            get
            {
                return 0x42;
            }
        }

        public override bool OnDragLift(Mobile from)
        {
            if (from.Backpack.Items.Contains(this) && BBServerClient.ServerIsRunning)
            {
                if (Hue == 1175)
                {
                    Item[] GetAllBoxes = from.Backpack.FindItemsByType(typeof(BBGumpBox));

                    foreach (Item box in GetAllBoxes)
                    {
                        if (box.Hue == 1153)
                            box.Hue = 1175;
                    }

                    Hue = 1153;

                    LoadBox();
                }
                else
                {
                    Hue = 1175;

                    CloseAll();
                }
            }
            else
            {
                Hue = 1175;
            }

            return base.OnDragLift(from);
        }

        public override bool OnDroppedToWorld(Mobile from, Point3D p)
        {
            Hue = 1175;

            return base.OnDroppedToWorld(from, p);  
        }

        public override Rectangle2D Bounds
        {
            get
            {
                return new Rectangle2D(0, 0, 600, 300);
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
			
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        { 
            base.Deserialize(reader);
			
            int version = reader.ReadInt();

            Hue = 1175;
        }
    }
}
