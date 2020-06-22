using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
namespace Crystalball
{
    public class Crystalball
    {
    }

    [StaticConstructorOnStartup]
    public static class CrystalBallStatic
    {
        public static CrystalBallMod currMod = null;

        static CrystalBallStatic() //constructor is called before anything is loaded in
        {
#if DEBUG
            Log.Message("CrystalBallStatic Initialized");
#endif
        }
    }

    public class CrystalBallSettings : ModSettings
    {

        public bool exampleBool;
        public int medianDelayTime = 300000; //5 Days
        public int delayTimeFudgeWindow = 240000; //4 Days
        public int crystalBallRechargeTime = 50000; //20 hours
        public float scrySpeedFactor = 1.0f;

        public string medianDelayEntryBuffer = "300000";
        public string fudgeWindowEntryBuffer = "240000";
        public string rechargeTimeEntryBuffer = "50000";
        public string scrySpeedEntryBuffer = "1.0";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref medianDelayTime, "medianDelayTime");
            Scribe_Values.Look(ref delayTimeFudgeWindow, "delayTimeFudgeWindow");
            Scribe_Values.Look(ref crystalBallRechargeTime, "crystalBallRechargeTime");
            Scribe_Values.Look(ref scrySpeedFactor, "scrySpeedFactor");

            Scribe_Values.Look(ref medianDelayEntryBuffer, "medianDelayEntryBuffer");
            Scribe_Values.Look(ref fudgeWindowEntryBuffer, "fudgeWindowEntryBuffer");
            Scribe_Values.Look(ref rechargeTimeEntryBuffer, "rechargeTimeEntryBuffer");
            Scribe_Values.Look(ref scrySpeedEntryBuffer, "scrySpeedEntryBuffer");

            base.ExposeData();
        }
    }

    public class CrystalBallMod : Mod
    {
        CrystalBallSettings settings;

        public CrystalBallMod(ModContentPack content) : base(content)
        {
            CrystalBallStatic.currMod = this;

            this.settings = GetSettings<CrystalBallSettings>();
#if DEBUG
            Log.Message("CrystalBallMode Initialized");
#endif
        }

        

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Median Delay Time");
            listingStandard.IntEntry(ref settings.medianDelayTime, ref settings.medianDelayEntryBuffer);

            listingStandard.Label("Delay Time Fudge Window");
            listingStandard.IntEntry(ref settings.delayTimeFudgeWindow, ref settings.fudgeWindowEntryBuffer);

            listingStandard.Label("Time For Crystal Balls to Recharge");
            listingStandard.IntEntry(ref settings.crystalBallRechargeTime, ref settings.rechargeTimeEntryBuffer);

            listingStandard.Label("Multipler for the speed to scry a crystal ball");
            listingStandard.TextFieldNumeric(ref settings.scrySpeedFactor, ref settings.scrySpeedEntryBuffer, 0.0f, 10.0f);

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "CrystalBall";
        }
    }


    [DefOf]
    public static class ModDefs
    {
        static ModDefs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ModDefs));
        }


        public static EffecterDef EffecterDef_Scry;
        public static JobDef JobDef_Scry;
        public static StatDef StatDef_Scry;
        public static StatDef StatDef_PredictionCount;
        public static ThingDef ThingDef_CrystalBallTable;
        public static WorkGiverDef WorkGiverDef_ScryCrystallBall;
        public static WorkTypeDef WorkTypeDef_Scry;
        

        //public static StatDef ScryAbility = DefDatabase<StatDef>.GetNamed("Stat_ScryAbility");
        //public static EffecterDef ScryEffect = DefDatabase<EffecterDef>.GetNamed("Effect_ScryEffect");
        //public static ThingDef CrystalBallTable = DefDatabase<ThingDef>.GetNamed("Building_CrystalBallTable");
    }


}