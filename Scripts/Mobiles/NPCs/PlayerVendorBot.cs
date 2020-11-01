using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Server.Items;
using Server.Mobiles;
using Server.Misc;

namespace Server
{
    public class PVBotSettings
    {
	public static bool EnableSimulatedSales = true; // master toggle for simulated sales
	public static bool EnableRandomAds = true; // if true, items for sale will be advertised on AdTimer tick
	public static bool ChargeForServicePerUODay = true; // if true, the vendor will charge money for its service every UO day (and not every real day)
	public static bool EnableSpeech = true; // if enabled, this NPC will respond to certain keywords when talked to

	// how often and how rigorously to perform a sale check
	public static int MaxIterationsPerCheck = 8; // indicates how many times items will be checked at each sale check
	public static int MinCheckPeriodInMinutes = 5; // indicates the minimum period of time between sale checks
	public static int MaxCheckPeriodInMinutes = 45; // indicates the maximum period of time between sale checks

	public static double AdPeriodInSeconds = 55.0; // how often to advertise sales (there's always a 50% chance of advertising or not advertising an item at this period)

	public static int NumRecentSalesToRemember = 50; // how many sales will be remembered and reported on "<NAME> report"
	public static bool LowPriceBoost = false; // boost the price of items normally priced at 1 gp by a little; not recommended, may make it easier to game the system on low sell price (1 gp) items
	public static int MinimalPriceMaxBoost = 4; // the maximum amount in gold that the low price of items would be boosted by, for the purpose of the option above
	public static bool HarderBagSale = true; // if enabled, makes it harder to get a decent price on items sold en masse in bags
	public static int BarterValue = 33; // %, indicates the percentage of the price from SB price lists at which the item will be considered
	public static int RichSuckerChance = 997; // % based around 1000, with 1000 indicating 100.0%, 997 indicating 99.7%, etc. The "rich fatcat" chance of overpaying for the item
	public static int ImprovedPriceModChance = 97; // %, the chance that the item will sell with a price multiplier
	public static int MinImprovedPriceMod = 2; // The min price multiplier for the option above
	public static int MaxImprovedPriceMod = 4; // The max price multiplier for the option above
	public static int MinAttrsMultiplier = 50; // %, the minimum multiplier for the price coming from item attributes
	public static int MaxAttrsMultiplier = 200; // %, the maximum multiplier for the price coming from item attributes
	public static int RichSuckerMinPrice = 100; // The min price that a rich fatcat sucker will be willing to pay for the item (even if the item would normally cost less, it'll be set to this value)
	public static int MinPriceModifier = 2; // the min price multiplier applied randomly
	public static int MaxPriceModifier = 10; // the max price multiplier applied randomly
	public static int RichSuckerMinPriceMultiplier = 5; // The min price multiplier applied to the rich fatcat sucker willing to overpay for the item
	public static int RichSuckerMaxPriceMultiplier = 10; // The max price multiplier applied to the rich fatcat sucker willing to overpay for the item
	public static bool AnnounceSalesOnlyIfClose = true; // The player vendor will only announce sales when his/her owner is close enough
	public static int DistToAnnounceSales = 15; // The distance to the player vendor at which the owner has to be for the vendor to announce sales
	public static int SBListMaxFixed = 10; // How many SB price lists will be considered at each sale check iteration
	public static int SBListMaxRandom = 10; // How many additional SB price lists will be considered at each iteration in addition to the fixed amount above
	public static int PriceThresholdForAttributeCheck = 2500000; // The items priced at above this value will no longer receive further bonuses from attributes
	public static bool PreAOSResourceBonus = false; // if true, resource type will be accounted for when deciding item worth, good for Pre-AoS (T2A, UOR, etc.) shards.
	public static bool PreAOSQualityBonus = false; // if true, will apply a bigger quality bonus (1.5x / 0.5x) to items, which is good for Pre-AoS (T2A, UOR, etc.) shards.
	public static bool IncreasePriceBasedOnNumberOfProps = true; // if true, items with many beneficial props will sell for more money
	public static int AttrsMod1Or2Props = 1; // price multiplier if the item has 1-2 beneficial props
	public static int AttrsMod3Or4Props = 2; // price multiplier if the item has 3-4 beneficial props
	public static int AttrsMod5Or6Props = 5; // price multiplier if the item has 5-6 beneficial props
	public static int AttrsMod7Or8Props = 10; // price multiplier if the item has 7-8 beneficial props
	public static int AttrsMod9OrMoreProps = 20; // price multiplier if the item has 9+ beneficial props
	public static int AttrsIntensityThreshold = 10; // threshold for attribute intensity to count toward the number of beneficial props (0 = any intensity, otherwise needs to be greater than the number specified)
	public static int IntensityPercentile = 20; // for each N% intensity, give a payout bonus equal to intensity multiplied by the multiplied below
	public static int IntensityMultiplier = 2; // for each N% intensity, give an additional intensity multiplier

	public static int PriceCutOnMaxDurability25 = 90; // %, the max price percentage when max durability is at 25 or less
	public static int PriceCutOnMaxDurability20 = 75; // %, the max price percentage when max durability is at 20 or less
	public static int PriceCutOnMaxDurability15 = 50; // %, the max price percentage when max durability is at 15 or less
	public static int PriceCutOnMaxDurability10 = 25; // %, the max price percentage when max durability is at 10 or less
	public static int PriceCutOnMaxDurability5 = 5; // %, the max price percentage when max durability is at 5 or less
	public static int PriceCutOnMaxDurability3 = 1; // %, the max price percentage when max durability is at 3 or less

	public static int FinalPriceModifier = 100; // % - the final price after all bonuses will be modified to this percentage (e.g. the final price of 1000 will be set to 800 if the 80% modifier is applied); use this to fine tune the prices without affecting the overall balance above
    }

