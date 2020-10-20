#region References
using Server.Spells;
#endregion

namespace Server.Mobiles
{
	public class EnergyVortexAI : MageAI
	{
        public EnergyVortexAI(BaseCreature m)
			: base(m)
		{ }
		
		public override Spell ChooseSpell(IDamageable c)
		{
			if (c == null || !c.Alive)
				return null;

			Spell spell = null;
			
			switch (Utility.Random(select))
			{
				case 0:
					spell = new HarmSpell(m_Mobile, null);
				case 1:
					spell = new CurseSpell(m_Mobile, null);
			}
			
			return spell;
		}
	}
}