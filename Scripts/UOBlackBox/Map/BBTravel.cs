//UO Black Box - By GoldDraco13

using Server.Mobiles;
using System;

namespace Server.UOBlackBox
{
    class BBTravel : Item
    {
        public string Owner { get; set; }

        private Map Old_Map { get; set; }
        private Map New_Map { get; set; }

        private Point3D Old_Location { get; set; }
        private Point3D New_Location { get; set; }

        private bool IsSpecial = false;

        private DeleteTimer Delete_Timer { get; set; }
        private const int deleteDelay = 10;

        private QuickDeleteTimer Quick_DeleteTimer { get; set; }
        private const int q_DeleteDelay = 50;

        private SoundTimer Sound_Timer { get; set; }
        private const int soundDelay = 50;

        private MoveTimer Move_Timer { get; set; }
        private const int moveDelay = 50;

        private RevealTimer Reveal_Timer { get; set; }
        private const int revealDelay = 500;

        private Random rnd = new Random();

        private int Custom_Hue { get; set; }

        [Constructable]
        public BBTravel(string owner)
        {
            Name = "Black Box Portal";
            Owner = owner;

            Movable = false;
            ItemID = 0xF6C;
            Light = LightType.Circle300;

            Old_Map = Map;
            Old_Location = new Point3D(X, Y, Z);

            StartDeleteGate(this);

            StartingSound(this);
        }

        [Constructable]
        public BBTravel(string owner, Map map, int x, int y, int z)
        {
            Name = "Black Box Portal";
            Owner = owner;

            Movable = false;
            ItemID = 0xF6C;
            Light = LightType.Circle300;

            Custom_Hue = Hue = rnd.Next(1, 1001);

            Old_Map = Map;
            Old_Location = new Point3D(X, Y, Z);

            New_Map = map;
            New_Location = new Point3D(x, y, z);

            StartDeleteGate(this);

            StartingSound(this);
        }

        [Constructable]
        public BBTravel(string owner, Map map, int x, int y, int z, bool isSpecial)
        {
            Name = "Black Box SoulStone";
            Owner = owner;

            IsSpecial = isSpecial;

            Movable = false;
            ItemID = 0x423F;
            Light = LightType.Circle300;

            int RandomEffect = rnd.Next(10);

            if (RandomEffect <= 1) //Fire
                Hue = 1260;
            if (RandomEffect == 2) //Ice
                Hue = 1266;
            if (RandomEffect == 3) //Toxic
                Hue = 1272;
            if (RandomEffect == 4) //Electric
                Hue = 1283;
            if (RandomEffect == 5) //Mist
                Hue = 1288;
            if (RandomEffect == 6) //Explosion
                Hue = 1174;
            if (RandomEffect == 7) //Stone
                Hue = 1177;
            if (RandomEffect >= 8) //Shiney
                Hue = 1287;

            Old_Map = Map;
            Old_Location = new Point3D(X, Y, Z);

            New_Map = map;
            New_Location = new Point3D(x, y, z);

            StartDeleteGate(this);

            StartingSound(this);
        }

        private void StartDeleteGate(BBTravel gate)
        {
            if (gate != null)
            {
                Delete_Timer = new DeleteTimer(gate);
                Delete_Timer.Start();
            }
        }

        private void QuickDelete(BBTravel gate)
        {
            if (gate != null)
            {
                Quick_DeleteTimer = new QuickDeleteTimer(gate);
                Quick_DeleteTimer.Start();
            }
        }

        private void StartingSound(BBTravel gate)
        {
            if (gate != null)
            {
                Sound_Timer = new SoundTimer(gate);
                Sound_Timer.Start();
            }
        }

        public BBTravel(Serial serial) : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (pm == null)
                return;

            if (!IsSpecial)
            {
                if (m.InRange(GetWorldLocation(), 1))
                    OnMoveOver(m);
                else
                    m.SendLocalizedMessage(500446); // That is too far away.
            }
            else
            {
                OnMoveOver(m);
            }
        }