    public class PVBotUtils
    {
	public static bool HandleBotSpeech(PlayerVendor pv, Mobile from, SpeechEventArgs e)
	{
	    bool handled = false;

	    // BaseConvo-like speech
	    string title = from.Female ? "milady" : "milord";
	    if (!handled && PVBotSettings.EnableSimulatedSales && (pv.WasNamed(e.Speech) || Insensitive.StartsWith(e.Speech, "vendor ")) && Insensitive.Contains(e.Speech, "report") || Insensitive.Contains(e.Speech, "recent") || Insensitive.Contains(e.Speech, "sales"))
	    {
		if (pv.IsOwner( from ))
		{
		    if (pv.RecentSales.Count > 0)
		    {
			int reported = 0;
			int numToReport = 5;

			String requestNumber = Regex.Match(e.Speech, @"\d+").Value;
			int requestNumberInt = 0;
			int.TryParse(requestNumber, out requestNumberInt);
			if (requestNumberInt > 0) numToReport = requestNumberInt;

			int startPos = pv.RecentSales.Count - numToReport;
			if (startPos < 0)
			    startPos = 0;

			for (int i = startPos; i < startPos + numToReport && i < pv.RecentSales.Count; i++)
			{
			    pv.SayTo( from, pv.RecentSales[i] );
			}
		    }
		    else
		    {
			pv.SayTo( from, "There were no recent sales, " + title + ".");
		    }
		}
		else
		{
		    pv.SayTo( from, "Sorry, " + title + ", I only respond to the shop owner." );
		}
		handled = true;
	    }
	    if (!handled && PVBotSettings.EnableSimulatedSales && PVBotSettings.EnableSpeech && pv.IsOwner(from) && (pv.WasNamed(e.Speech) || Insensitive.StartsWith(e.Speech, "vendor ")))
	    {
		if (Insensitive.Contains(e.Speech, "forget") || Insensitive.Contains(e.Speech, "wipe") || Insensitive.Contains(e.Speech, "clear") || Insensitive.Contains(e.Speech, "new"))
		{
		    pv.SayTo( from, "Very well, " + title + ", I will start a new list of sales." );
		    pv.RecentSales.Clear();
		    e.Handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "job") || Insensitive.Contains(e.Speech, "work") || Insensitive.Contains(e.Speech, "profession") || Insensitive.Contains(e.Speech, "occupation") || Insensitive.Contains(e.Speech, "explain"))
		{
		    int r = Utility.RandomMinMax(1, 3);
		    switch(r)
		    {
			case 1:
			    pv.SayTo( from, "I will do my best to sell thy goods for the price that you specify. Thou canst also provide an optional item description." );
			    break;
			case 2:
			    pv.SayTo( from, "I sell thy goods to the passing adventurers, " + title + ", at the minimum price that thou specify." );

			    break;
			case 3:
			    pv.SayTo( from, "I am thy personal merchant, in charge of advertising and selling goods to passing adventurers for the price that thou specify." );
			    break;
			default:
			    break;
		    }
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "sell") || Insensitive.Contains(e.Speech, "item"))
		{
		    pv.SayTo( from, "To put the item up for sale, put it in my inventory and specify the price. You can also specify an optional description." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "price"))
		{
		    pv.SayTo( from, "Make sure the price for the item is reasonable, " + title + ". If the item doesn't sell, thou should consider reducing the price." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "description"))
		{
		    pv.SayTo( from, "Thou can specify an item description after a space when you specify the price. I will use it in my advertisements." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "free"))
		{
		    pv.SayTo( from, "Thou can give an item away for free to the first interested adventurer if you specify the price of zero." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "bag") || Insensitive.Contains(e.Speech, "container") || Insensitive.Contains(e.Speech, "chest"))
		{
		    int r = Utility.RandomMinMax(1, 2);
		    switch(r)
		    {
			case 1:
			    pv.SayTo( from, "Items inside bags and other containers will be sold too, if you specify the price of -1, just don't put bags inside bags, adventurers have problems finding those." );
			    break;
			case 2:
			    pv.SayTo( from, "Thou can also try to sell the bag and its contents as a package deal if you specify the price for the container itself." );
			    break;
			default:
			    break;
		    }
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "payment") || Insensitive.Contains(e.Speech, "charge"))
		{
		    pv.SayTo( from, "I charge thee a small sum of gold for my services every day. If I don't receive my payment on time, I will leave your premises.");
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "rob") || Insensitive.Contains(e.Speech, "steal") || Insensitive.Contains(e.Speech, "thief"))
		{
		    int r = Utility.RandomMinMax(1, 2);
		    switch(r)
		    {
			case 1:
			    pv.SayTo( from, "Thy goods are safe with me, " + title + ". No one can steal them.");
			    break;
			case 2:
			    pv.SayTo( from, "A thief could kill me and I still couldn't give any of thy goods away." );
			    break;
			default:
			    break;
		    }
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "coin") || Insensitive.Contains(e.Speech, "money") || Insensitive.Contains(e.Speech, "currency"))
		{
		    pv.SayTo( from, "Thou can give me some gold to cover the daily service payments." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "copper") || Insensitive.Contains(e.Speech, "jewel"))
		{
		    pv.SayTo( from, "Sorry, I only accept gold for my services." );
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "name"))
		{
		    pv.SayTo( from, "Why, I'm " + pv.Name + ", " + title + ".");
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "greetings") || Insensitive.Contains(e.Speech, "hail") || Insensitive.Contains(e.Speech, "hello"))
		{
		    int r = Utility.RandomMinMax(1, 2);
		    switch(r)
		    {
			case 1:
			    pv.SayTo( from, "Greetings, " + title + "." );
			    break;
			case 2:
			    pv.SayTo( from, "Hail, " + title + "." );
			    break;
			default:
			    break;
		    }
		    handled = true;
		}
		else if (Insensitive.Contains(e.Speech, "goodbye") || Insensitive.Contains(e.Speech, "farewell") || Insensitive.Contains(e.Speech, "bye"))
		{
		    int r = Utility.RandomMinMax(1, 2);
		    switch(r)
		    {
			case 1:
			    pv.SayTo( from, "Farewell, " + title + "." );
			    break;
			case 2:
			    pv.SayTo( from, "Safe travels, " + title + "." );
			    break;
			default:
			    break;
		    }
		    handled = true;
		}
	    }
	    // - PV update -

	    return handled;
	}

	private static string AddSpacesToSentence(string text)
	{
	    StringBuilder newText = new StringBuilder(text.Length * 2);
	    newText.Append(text[0]);
	    for (int i = 1; i < text.Length; i++)
	    {
		if (char.IsUpper(text[i]) && text[i - 1] != ' ')
		    newText.Append(' ');
		newText.Append(text[i]);
	    }
	    return newText.ToString();
	}

	public static string GetItemName( Item item )
	{

	    string Label = item.Name;
	    TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
	    if ( Label != null && Label != "" ){} else { Label = AddSpacesToSentence( (item.GetType()).Name ); }
	    //if ( Server.Misc.MaterialInfo.GetMaterialName( item ) != "" ){ Label = Server.Misc.MaterialInfo.GetMaterialName( item ) + " " + item.Name; } // TODO
	    Label = cultInfo.ToTitleCase(Label);

	    return Label;
	}

    }

    public class RandomAdTimer : Timer
    {
	private PlayerVendor m_Vendor;

	public RandomAdTimer( PlayerVendor vendor, TimeSpan delay ) : base( delay, GetInterval() )
	{
	    m_Vendor = vendor;
	    Priority = TimerPriority.FiveSeconds;
	}

	public static TimeSpan GetInterval()
	{
	    return TimeSpan.FromSeconds(PVBotSettings.AdPeriodInSeconds);
	}

	protected override void OnTick()
	{
	    if (m_Vendor == null || m_Vendor.Backpack == null || !Utility.RandomBool())
		return;

	    List<String> list = new List<String>();

	    foreach ( Item item in m_Vendor.Backpack.Items )
	    {
		VendorItem vi = m_Vendor.GetVendorItem( item );
		bool banned = item is Gold;

		if ( vi != null && (vi.IsForSale || vi.IsForFree) && vi.Price != 999 && !banned)
		{
		    string Desc = vi.Description;
		    string Label = PVBotUtils.GetItemName(item);

		    if (Desc != String.Empty || Label != String.Empty)
			list.Add(Desc != String.Empty ? Desc : Label);
		}

		if ( item is Container && vi != null && vi.Price < 0 ) // containers not for sale
		{
		    foreach( Item ins in item.Items )
		    {
			if (!(ins is Container))
			{
			    VendorItem vii = m_Vendor.GetVendorItem( ins );
			    bool insBanned = ins is Gold;
			    if ( vii != null && (vii.IsForSale || vii.IsForFree) && vii.Price != 999 && !insBanned)
			    {
				string Desc = vi.Description;
				string Label = PVBotUtils.GetItemName(ins);

				if (Desc != String.Empty || Label != String.Empty)
				    list.Add(Desc != String.Empty ? Desc : Label);
			    }
			}
		    }
		}
	    }

	    if (list.Count == 0)
		return;

	    String AdTarget = list[Utility.Random(list.Count)];

	    // advertise stuff
	    
	    if (AdTarget.StartsWith("@") && AdTarget.Length > 1)
	    {
		// custom advertisement
		AdTarget = AdTarget.Substring(1);
		m_Vendor.Say(AdTarget);
		return;
	    }

	    switch(Utility.RandomMinMax( 0, 4 ))
	    {
		case 0:
		    m_Vendor.Say("Hurry, hurry! " + AdTarget + " for sale!");
		    break;
		case 1:
		    m_Vendor.Say("Step right up, we have " + AdTarget + "!");
		    break;
		case 2:
		    m_Vendor.Say(AdTarget + " for sale");
		    break;
		case 3:
		    m_Vendor.Say("Check out " + AdTarget + "! It's a good deal!");
		    break;
		case 4:
		default:
		    m_Vendor.Say(AdTarget + " for a great price!");
		    break;
	    }
	}
    }
    
    public class RandomBuyTimer : Timer
    {
	private PlayerVendor m_Vendor;

	public RandomBuyTimer( PlayerVendor vendor, TimeSpan delay ) : base( delay, GetInterval() )
	{
	    m_Vendor = vendor;
	    Priority = TimerPriority.OneMinute;
	}

	public static TimeSpan GetInterval()
	{
	    return GetRandomBuyInterval();
	}

	private static TimeSpan GetRandomBuyInterval()
	{
	    int nextPurchase = Utility.RandomMinMax(PVBotSettings.MinCheckPeriodInMinutes, PVBotSettings.MaxCheckPeriodInMinutes);

	    return TimeSpan.FromMinutes((double)nextPurchase);
	}

	private void SetupSBList( ref List<SBInfo> sbList )
	{
	    sbList.Clear();
	    for (int i = 0; i < Utility.RandomMinMax( 1, PVBotSettings.SBListMaxRandom ) + PVBotSettings.SBListMaxFixed; i++)
	    {
		int sbListID = Utility.RandomMinMax( 1, 89 );
		switch(sbListID)
		{
		    case 1: { sbList.Add(new SBAlchemist(m_Vendor)); break; }
		    case 2: { sbList.Add(new SBAnimalTrainer()); break; }
		    case 3: { sbList.Add(new SBArchitect()); break; }
		    case 4: { sbList.Add(new SBAxeWeapon()); break; }
		    case 5: { sbList.Add(new SBBaker()); break; }
		    case 6: { sbList.Add(new SBBanker()); break; }
		    case 7: { sbList.Add(new SBBard()); break; }
		    case 8: { sbList.Add(new SBBarkeeper()); break; }
		    case 9: { sbList.Add(new SBBeekeeper()); break; }
		    case 10: { sbList.Add(new SBBlacksmith()); break; }
		    case 11: { sbList.Add(new SBBowyer()); break; }
		    case 12: { sbList.Add(new SBButcher()); break; }
		    case 13: { sbList.Add(new SBCarpenter()); break; }
		    case 14: { sbList.Add(new SBCarpets()); break; }
		    case 15: { sbList.Add(new SBChainmailArmor()); break; }
		    case 16: { sbList.Add(new SBCobbler()); break; }
		    case 17: { sbList.Add(new SBCook()); break; }
		    case 18: { sbList.Add(new SBFarmer()); break; }
		    case 19: { sbList.Add(new SBFisherman()); break; }
		    case 20: { sbList.Add(new SBFortuneTeller()); break; }
		    case 21: { sbList.Add(new SBFurtrader()); break; }
		    case 22: { sbList.Add(new SBGardener()); break; }
		    case 23: { sbList.Add(new SBGlassblower()); break; }
		    case 24: { sbList.Add(new SBHairStylist()); break; }
		    case 25: { sbList.Add(new SBHealer()); break; }
		    case 26: { sbList.Add(new SBHelmetArmor()); break; }
		    case 27: { sbList.Add(new SBHerbalist()); break; }
		    case 28: { sbList.Add(new SBHolyMage()); break; }
		    case 29: { sbList.Add(new SBHouseDeed()); break; }
		    case 31: { sbList.Add(new SBInnKeeper()); break; }
		    case 32: { sbList.Add(new SBJewel()); break; }
		    case 33: { sbList.Add(new SBKeeperOfBushido()); break; }
		    case 34: { sbList.Add(new SBKeeperOfChivalry()); break; }
		    case 35: { sbList.Add(new SBKeeperOfNinjitsu()); break; }
		    case 36: { sbList.Add(new SBKnifeWeapon()); break; }
		    case 37: { sbList.Add(new SBLeatherArmor()); break; }
		    case 38: { sbList.Add(new SBLeatherWorker()); break; }
		    case 39: { sbList.Add(new SBMaceWeapon()); break; }
		    case 40: { sbList.Add(new SBMage()); break; }
		    case 41: { sbList.Add(new SBMapmaker()); break; }
		    case 42: { sbList.Add(new SBMetalShields()); break; }
		    case 43: { sbList.Add(new SBMiller()); break; }
		    case 44: { sbList.Add(new SBMiner()); break; }
		    case 45: { sbList.Add(new SBMonk()); break; }
		    case 46: { sbList.Add(new SBMystic()); break; }
		    case 47: { sbList.Add(new SBNecromancer()); break; }
		    case 48: { sbList.Add(new SBPlateArmor()); break; }
		    case 49: { sbList.Add(new SBPlayerBarkeeper()); break; }
		    case 50: { sbList.Add(new SBPoleArmWeapon()); break; }
		    case 51: { sbList.Add(new SBProvisioner()); break; }
		    case 52: { sbList.Add(new SBRancher()); break; }
		    case 53: { sbList.Add(new SBRangedWeapon()); break; }
		    case 54: { sbList.Add(new SBRanger()); break; }
		    case 55: { sbList.Add(new SBRealEstateBroker()); break; }
		    case 56: { sbList.Add(new SBRingmailArmor()); break; }
		    case 57: { sbList.Add(new SBSAArmor()); break; }
		    case 58: { sbList.Add(new SBSABlacksmith()); break; }
		    case 59: { sbList.Add(new SBSATailor()); break; }
		    case 60: { sbList.Add(new SBSATanner()); break; }
		    case 61: { sbList.Add(new SBSAWeapons()); break; }
		    case 62: { sbList.Add(new SBScribe(m_Vendor)); break; }
		    case 63: { sbList.Add(new SBSEArmor()); break; }
		    case 64: { sbList.Add(new SBSEBowyer()); break; }
		    case 65: { sbList.Add(new SBSECarpenter()); break; }
		    case 66: { sbList.Add(new SBSECook()); break; }
		    case 67: { sbList.Add(new SBSEFood()); break; }
		    case 68: { sbList.Add(new SBSEHats()); break; }
		    case 69: { sbList.Add(new SBSELeatherArmor()); break; }
		    case 70: { sbList.Add(new SBSEWeapons()); break; }
		    case 71: { sbList.Add(new SBShipwright(m_Vendor)); break; }
		    case 72: { sbList.Add(new SBSmithTools()); break; }
		    case 73: { sbList.Add(new SBSpearForkWeapon()); break; }
		    case 74: { sbList.Add(new SBStavesWeapon()); break; }
		    case 75: { sbList.Add(new SBStoneCrafter()); break; }
		    case 76: { sbList.Add(new SBStuddedArmor()); break; }
		    case 77: { sbList.Add(new SBSwordWeapon()); break; }
		    case 78: { sbList.Add(new SBTailor()); break; }
		    case 79: { sbList.Add(new SBTanner()); break; }
		    case 80: { sbList.Add(new SBTavernKeeper()); break; }
		    case 81: { sbList.Add(new SBThief()); break; }
		    case 82: { sbList.Add(new SBTinker(null)); break; }
		    case 83: { sbList.Add(new SBVagabond()); break; }
		    case 84: { sbList.Add(new SBVarietyDealer()); break; }
		    case 85: { sbList.Add(new SBVeterinarian()); break; }
		    case 86: { sbList.Add(new SBWaiter()); break; }
		    case 87: { sbList.Add(new SBWeaponSmith()); break; }
		    case 88: { sbList.Add(new SBWeaver()); break; }
		    case 89: { sbList.Add(new SBWoodenShields()); break; }
		}
	    }
	}

	// The lists below must correspond to the enum definitions in AOS.cs. The number of elements
	// must strictly correspond to the number of elements in the AOS enums, or the game will crash.
	private int[] AosAttributeIntensities = {
	    10, // RegenHits
	    10, // RegenStam
	    10, // RegenMana
	    25, // DefendChance
	    25, // AttackChance
	    25, // BonusStr
	    25, // BonusDex
	    25, // BonusInt
	    25, // BonusHits
	    25, // BonusStam
	    25, // BonusMana
	    50, // WeaponDamage
	    50, // WeaponSpeed
	    50, // SpellDamage
	    3, // CastRecovery
	    3, // CastSpeed
	    25, // LowerManaCost
	    25, // LowerRegCost
	    50, // ReflectPhysical
	    50, // EnhancePotions,
	    150, // Luck
	    1, // SpellChanneling
	    1, // NightSight
	    1, // IncreasedKarmaLoss (FIXME!)
	    1, // Brittle
	    100, // LowerAmmoCost
	    1 // BalancedWeapon
	};

	private int[] AosWeaponAttributeIntensities = {
	    50, // LowerStatReq
	    5, // SelfRepair
	    50, // HitLeechHits
	    50, // HitLeechStam
	    50, // HitLeechMana
	    50, // HitLowerAttack
	    50, // HitLowerDefend
	    50, // HitMagicArrow
	    50, // HitHarm
	    50, // HitFireball
	    50, // HitLightning
	    50, // HitDispel
	    50, // HitColdArea
	    50, // HitFireArea
	    50, // HitPoisonArea
	    50, // HitEnergyArea
	    50, // HitPhysicalArea
	    15, // ResistPhysicalBonus
	    15, // ResistFireBonus
	    15, // ResistColdBonus
	    15, // ResistPoisonBonus
	    15, // ResistEnergyBonus
	    1, // UseBestSkill
	    1, // MageWeapon
	    100, // DurabilityBonus
	    1, // BloodDrinker
	    1, // BattleLust
	    50, // HitCurse
	    50, // HitFatigue
	    50, // HitManaDrain
	    30, // SplinteringWeapon
	    1 // ReactiveParalyze
	};

	private int[] AosExtendedWeaponAttributeIntensities = {
	    1, // BoneBreaker
	    1, // HitSwarm
	    1, // HitSparks
	    1, // Bane
	    1, // MysticWeapon
	    1, // AssassinHoned
	    1, // Focus
	    1 // HitExplosion
	};

	private int[] AosArmorAttributeIntensities = {
	    50, // LowerStatReq
	    5, // SelfRepair
	    1, // MageArmor
	    100, // DurabilityBonus
	    1, // ReactiveParalyze
	    30 // SoulCharge
	};

	private int[] AosElementAttributeIntensities = {
	    1100, // Physical - avoid overvaluing weapons with 100% physical
	    100, // Fire
	    100, // Cold
	    100, // Poison
	    100, // Energy
	    100, // Chaos
	    100, // Direct
	};

	private int[] SAAbsorptionAttributeIntensities = {
	    15, // EaterFire
	    15, // EaterCold
	    15, // EaterPoison
	    15, // EaterEnergy
	    15, // EaterKinetic
	    15, // EaterDamage
	    20, // ResonanceFire
	    20, // ResonanceCold
	    20, // ResonancePoison
	    20, // ResonanceEnergy
	    20, // ResonanceKinetic
	    1, // SoulChargeFire
	    1, // SoulChargeCold
	    1, // SoulChargePoison
	    1, // SoulChargeEnergy
	    1, // SoulChargeKinetic
	    3 // CastingFocus
	};

	private int MaxSkillIntensity = 15; // FIXME: 12?
	private int MaxResistanceIntensity = 25;
	private int ResistanceIntensityCountsAsProp = 90; // %

	private enum IntensityMode
	{
	    AosAttribute,
	    AosWeaponAttribute,
	    AosArmorAttribute,
	    AosElementAttribute,
	    SkillBonus,
	    ResistanceBonus,
	    RunicToolProperties,
	    // ServUO
	    AosExtendedWeaponAttribute,
	    SAAbsorptionAttribute
	}

	private void AddSkillBonuses(double skill1, double skill2, double skill3, double skill4, double skill5, ref int attrsMod, ref int props)
	{
	    int NormalizedSkillBonus1 = (int)skill1 * 100 / MaxSkillIntensity;
	    int NormalizedSkillBonus2 = (int)skill2 * 100 / MaxSkillIntensity;
	    int NormalizedSkillBonus3 = (int)skill3 * 100 / MaxSkillIntensity;
	    int NormalizedSkillBonus4 = (int)skill4 * 100 / MaxSkillIntensity;
	    int NormalizedSkillBonus5 = (int)skill5 * 100 / MaxSkillIntensity;

	    if(NormalizedSkillBonus1 > 0 && NormalizedSkillBonus1 >= PVBotSettings.AttrsIntensityThreshold) ++props;
	    if(NormalizedSkillBonus2 > 0 && NormalizedSkillBonus2 >= PVBotSettings.AttrsIntensityThreshold) ++props;
	    if(NormalizedSkillBonus3 > 0 && NormalizedSkillBonus3 >= PVBotSettings.AttrsIntensityThreshold) ++props;
	    if(NormalizedSkillBonus4 > 0 && NormalizedSkillBonus4 >= PVBotSettings.AttrsIntensityThreshold) ++props;
	    if(NormalizedSkillBonus5 > 0 && NormalizedSkillBonus5 >= PVBotSettings.AttrsIntensityThreshold) ++props;

	    attrsMod += (int)skill1 * (NormalizedSkillBonus1 / 2);
	    attrsMod += (int)skill2 * (NormalizedSkillBonus2 / 2);
	    attrsMod += (int)skill3 * (NormalizedSkillBonus3 / 2);
	    attrsMod += (int)skill4 * (NormalizedSkillBonus4 / 2);
	    attrsMod += (int)skill5 * (NormalizedSkillBonus5 / 2);
	}

	private void AddResistanceBonuses(int physical, int fire, int cold, int poison, int energy, ref int attrsMod, ref int props)
	{
	    int NormalizedPhysicalResistance = physical * 100 / MaxResistanceIntensity;
	    int NormalizedFireResistance = fire * 100 / MaxResistanceIntensity;
	    int NormalizedColdResistance = cold * 100 / MaxResistanceIntensity;
	    int NormalizedPoisonResistance = poison * 100 / MaxResistanceIntensity;
	    int NormalizedEnergyResistance = energy * 100 / MaxResistanceIntensity;

	    if (NormalizedPhysicalResistance >= ResistanceIntensityCountsAsProp) ++props;
	    if (NormalizedFireResistance >= ResistanceIntensityCountsAsProp) ++props;
	    if (NormalizedColdResistance >= ResistanceIntensityCountsAsProp) ++props;
	    if (NormalizedPoisonResistance >= ResistanceIntensityCountsAsProp) ++props;
	    if (NormalizedEnergyResistance >= ResistanceIntensityCountsAsProp) ++props;

	    attrsMod += physical * (NormalizedPhysicalResistance / 10);
	    attrsMod += fire * (NormalizedFireResistance / 10);
	    attrsMod += cold * (NormalizedColdResistance / 10);
	    attrsMod += poison * (NormalizedPoisonResistance / 10);
	    attrsMod += energy * (NormalizedEnergyResistance / 10);
	}

	private void AddResourceBonuses(CraftResource resource, ref int attrsMod)
	{
	    switch ( resource )
	    {
		case CraftResource.DullCopper: attrsMod = (int)( attrsMod * 1.25 ); break;
		case CraftResource.ShadowIron: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.Copper: attrsMod = (int)( attrsMod * 1.75 ); break;
		case CraftResource.Bronze: attrsMod = (int)( attrsMod * 2 ); break;
		case CraftResource.Gold: attrsMod = (int)( attrsMod * 2.25 ); break;
		case CraftResource.Agapite: attrsMod = (int)( attrsMod * 2.50 ); break;
		case CraftResource.Verite: attrsMod = (int)( attrsMod * 2.75 ); break;
		case CraftResource.Valorite: attrsMod = (int)( attrsMod * 3 ); break;
		case CraftResource.SpinedLeather: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.HornedLeather: attrsMod = (int)( attrsMod * 1.75 ); break;
		case CraftResource.BarbedLeather: attrsMod = (int)( attrsMod * 2.0 ); break;
		case CraftResource.RedScales: attrsMod = (int)( attrsMod * 1.25 ); break;
		case CraftResource.YellowScales: attrsMod = (int)( attrsMod * 1.25 ); break;
		case CraftResource.BlackScales: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.GreenScales: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.WhiteScales: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.BlueScales: attrsMod = (int)( attrsMod * 1.5 ); break;
		case CraftResource.OakWood: attrsMod = (int)( attrsMod * 1.25 ); break; // TODO: better wood coeffs
		case CraftResource.AshWood: attrsMod = (int)( attrsMod * 1.25 ); break; 
		case CraftResource.YewWood: attrsMod = (int)( attrsMod * 1.5 ); break; 
		case CraftResource.Heartwood: attrsMod = (int)( attrsMod * 2 ); break;
		case CraftResource.Bloodwood: attrsMod = (int)( attrsMod * 2 ); break;
		case CraftResource.Frostwood: attrsMod = (int)( attrsMod * 2 ); break;
	    }
	}

	private void AddQualityBonuses(ItemQuality quality, ref int attrsMod, ref int props)
	{
	    if (PVBotSettings.PreAOSQualityBonus)
	    {
		if ( quality == ItemQuality.Exceptional ) { attrsMod = (int)((double)attrsMod * 1.5); ++props; }
		else if ( quality == ItemQuality.Low ) attrsMod = (int)((double)attrsMod * 0.5);
	    }
	    else
	    {
		if ( quality == ItemQuality.Exceptional ) { attrsMod += 10; ++props; }
		else if ( quality == ItemQuality.Low ) attrsMod -= 10;
	    }
	}

	private void AddNegativeBonuses(int Brittle, int Prized, int Massive, int Unwieldly, int Antique, int NoRepair, ref int attrsMod)
	{
	    if (Brittle > 0) attrsMod -= 150;
	    if (Prized > 0) attrsMod -= 80;
	    if (Massive > 0) attrsMod -= 50;
	    if (Unwieldly > 0) attrsMod -= 50;
	    if (Antique > 0) attrsMod -= 20;
	    if (NoRepair > 0) attrsMod -= 50;
	}

	private void ScalePriceOnDurability(Item item, ref int price)
	{
	    int cur_dur = 0;
	    int max_dur = 0;

	    if (item is BaseWeapon)
	    {
		cur_dur = ((BaseWeapon)item).HitPoints;
		max_dur = ((BaseWeapon)item).MaxHitPoints;
	    }
	    else if (item is BaseArmor)
	    {
		cur_dur = ((BaseArmor)item).HitPoints;
		max_dur = ((BaseArmor)item).MaxHitPoints;
	    }
	    else if (item is BaseClothing)
	    {
		cur_dur = ((BaseClothing)item).HitPoints;
		max_dur = ((BaseClothing)item).MaxHitPoints;
	    }
	    else if (item is BaseShield)
	    {
		cur_dur = ((BaseShield)item).HitPoints;
		max_dur = ((BaseShield)item).MaxHitPoints;
	    }
	    else if (item is BaseJewel)
	    {
		cur_dur = ((BaseJewel)item).HitPoints;
		max_dur = ((BaseJewel)item).MaxHitPoints;
	    }

	    if (cur_dur > 0 && max_dur > 0)
	    {
		if (max_dur <= 3)
		    price = price * PVBotSettings.PriceCutOnMaxDurability3 / 100;
		if (max_dur <= 5)
		    price = price * PVBotSettings.PriceCutOnMaxDurability5 / 100;
		if (max_dur <= 10)
		    price = price * PVBotSettings.PriceCutOnMaxDurability10 / 100;
		if (max_dur <= 15)
		    price = price * PVBotSettings.PriceCutOnMaxDurability15 / 100;
		if (max_dur <= 20)
		    price = price * PVBotSettings.PriceCutOnMaxDurability20 / 100;
		else if (max_dur <= 25)
		    price = price * PVBotSettings.PriceCutOnMaxDurability25 / 100;
	    }
	}

	// BaseWeapon
	private void AddNormalizedBonuses(BaseWeapon bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.AosWeaponAttribute)
	    {
		foreach( long i in Enum.GetValues(typeof( AosWeaponAttribute ) ) ) 
		{
		    int MaxWeaponIntensity = AosWeaponAttributeIntensities[id++];
		    int NormalizedWeaponAttribute = bw.WeaponAttributes[ (AosWeaponAttribute)i ] * 100 / MaxWeaponIntensity;
		    if ( NormalizedWeaponAttribute > 0 && NormalizedWeaponAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxWeaponIntensity > 1 )
			attrsMod += (int)(NormalizedWeaponAttribute * ( (double)NormalizedWeaponAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedWeaponAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.AosExtendedWeaponAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( ExtendedWeaponAttribute ) ) ) 
		{
		    int MaxWeaponIntensity = AosExtendedWeaponAttributeIntensities[id++];
		    int NormalizedWeaponAttribute = bw.ExtendedWeaponAttributes[ (ExtendedWeaponAttribute)i ] * 100 / MaxWeaponIntensity;
		    if ( NormalizedWeaponAttribute > 0 && NormalizedWeaponAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxWeaponIntensity > 1 )
			attrsMod += (int)(NormalizedWeaponAttribute * ( (double)NormalizedWeaponAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedWeaponAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.AosElementAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosElementAttribute ) ) ) 
		{
		    int MaxElemIntensity = AosElementAttributeIntensities[id++];
		    int NormalizedElementalAttribute = bw.AosElementDamages[ (AosElementAttribute)i ] * 100 / MaxElemIntensity;
		    if ( NormalizedElementalAttribute > 0 && NormalizedElementalAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxElemIntensity > 1 )
			attrsMod += (int)(NormalizedElementalAttribute * ( (double)NormalizedElementalAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedElementalAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.AbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for weapon: " + mode);
	    }
	}

	// BaseArmor
	private void AddNormalizedBonuses(BaseArmor bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.AosArmorAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosArmorAttribute ) ) ) 
		{
		    int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
		    int NormalizedArmorAttribute = bw.ArmorAttributes[ (AosArmorAttribute)i ] * 100 / MaxArmorIntensity;
		    if ( NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxArmorIntensity > 1 )
			attrsMod += (int)(NormalizedArmorAttribute * ( (double)NormalizedArmorAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedArmorAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.AbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else if (mode == IntensityMode.ResistanceBonus)
	    {
		AddResistanceBonuses(bw.PhysicalBonus, bw.FireBonus, bw.ColdBonus, bw.PoisonBonus, bw.EnergyBonus,
			ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for armor: " + mode);
	    }
	}

	// BaseShield
	private void AddNormalizedBonuses(BaseShield bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.AosArmorAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosArmorAttribute ) ) ) 
		{
		    int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
		    int NormalizedArmorAttribute = bw.ArmorAttributes[ (AosArmorAttribute)i ] * 100 / MaxArmorIntensity;
		    if ( NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxArmorIntensity > 1 )
			attrsMod += (int)(NormalizedArmorAttribute * ( (double)NormalizedArmorAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedArmorAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.AbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else if (mode == IntensityMode.ResistanceBonus)
	    {
		AddResistanceBonuses(bw.PhysicalBonus, bw.FireBonus, bw.ColdBonus, bw.PoisonBonus, bw.EnergyBonus,
			ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for shield: " + mode);
	    }
	}

	// BaseClothing
	private void AddNormalizedBonuses(BaseClothing bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.AosArmorAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosArmorAttribute ) ) ) 
		{
		    int MaxArmorIntensity = AosArmorAttributeIntensities[id++];
		    int NormalizedArmorAttribute = bw.ClothingAttributes[ (AosArmorAttribute)i ] * 100 / MaxArmorIntensity;
		    if ( NormalizedArmorAttribute > 0 && NormalizedArmorAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxArmorIntensity > 1 )
			attrsMod += (int)(NormalizedArmorAttribute * ( (double)NormalizedArmorAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedArmorAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.SAAbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for clothing: " + mode);
	    }
	}

	// BaseJewel
	private void AddNormalizedBonuses(BaseJewel bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.AosElementAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosElementAttribute ) ) ) 
		{
		    int MaxElemIntensity = AosElementAttributeIntensities[id++];
		    int NormalizedElementalAttribute = bw.Resistances[ (AosElementAttribute)i ] * 100 / MaxElemIntensity;
		    if ( NormalizedElementalAttribute > 0 && NormalizedElementalAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxElemIntensity > 1 )
			attrsMod += (int)(NormalizedElementalAttribute * ( (double)NormalizedElementalAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedElementalAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    }
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.AbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else if (mode == IntensityMode.ResistanceBonus)
	    {
		AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
			ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for jewel: " + mode);
	    }
	}

	// BaseQuiver
	private void AddNormalizedBonuses(BaseQuiver bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else
	    {
		Console.WriteLine("Unexpected mode for quiver: " + mode);
	    }
	}

	// BaseInstrument
	private void AddNormalizedBonuses(BaseInstrument bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.ResistanceBonus)
	    {
		AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
			ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for instrument: " + mode);
	    }
	}

	// Spellbook
	private void AddNormalizedBonuses(Spellbook bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SkillBonus)
	    {
		AddSkillBonuses(bw.SkillBonuses.Skill_1_Value, bw.SkillBonuses.Skill_2_Value, bw.SkillBonuses.Skill_3_Value,
			bw.SkillBonuses.Skill_4_Value, bw.SkillBonuses.Skill_5_Value, ref attrsMod, ref props);
	    }
	    else if (mode == IntensityMode.ResistanceBonus)
	    {
		AddResistanceBonuses(bw.PhysicalResistance, bw.FireResistance, bw.ColdResistance, bw.PoisonResistance, bw.EnergyResistance,
			ref attrsMod, ref props);
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for spellbook: " + mode);
	    }
	}

	// BaseRunicTool
	private void AddNormalizedBonuses(BaseRunicTool bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    if (mode == IntensityMode.RunicToolProperties)
	    {
		attrsMod += 1000;

		AddResourceBonuses(bw.Resource, ref attrsMod);

		attrsMod -= (50 - bw.UsesRemaining) * 30;
		if (attrsMod < 0)
		    attrsMod = 0;
	    }
	    else
	    {
		Console.WriteLine("Unexpected mode for runic tool: " + mode);
	    }
	}

	// BaseTalisman
	private void AddNormalizedBonuses(BaseTalisman bw, IntensityMode mode, ref int attrsMod, ref int props)
	{
	    int id = 0;

	    if (mode == IntensityMode.AosAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( AosAttribute ) ) )
		{
		    int MaxIntensity = AosAttributeIntensities[id++];
		    int NormalizedAttribute = bw.Attributes[ (AosAttribute)i ] * 100 / MaxIntensity;
		    if ( NormalizedAttribute > 0 && NormalizedAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxIntensity > 1 )
			attrsMod += (int)(NormalizedAttribute * ( (double)NormalizedAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else if (mode == IntensityMode.SAAbsorptionAttribute)
	    {
		foreach( int i in Enum.GetValues(typeof( SAAbsorptionAttribute ) ) )
		{
		    int MaxAbsorptionIntensity = SAAbsorptionAttributeIntensities[id++];
		    int NormalizedAbsorptionAttribute = bw.SAAbsorptionAttributes[ (SAAbsorptionAttribute)i ] * 100 / MaxAbsorptionIntensity;
		    if ( NormalizedAbsorptionAttribute > 0 && NormalizedAbsorptionAttribute >= PVBotSettings.AttrsIntensityThreshold ) ++props;

		    if ( MaxAbsorptionIntensity > 1 )
			attrsMod += (int)(NormalizedAbsorptionAttribute * ( (double)NormalizedAbsorptionAttribute / PVBotSettings.IntensityPercentile * PVBotSettings.IntensityMultiplier ));
		    else if ( NormalizedAbsorptionAttribute > 0 )
			attrsMod += Utility.RandomMinMax(50, 100);
		}
	    } 
	    else
	    {
		Console.WriteLine("Unexpected mode for talisman: " + mode);
	    }
	}

	private int GetAttrsMod( Item ii )
	{
	    int attrsMod = 0;
	    int props = 0;
	    int id = 0;

	    if (ii == null)
	    {
		return 0;
	    }

	    if (ii is BaseWeapon)
	    {
		BaseWeapon bw = ii as BaseWeapon;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosWeaponAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosExtendedWeaponAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosElementAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);

		if(bw.Slayer != SlayerName.None) ++props;
		if(bw.Slayer2 != SlayerName.None) ++props;
		if(bw.Slayer3 != TalismanSlayerName.None) ++props;

		if(bw.PoisonCharges > 0 && bw.Poison.Level > 0)
		{
		    ++props;
		    attrsMod += 5 * (int)bw.Poison.Level;
		    attrsMod += 2 * (int)bw.PoisonCharges;
		}

		// Pre-AoS
		if ( (int)bw.DamageLevel > 0 ) { ++props; attrsMod += Utility.Random(80, 150) * (int)bw.DamageLevel; }
		if ( (int)bw.DurabilityLevel > 0 ) { ++props; attrsMod += Utility.Random(50, 90) * (int)bw.DurabilityLevel; }
		if ( (int)bw.AccuracyLevel > 0 ) { ++props; attrsMod += Utility.Random(70, 120) * (int)bw.AccuracyLevel; }

		if (bw.Slayer != SlayerName.None) 
		{
		    attrsMod += 100;
		    props++;
		}
		if (bw.Slayer2 != SlayerName.None) 
		{
		    attrsMod += 100;
		    props++;
		}
		if (bw.Slayer3 != TalismanSlayerName.None) 
		{
		    attrsMod += 100;
		    props++;
		}

		if (props >= 3 && (bw.WeaponAttributes.MageWeapon > 0 || bw.Attributes.SpellChanneling > 0))
		    attrsMod = (int)((double)attrsMod * 1.3);

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);

		AddQualityBonuses(bw.Quality, ref attrsMod, ref props);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseArmor)
	    {
		BaseArmor bw = ii as BaseArmor;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		if(bw.SkillBonuses.Skill_1_Value > 0) ++props;
		if(bw.SkillBonuses.Skill_2_Value > 0) ++props;
		if(bw.SkillBonuses.Skill_3_Value > 0) ++props;
		if(bw.SkillBonuses.Skill_4_Value > 0) ++props;
		if(bw.SkillBonuses.Skill_5_Value > 0) ++props;

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		// Pre-AoS
		if ( (int)bw.Durability > 0 ) { ++props; attrsMod += Utility.Random(50, 80) * (int)bw.Durability; }
		if ( (int)bw.ProtectionLevel > 0 ) { ++props; attrsMod += Utility.Random(70, 100) * (int)bw.ProtectionLevel; }

		if (props >= 3 && bw.ArmorAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
		    attrsMod = (int)((double)attrsMod * 1.3);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);

		AddQualityBonuses(bw.Quality, ref attrsMod, ref props);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseClothing)
	    {
		BaseClothing bw = ii as BaseClothing;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		// Pre-AoS - TODO

		if (props >= 3 && bw.ClothingAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
		    attrsMod = (int)((double)attrsMod * 1.3);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);

		AddQualityBonuses(bw.Quality, ref attrsMod, ref props);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseJewel)
	    {
		BaseJewel bw = ii as BaseJewel;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosElementAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseShield)
	    {
		BaseShield bw = ii as BaseShield;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.AosArmorAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		// Pre-AoS
		if ( (int)bw.Durability > 0 ) { ++props; attrsMod += Utility.Random(50, 80) * (int)bw.Durability; }
		if ( (int)bw.ProtectionLevel > 0 ) { ++props; attrsMod += Utility.Random(70, 100) * (int)bw.ProtectionLevel; }

		if (props >= 3 && bw.ArmorAttributes.MageArmor > 0 || bw.Attributes.SpellChanneling > 0)
		    attrsMod = (int)((double)attrsMod * 1.3);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);

		AddQualityBonuses(bw.Quality, ref attrsMod, ref props);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseQuiver)
	    {
		BaseQuiver bw = ii as BaseQuiver;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
	    }
	    else if (ii is BaseInstrument)
	    {
		BaseInstrument bw = ii as BaseInstrument;

		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		if (PVBotSettings.PreAOSResourceBonus)
		    AddResourceBonuses(bw.Resource, ref attrsMod);
	    }
	    else if (ii is BaseRunicTool)
	    {
		BaseRunicTool bw = ii as BaseRunicTool;

		AddNormalizedBonuses(bw, IntensityMode.RunicToolProperties, ref attrsMod, ref props);
	    }
	    else if (ii is Spellbook)
	    {
		Spellbook bw = ii as Spellbook;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		if(bw.Slayer != SlayerName.None) ++props;
		if(bw.Slayer2 != SlayerName.None) ++props;

		if (bw.SpellCount > 0)
		{
		    attrsMod += bw.SpellCount * 20; // TODO: make the higher circle spells cost more
		}
		if (bw.Slayer != SlayerName.None)
		{
		    attrsMod += 100;
		}
		if (bw.Slayer2 != SlayerName.None)
		{
		    attrsMod += 100;
		}

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		if (attrsMod < 0) attrsMod = 0;
	    }
	    else if (ii is BaseTalisman)
	    {
		BaseTalisman bw = ii as BaseTalisman;

		AddNormalizedBonuses(bw, IntensityMode.AosAttribute, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.SAAbsorptionAttribute, ref attrsMod, ref props);

		AddNormalizedBonuses(bw, IntensityMode.SkillBonus, ref attrsMod, ref props);
		AddNormalizedBonuses(bw, IntensityMode.ResistanceBonus, ref attrsMod, ref props);

		if(bw.Slayer != TalismanSlayerName.None) ++props;

		if (bw.Slayer != TalismanSlayerName.None) 
		{
		    attrsMod += 100;
		    props++;
		}

		AddNegativeBonuses(bw.NegativeAttributes.Brittle, bw.NegativeAttributes.Prized,
			bw.NegativeAttributes.Massive, bw.NegativeAttributes.Unwieldly,
			bw.NegativeAttributes.Antique, bw.NegativeAttributes.NoRepair, ref attrsMod);

		if (attrsMod < 0) attrsMod = 0;
	    }

	    if (PVBotSettings.IncreasePriceBasedOnNumberOfProps)
	    {
		if (props == 1 || props == 2) { attrsMod *= PVBotSettings.AttrsMod1Or2Props; }
		else if (props == 3 || props == 4) { attrsMod *= PVBotSettings.AttrsMod3Or4Props; }
		else if (props == 5 || props == 6) { attrsMod *= PVBotSettings.AttrsMod5Or6Props; }
		else if (props == 7 || props == 8) { attrsMod *= PVBotSettings.AttrsMod7Or8Props; }
		else if (props >= 9) { attrsMod *= PVBotSettings.AttrsMod9OrMoreProps; }
	    }

	    return attrsMod;
	}

	private int PredictPrice( Item item )
	{
	    int price = 0;
	    List<SBInfo> sbList = new List<SBInfo>();
	    List<Item> items = new List<Item>();

	    SetupSBList(ref sbList);

	    items.Add(item);
	    if (item is Container)
	    {
		Container c = item as Container;
		foreach(Item it in c.Items)
		{
		    bool banned = it is Gold;

		    if (!banned)
			items.Add( it );
		}
	    }

	    int barterValue = Utility.Random(PVBotSettings.BarterValue);
	    int attrsMultiplier = Utility.RandomMinMax( PVBotSettings.MinAttrsMultiplier, PVBotSettings.MaxAttrsMultiplier );
	    bool isRichSucker = Utility.RandomMinMax( 0, 1000 ) > PVBotSettings.RichSuckerChance;
	    float modifier = (float)Utility.RandomMinMax( PVBotSettings.MinPriceModifier, PVBotSettings.MaxPriceModifier );
	    if (Utility.Random(100) > PVBotSettings.ImprovedPriceModChance && Utility.RandomBool())
		modifier *= Utility.RandomMinMax( PVBotSettings.MinImprovedPriceMod, PVBotSettings.MaxImprovedPriceMod ); // big luck, improved mod

	    foreach(Item ii in items)
	    { 
		if (PVBotSettings.HarderBagSale)
		{
		    // reroll for each item in the bag, makes it progressively harder to get a decent price
		    // SetupSBList(ref sbList); // enable to make things even harder for bag sales
		    barterValue = Utility.Random(PVBotSettings.BarterValue);
		    attrsMultiplier = Utility.RandomMinMax( PVBotSettings.MinAttrsMultiplier, PVBotSettings.MaxAttrsMultiplier );
		    modifier = (float)Utility.RandomMinMax( PVBotSettings.MinPriceModifier, PVBotSettings.MaxPriceModifier );
		    if (Utility.Random(100) > PVBotSettings.ImprovedPriceModChance && Utility.RandomBool())
			modifier *= Utility.RandomMinMax( PVBotSettings.MinImprovedPriceMod, PVBotSettings.MaxImprovedPriceMod );
		}

		int itemPrice = 0;
		foreach(SBInfo priceInfo in sbList)
		{
		    int estimate = priceInfo.SellInfo.GetSellPriceFor(ii/*, barterValue*/);
		    estimate = estimate * barterValue / 100;
		    if (itemPrice < estimate)
			itemPrice = estimate;
		}

		ScalePriceOnDurability(ii, ref price);

		price += itemPrice;

		if (price < PVBotSettings.PriceThresholdForAttributeCheck)
		{
		    int attrsMod = GetAttrsMod(ii);
		    attrsMod *= (int)((float)attrsMultiplier / 100);

		    ScalePriceOnDurability(ii, ref attrsMod);

		    price += attrsMod;
		}

		if (price == 1 && PVBotSettings.LowPriceBoost)
		{
		    price = Utility.RandomMinMax( 1, PVBotSettings.MinimalPriceMaxBoost );
		}
		else if (price > PVBotSettings.RichSuckerMinPrice && isRichSucker)
		{
		    price *= Utility.RandomMinMax( PVBotSettings.RichSuckerMinPriceMultiplier, PVBotSettings.RichSuckerMaxPriceMultiplier ); // rich sucker
		}

		price += (int)((float)price / modifier);
	    }

	    return price;
	}

	protected override void OnTick()
	{
	    if (!PVBotSettings.EnableSimulatedSales || m_Vendor.Backpack == null)
		return;

	    //Debug(); // uncomment to enable debug runs on every tick

	    int iterations = PVBotSettings.MaxIterationsPerCheck; //Math.Min(PVBotSettings.MaxIterationsPerCheck, m_Vendor.Backpack.Items.Count);

	    for (int i = 0; i < iterations; i++)
	    {
		List<Item> list = new List<Item>();
		string BuyerName = Utility.RandomBool() ? NameList.RandomName("male") : NameList.RandomName("female");

		foreach ( Item item in m_Vendor.Backpack.Items )
		{
		    VendorItem vi = m_Vendor.GetVendorItem( item );
		    bool banned = item is Gold;

		    if ( vi != null && (vi.IsForSale || vi.IsForFree) && vi.Price != 999 && !banned)
			list.Add( item );

		    if ( item is Container && vi.Price < 0 ) // containers not for sale
		    {
			foreach( Item ins in item.Items )
			{
			    if (!(ins is Container))
			    {
				VendorItem vii = m_Vendor.GetVendorItem( ins );
				bool insBanned = ins is Gold;
				if ( vii != null && (vii.IsForSale || vii.IsForFree) && vii.Price != 999 && !insBanned)
				    list.Add( ins );
			    }
			}
		    }
		}

		if ( list.Count > 0 )
		{
		    Item randItem = list[Utility.Random(list.Count)];
		    VendorItem viToSell = m_Vendor.GetVendorItem( randItem );

		    string Desc = viToSell.Description;
		    string Label = PVBotUtils.GetItemName(randItem);

		    int TotalPrice = viToSell.Price;

		    string Fmt = BuyerName + " ";
		    if (TotalPrice != 0)
			Fmt += "bought " + Label + " ";
		    else
			Fmt += "took " + Label + " ";
		    if (randItem.Amount > 1)
			Fmt += "(" + randItem.Amount + "x) ";
		    if (Desc != "" && !Desc.StartsWith("@"))
			Fmt += "[" + Desc + "] ";
		    if (TotalPrice != 0)
			Fmt += "for " + TotalPrice + " gold.";
		    else
			Fmt += "for free.";

		    // projected price
		    int ProjPrice = PredictPrice(randItem) * randItem.Amount;
		    if (ProjPrice > 1)
			ProjPrice = ProjPrice * PVBotSettings.FinalPriceModifier / 100;

		    //m_Vendor.SayTo( m_Vendor.Owner, $"Projected price: {ProjPrice} for {Label}.");

		    if (TotalPrice <= ProjPrice)
		    {
			if (!PVBotSettings.AnnounceSalesOnlyIfClose || m_Vendor.Owner.GetDistanceToSqrt( m_Vendor ) <= PVBotSettings.DistToAnnounceSales)
			    m_Vendor.SayTo( m_Vendor.Owner, Fmt );

			m_Vendor.HoldGold += TotalPrice;

			m_Vendor.RecentSales.Add(Fmt);
			if (m_Vendor.RecentSales.Count > PVBotSettings.NumRecentSalesToRemember)
			{
			    m_Vendor.RecentSales.RemoveAt(0);
			}

			if (randItem is Container)
			{
			    Container ct = randItem as Container;
			    for (int d = ct.Items.Count - 1; d >= 0; d-- )
			    {
				ct.Items[d].Delete();
			    }
			}
			randItem.Delete();
		    }
		}
	    }

	    // Set the new interval here
	    this.Interval = GetRandomBuyInterval();
	    // Console.WriteLine(m_Vendor.Name + " - next purchase in " + this.Interval);
	}

	private void Debug()
	{
	    const int NUM_ITERATIONS = 100;
	    List<Item> list = new List<Item>();

	    Console.WriteLine("Predicting prices over " + NUM_ITERATIONS + " iterations...");

	    foreach ( Item item in m_Vendor.Backpack.Items )
	    {
		VendorItem vi = m_Vendor.GetVendorItem( item );
		bool banned = item is Gold;

		if ( vi != null && (vi.IsForSale || vi.IsForFree) && vi.Price != 999 && !banned)
		    list.Add( item );

		if ( item is Container && vi.Price < 0 ) // containers not for sale
		{
		    foreach( Item ins in item.Items )
		    {
			if (!(ins is Container))
			{
			    VendorItem vii = m_Vendor.GetVendorItem( ins );
			    bool insBanned = ins is Gold;
			    if ( vii != null && (vii.IsForSale || vii.IsForFree) && vii.Price != 999 && !insBanned)
				list.Add( ins );
			}
		    }
		}
	    }

	    if ( list.Count > 0 )
	    {
		foreach( Item item in list)
		{
		    VendorItem viToSell = m_Vendor.GetVendorItem( item );
		    string Label = PVBotUtils.GetItemName(item);

		    int min = int.MaxValue;
		    int max = 1;
		    int avg = 0;

		    for (int i = 0; i < NUM_ITERATIONS; i++)
		    {
			int ProjPrice = PredictPrice(item) * item.Amount;
			if (ProjPrice > 1)
			    ProjPrice = ProjPrice * PVBotSettings.FinalPriceModifier / 100;
			if (ProjPrice > 1 && ProjPrice < min)
			    min = ProjPrice;
			if (ProjPrice > max)
			    max = ProjPrice;
		    }

		    if (min == int.MaxValue) { min = 1; }
		    avg = (min + max) / 2;

		    Console.WriteLine("Prices for " + Label + ": min = " + min + ", max = " + max + ", avg = " + avg);
		}
	    }
	}
    }
}
