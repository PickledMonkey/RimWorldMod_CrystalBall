using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using Verse.AI;

namespace Crystalball
{
    class JobDriver_ScryCrystalBall : JobDriver
    {
        private Building_CrystalBallTable crystalBallTable
        {
            get
            {
                return (Building_CrystalBallTable)base.TargetThingA;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            float scryAbility = this.pawn.GetStatValue(ModDefs.StatDef_Scry, true);
            if(scryAbility > 0)
            {
                return this.pawn.Reserve(this.crystalBallTable, this.job, 1, -1, null, errorOnFailed);
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil scry = new Toil();
            scry.tickAction = delegate ()
            {
                Pawn actor = scry.actor;
                float scryAbility = actor.GetStatValue(ModDefs.StatDef_Scry, true);
                float predictionCount = actor.GetStatValue(ModDefs.StatDef_PredictionCount, true);

                Building_CrystalBallTable crystalBall = this.crystalBallTable;
                crystalBall.PerformScryWork(scryAbility, predictionCount);

                actor.skills.Learn(SkillDefOf.Intellectual, 0.01f, false);
                actor.GainComfortFromCellIfPossible(true);
            };
            scry.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            scry.WithEffect(ModDefs.EffecterDef_Scry, TargetIndex.A);
            scry.WithProgressBar(TargetIndex.A, delegate
            {
                Building_CrystalBallTable crystalBall = this.crystalBallTable;
                if (crystalBall == null)
                {
                    return 0f;
                }
                return crystalBall.GetCurrentProgress();
            }, false, -0.5f);
            scry.defaultCompleteMode = ToilCompleteMode.Delay;
            scry.defaultDuration = 500;
            scry.activeSkill = (() => SkillDefOf.Intellectual);
            yield return scry;
            yield return Toils_General.Wait(2, TargetIndex.None);
            yield break;
        }


    }
}
