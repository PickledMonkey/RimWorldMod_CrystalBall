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
    class WorkGiver_CrystalBall : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ModDefs.ThingDef_CrystalBallTable);
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            float scryAbility = pawn.GetStatValue(ModDefs.StatDef_Scry, true);

            if(!pawn.RaceProps.Humanlike)
            {
                return false;
            }

            if (scryAbility < 0.1)
            {
                return false;
            }

            if (pawn.CanReserve(t, 1, -1, null, forced))
            {
                Building_CrystalBallTable crystalBall = t as Building_CrystalBallTable;
                return crystalBall.isReadyForScrying();
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(ModDefs.JobDef_Scry, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            return t.Thing.GetStatValue(ModDefs.StatDef_Scry, true);
        }
    }


}
