#region References
using Server.Spells;
using Server.Spells.Fourth;
using Server.Spells.Second;
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
			var select = 2;
			
			if (c == null || !c.Alive)
				return null;

			Spell spell = null;
			
			switch (Utility.Random(select))
			{
				case 0:
					spell = new HarmSpell(m_Mobile, null);
					break;
				case 1:
					spell = new CurseSpell(m_Mobile, null);
					break;
			}
			
			return spell;
		}
	}
}
