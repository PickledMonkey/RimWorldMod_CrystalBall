using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrystalBall;
using RimWorld;
using Verse;

namespace Crystalball
{
    public class Building_CrystalBallTable : Building
    {
        private float scryWorkTickAmount = (1.0f / 50.0f); // 1/50 = 2 hours at full speed because we need to hit a work amount of 100
        private float scryWorkAmount = 100.0f;
        private float progress = 0.0f;
        private bool recharged = false;
        private int rechargedTick = 0;

        private float accumulatedScryAbility = 0.0f;
        private float accumulatedPredictionCount = 0.0f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.scryWorkTickAmount, "scryWorkTickAmount", (1.0f / 50.0f), false);
            Scribe_Values.Look<float>(ref this.scryWorkAmount, "scryWorkAmount", 100.0f, false);
            Scribe_Values.Look<float>(ref this.progress, "progress", 0.0f, false);
            Scribe_Values.Look<bool>(ref this.recharged, "recharged", false, false);
            Scribe_Values.Look<int>(ref this.rechargedTick, "rechargedTick", 0, false);
            Scribe_Values.Look<float>(ref this.accumulatedScryAbility, "accumulatedScryAbility", 0.0f, false);
            Scribe_Values.Look<float>(ref this.accumulatedPredictionCount, "accumulatedPredictionCount", 0.0f, false);
        }

        public float GetCurrentProgress()
        {
            return (progress / scryWorkAmount);
        }

        public void PerformScryWork(float scryingAbility, float predictionCount)
        {
            CrystalBallSettings settings = CrystalBallStatic.currMod.GetSettings<CrystalBallSettings>();

            if (recharged)
            {
                if(scryingAbility > 0.0f)
                {
                    float progressTick = scryWorkTickAmount * scryingAbility * settings.scrySpeedFactor;
                    progress += progressTick;

                    float tickFactor = progressTick / scryWorkAmount;

                    accumulatedScryAbility += scryingAbility * tickFactor;
                    accumulatedPredictionCount += predictionCount * tickFactor;
                }
            }


            if (recharged && (progress >= scryWorkAmount) )
            {
                int currTime = Find.TickManager.TicksGame;
                rechargedTick = currTime + settings.crystalBallRechargeTime;
                recharged = false;
                progress = 0.0f;

#if DEBUG
                Log.Message(String.Format("Scry complete ability={0}, num={1}", scryingAbility, predictionCount));
#endif
                //perform predictions
                WarnedIncidentQueueWorldComponent warnedIncidentQueue = Find.World.GetComponent<WarnedIncidentQueueWorldComponent>();
                warnedIncidentQueue.PredictEvents(accumulatedScryAbility, (int)accumulatedPredictionCount);

                accumulatedScryAbility = 0.0f;
                accumulatedPredictionCount = 0.0f;
            }
        }

        public bool isReadyForScrying()
        {
            return recharged;
        }

        public override void TickRare()
        {
            base.TickRare(); //Make sure any components are ticked if needed.

            //Log.Message(String.Format("Crystal Ball Recharged = {0}, CurrTick= {1}, RechargeTick={2}, Progress={3}", recharged.ToString(), Find.TickManager.TicksGame, rechargedTick, progress.ToString()));

            if (!recharged)
            {
                int currTime = Find.TickManager.TicksGame;
                if (currTime >= rechargedTick)
                {
                    recharged = true;
                }
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (!recharged)
            {
                int ticksLeft = rechargedTick - Find.TickManager.TicksGame;
                stringBuilder.AppendInNewLine(String.Format("Crystal ball is recharging psychic energy: {0} until complete.", ticksLeft.ToStringTicksToPeriod(true, true, false, false).ToString()));
            }
            else if (progress < 1.0f)
            {
                stringBuilder.AppendInNewLine(String.Format("Crystal ball is recharged and ready for use."));
            }
            else
            {
                stringBuilder.AppendInNewLine(String.Format("Progress scrying into the future: {0}%", (int)progress));
            }
            return stringBuilder.ToString();
        }


        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            base.GetFloatMenuOptions(myPawn);
            if(myPawn.RaceProps.Humanlike)
            {
                float scryAbility = myPawn.GetStatValue(ModDefs.StatDef_Scry, true);

                if(scryAbility < 0.1f) //Make sure this number matches the work giver check
                {
                    yield return new FloatMenuOption("Cannot use. Not enough intellectual and psychic sensitivity.", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                else if(!recharged)
                {
                    yield return new FloatMenuOption("Cannot use. Still recharging.", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
            }
            yield break;
        }

    }






}