        public override bool OnMoveOver(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (pm != null)
            {
                if (IsSpecial)
                {
                    pm.Hidden = true;

                    if (Hue == 1260) //Fire
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x3709, 30);
                    if (Hue == 1266) //Ice
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x37CC, 40);
                    if (Hue == 1272) //Toxic
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x374A, 17);
                    if (Hue == 1283) //Electric
                        Effects.SendBoltEffect(pm);
                    if (Hue == 1288) //Mist
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x3728, 13);
                    if (Hue == 1174) //Explosion
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x36BD, 10);
                    if (Hue == 1177) //Stone
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x37C4, 31);
                    if (Hue == 1287) //Shiny
                        Effects.SendLocationEffect(Old_Location, Old_Map, 0x375A, 30);

                    Reveal_Timer = new RevealTimer(pm);
                    Reveal_Timer.Start();
                }
                else
                {
                    Effects.SendLocationEffect(Old_Location, Old_Map, 0x3728, 13);
                }
                StartTravel(pm, this);
                    return true;
            }
            else
            {
                return false;
            }
        }

        private void StartTravel(PlayerMobile pm, BBTravel BBT)
        {
            if (New_Map == null || New_Map == Map.Internal)
                return;

            if (pm != null && BBT != null)
            {
                Move_Timer = new MoveTimer(pm, BBT);
                Move_Timer.Start();
            }
        }

        private void StartSound(BBTravel BBT)
        {
            foreach (Mobile player in World.Mobiles.Values)
            {
                PlayerMobile pm = player as PlayerMobile;

                if (pm != null && BBT != null)
                {
                    if (pm.Map == BBT.Map)
                    {
                        if (pm.X > (BBT.X - 20) && pm.X < (BBT.X + 20))
                        {
                            if (pm.Y > (BBT.Y - 20) && pm.Y < (BBT.Y + 20))
                            {
                                if (BBT.IsSpecial)
                                {
                                    if (Hue == 1260) //Fire
                                    {
                                        pm.PlaySound(855);
                                    }
                                    if (Hue == 1266) //Ice
                                    {
                                        pm.PlaySound(20);
                                    }
                                    if (Hue == 1272) //Toxic
                                    {
                                        pm.PlaySound(1140);
                                    }
                                    if (Hue == 1283) //Electric
                                    {
                                        pm.PlaySound(41);
                                        Effects.SendBoltEffect(pm);
                                    }
                                    if (Hue == 1288) //Mist
                                    {
                                        pm.PlaySound(252);
                                    }
                                    if (Hue == 1174) //Explosion
                                    {
                                        pm.PlaySound(519);
                                    }
                                    if (Hue == 1177) //Stone
                                    {
                                        pm.PlaySound(515);
                                    }
                                    if (Hue == 1287) //Shiny
                                    {
                                        pm.PlaySound(492);
                                    }
                                }
                                else
                                {
                                    pm.PlaySound(0x20F);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }

        private class MoveTimer : Timer
        {
            private readonly PlayerMobile pm;
            private readonly BBTravel i_Gate;

            public MoveTimer(PlayerMobile player, BBTravel gate) : base(TimeSpan.FromMilliseconds(moveDelay))
            {
                pm = player;
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                pm.MoveToWorld(i_Gate.New_Location, i_Gate.New_Map);

                if (i_Gate.IsSpecial)
                {
                    Point3D point3D = new Point3D(pm.X, pm.Y, pm.Z);

                    if (i_Gate.Hue == 1260) //Fire
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x3709, 30);
                    if (i_Gate.Hue == 1266) //Ice
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x37CC, 40);
                    if (i_Gate.Hue == 1272) //Toxic
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x374A, 17);
                    if (i_Gate.Hue == 1283) //Electric
                        Effects.SendBoltEffect(pm);
                    if (i_Gate.Hue == 1288) //Mist
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x3728, 13);
                    if (i_Gate.Hue == 1174) //Explosion
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x36BD, 10);
                    if (i_Gate.Hue == 1177) //Stone
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x37C4, 31);
                    if (i_Gate.Hue == 1287) //Shiny
                        Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x375A, 30);
                }
                else
                {
                    BBTravelEnd i_BlackBoxEnd = new BBTravelEnd(i_Gate.Custom_Hue, pm.Name);

                    i_BlackBoxEnd.MoveToWorld(i_Gate.New_Location, i_Gate.New_Map);
                    Effects.SendLocationEffect(i_Gate.New_Location, i_Gate.New_Map, 0x3728, 13);
                }
                i_Gate.StartSound(i_Gate);

                if (i_Gate.IsSpecial)
                {
                    i_Gate.Delete();
                }
                Stop();
            }
        }

        private class RevealTimer : Timer
        {
            private readonly PlayerMobile pm;

            public RevealTimer(PlayerMobile player) : base(TimeSpan.FromMilliseconds(revealDelay))
            {
                pm = player;
            }

            protected override void OnTick()
            {
                pm.Hidden = false;
                Stop();
            }
        }

        private class SoundTimer : Timer
        {
            private readonly BBTravel i_Gate;

            public SoundTimer(BBTravel gate) : base(TimeSpan.FromMilliseconds(soundDelay))
            {
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                i_Gate.StartSound(i_Gate);
                Stop();
            }
        }

        private class QuickDeleteTimer : Timer
        {
            private readonly BBTravel i_Gate;

            public QuickDeleteTimer(BBTravel gate) : base(TimeSpan.FromMilliseconds(q_DeleteDelay))
            {
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                i_Gate.Delete();
                Stop();
            }
        }

        private class DeleteTimer : Timer
        {
            private readonly BBTravel i_Gate;

            public DeleteTimer(BBTravel gate) : base(TimeSpan.FromSeconds(deleteDelay))
            {
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                i_Gate.StartSound(i_Gate);
                i_Gate.Delete();
                Stop();
            }
        }
    }

    class BBTravelEnd : Item
    {
        public string Owner { get; set; }

        private DeleteTimer Delete_Timer { get; set; }
        private const int deleteDelay = 10;

        private SoundTimer Sound_Timer { get; set; }
        private const int soundDelay = 50;

        [Constructable]
        public BBTravelEnd(int hue, string owner)
        {
            Name = "Black Box Portal Exit";
            Owner = owner;

            Movable = false;
            ItemID = 0xF6C;
            Hue = hue;
            Light = LightType.Circle300;

            StartDeleteGate(this);

            StartingSound(this);
        }

        private void StartDeleteGate(BBTravelEnd gate)
        {
            if (gate != null)
            {
                Delete_Timer = new DeleteTimer(gate);
                Delete_Timer.Start();
            }
        }

        public BBTravelEnd(Serial serial) : base(serial)
        {
        }

        public override bool OnMoveOver(Mobile m)
        {
            return false;
        }

        private void StartingSound(BBTravelEnd gate)
        {
            if (gate != null)
            {
                Sound_Timer = new SoundTimer(gate);
                Sound_Timer.Start();
            }
        }

        private void StartSound(BBTravelEnd BBT)
        {
            foreach (Mobile player in World.Mobiles.Values)
            {
                PlayerMobile pm = player as PlayerMobile;

                if (pm != null && BBT != null)
                {
                    if (pm.Map == BBT.Map)
                    {
                        if (pm.X > (BBT.X - 20) && pm.X < (BBT.X + 20))
                        {
                            if (pm.Y > (BBT.Y - 20) && pm.Y < (BBT.Y + 20))
                            {
                                pm.PlaySound(0x20F);
                            }
                        }
                    }
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }

        private class SoundTimer : Timer
        {
            private readonly BBTravelEnd i_Gate;

            public SoundTimer(BBTravelEnd gate) : base(TimeSpan.FromMilliseconds(soundDelay))
            {
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                i_Gate.StartSound(i_Gate);
                Stop();
            }
        }

        private class DeleteTimer : Timer
        {
            private readonly BBTravelEnd i_Gate;

            public DeleteTimer(BBTravelEnd gate) : base(TimeSpan.FromSeconds(deleteDelay))
            {
                i_Gate = gate;
            }

            protected override void OnTick()
            {
                i_Gate.StartSound(i_Gate);
                i_Gate.Delete();
                Stop();
            }
        }
    }
}
