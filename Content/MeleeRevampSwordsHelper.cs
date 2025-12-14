using System.Collections.Generic;
using Terraria.ID;

namespace MeleeRevamp.Content
{
    public class MeleeRevampSwordsHelper
    {
        public static bool[] Broadswords = ItemID.Sets.Factory.CreateBoolSet(
            ItemID.AshWoodSword, ItemID.BorealWoodSword, ItemID.CactusSword, ItemID.EbonwoodSword, ItemID.PalmWoodSword, ItemID.PearlwoodSword, ItemID.RichMahoganySword, ItemID.ShadewoodSword, ItemID.WoodenSword, ItemID.ZombieArm, ItemID.BluePhaseblade, ItemID.BreathingReed, ItemID.CandyCaneSword, ItemID.GreenPhaseblade, ItemID.IceBlade, ItemID.Katana, ItemID.OrangePhaseblade, ItemID.PurpleClubberfish, ItemID.PurplePhaseblade, ItemID.RedPhaseblade, ItemID.WhitePhaseblade, ItemID.YellowPhaseblade, ItemID.BatBat, ItemID.DyeTradersScimitar, ItemID.Flymeal, ItemID.AntlionClaw, ItemID.Muramasa,
            ItemID.TentacleSpike, ItemID.BeeKeeper, ItemID.BoneSword, ItemID.NightsEdge, ItemID.FieryGreatsword, ItemID.AdamantiteSword, ItemID.BeamSword, ItemID.BluePhasesaber, ItemID.BreakerBlade, ItemID.CobaltSword, ItemID.Cutlass, ItemID.FalconBlade, ItemID.GreenPhasesaber, ItemID.HamBat, ItemID.MythrilSword, ItemID.OrangePhasesaber, ItemID.OrichalcumSword, ItemID.PalladiumSword, ItemID.PurplePhasesaber, ItemID.RedPhasesaber, ItemID.SlapHand, ItemID.TitaniumSword, ItemID.WhitePhasesaber, ItemID.YellowPhasesaber, ItemID.Bladetongue, ItemID.DD2SquireDemonSword, ItemID.Excalibur, ItemID.Frostbrand,
            ItemID.IceSickle, ItemID.Seedler, ItemID.DeathSickle, ItemID.ChlorophyteClaymore, ItemID.ChlorophyteSaber, ItemID.ChristmasTreeSword, ItemID.DD2SquireBetsySword, ItemID.InfluxWaver, ItemID.Keybrand, ItemID.PsychoKnife, ItemID.TheHorsemansBlade, ItemID.TrueExcalibur, ItemID.Meowmere, ItemID.StarWrath
            //ItemID.TrueNightsEdge, ItemID.TerraBlade
            );
        public static bool[] RevampedBroadswords = ItemID.Sets.Factory.CreateBoolSet(
            ItemID.CopperBroadsword, ItemID.TinBroadsword, ItemID.IronBroadsword, ItemID.LeadBroadsword, ItemID.SilverBroadsword, ItemID.TungstenBroadsword, ItemID.GoldBroadsword, ItemID.PlatinumBroadsword, ItemID.LightsBane, ItemID.BloodButcherer, ItemID.BladeofGrass, ItemID.EnchantedSword);
        
        public static float SwordGaugeDefaultMax = 1.2f;
        public static Dictionary<int, float> SwordGaugeMaxValues = new Dictionary<int, float>()
        {
            { ItemID.CopperBroadsword, 1.2f },
            { ItemID.TinBroadsword, 1.2f },
            { ItemID.IronBroadsword, 1.2f },
            { ItemID.LeadBroadsword, 1.2f },
            { ItemID.SilverBroadsword, 1.2f },
            { ItemID.TungstenBroadsword, 1.2f },
            { ItemID.GoldBroadsword, 1.2f },
            { ItemID.PlatinumBroadsword, 1.2f },
            { ItemID.LightsBane, 1.6f },
            { ItemID.BloodButcherer, 2.4f },
            { ItemID.BladeofGrass, 2.4f },
            { ItemID.EnchantedSword, 2.4f }
        };
    }
}