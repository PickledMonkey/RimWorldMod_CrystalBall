using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Crystalball;
using RimWorld.Planet;
using System.Collections;

namespace CrystalBall
{

    
    public class WarnedIncidentQueueWorldComponent : WorldComponent
    {
        private const float predictionDecayFactor = 0.8f;

        private IncidentQueue warnedIncidents = new IncidentQueue();
        private List<QueuedIncident> knownIncidents = new List<QueuedIncident>();

        private bool isFiringEvents = false;

        private Dictionary<string, int> specialIncidents = new Dictionary<string, int>();

        public static bool warningsActivated = false;



        public WarnedIncidentQueueWorldComponent(World world) : base(world)
        {
            specialIncidents.Add("IncidentWorker_CaravanDemand", 0);
            specialIncidents.Add("IncidentWorker_CaravanMeeting", 0);
            specialIncidents.Add("IncidentWorker_CaravanArrivalTributeCollector", 0);
            specialIncidents.Add("IncidentWorker_GiveQuest", 0);
            specialIncidents.Add("IncidentWorker_Ambush", 0);
            specialIncidents.Add("IncidentWorker_Ambush_EnemyFaction", 0);
            specialIncidents.Add("IncidentWorker_Ambush_ManhunterPack", 0);
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look<IncidentQueue>(ref this.warnedIncidents, "warnedIncidents", Array.Empty<object>());
            Scribe_Collections.Look<QueuedIncident>(ref this.knownIncidents, "knownIncidents", LookMode.Deep, Array.Empty<object>());
            Scribe_Values.Look<bool>(ref warningsActivated, "warningsActivated", false, false);
        }

        private bool AddKnownIncident(QueuedIncident qi)
        {
            bool incidentAdded = false;

            Predicate<QueuedIncident> findIncident = (QueuedIncident a) =>
            {
                return (a.FireTick == qi.FireTick) && (a.FiringIncident.def.shortHash == qi.FiringIncident.def.shortHash);
            };

            if(knownIncidents.Find(findIncident) == null)
            {
                knownIncidents.Add(qi);
                knownIncidents.Sort((QueuedIncident a, QueuedIncident b) => a.FireTick.CompareTo(b.FireTick));
                incidentAdded = true;
            }

            return incidentAdded;
        }

        private void RemoveCompletedIncidentsFromKnown()
        {
            int startSize = knownIncidents.Count;

            int currTick = Find.TickManager.TicksGame;
            knownIncidents.RemoveAll((QueuedIncident a) => a.FireTick < currTick);

            if(startSize > knownIncidents.Count)
            {
                knownIncidents.Sort((QueuedIncident a, QueuedIncident b) => a.FireTick.CompareTo(b.FireTick));
            }
        }

        public bool AddIncidentToQueue(FiringIncident fi)
        {
            bool incidentWasQueued = false;

            CrystalBallSettings settings = CrystalBallStatic.currMod.GetSettings<CrystalBallSettings>();

            int tickDelay = Verse.Rand.RangeInclusive(settings.medianDelayTime - settings.delayTimeFudgeWindow, settings.medianDelayTime + settings.delayTimeFudgeWindow);

            if((tickDelay < 0) || (!warningsActivated))
            {
                tickDelay = 0;
            }

#if DEBUG
            Log.Message(String.Format("Adding incident TickDelay={0}", tickDelay));
#endif

            if (specialIncidents.ContainsKey(fi.def.workerClass.ToString()))
            {
                specialIncidents.TryGetValue(fi.def.workerClass.ToString(), tickDelay);
            }

            if(tickDelay > 0)
            {
                int currTick = Find.TickManager.TicksGame;
                int fireTick = currTick + tickDelay;
                int retryDuration = 5000;

                warnedIncidents.Add(fi.def, fireTick, fi.parms, retryDuration);

                incidentWasQueued = true;
            }

            return incidentWasQueued;
        }

        public bool IsFiringEvents()
        {
            return isFiringEvents;
        }

        public void WarnedIncidentQueueTick()
        {
            isFiringEvents = true;
            warnedIncidents.IncidentQueueTick();
            RemoveCompletedIncidentsFromKnown();
            isFiringEvents = false;
        }

        public int GetEventsInQueue(out List<QueuedIncident> events)
        {
            events = new List<QueuedIncident>();

            int count = 0;
            foreach (QueuedIncident qi in warnedIncidents)
            {
                events.Add(qi);
                count++;
            }

            return count;
        }

        public void PredictEvents(float predictionStrength, int maxNumPredictions)
        {
#if DEBUG
            Log.Message(String.Format("Predicting with strength={0}, num={1}", predictionStrength, maxNumPredictions));
#endif


            List<QueuedIncident> incidentList;
            GetEventsInQueue(out incidentList);
            incidentList.Shuffle();
            
            int count = 0;
            foreach (QueuedIncident qi in incidentList)
            {
                bool predictionSuccess = Verse.Rand.Chance(predictionStrength);
                
                if(predictionSuccess)
                {
                    Log.Message(String.Format("Prediction Success strength={0}", predictionStrength));

                    if (AddKnownIncident(qi))
                    {
                        Log.Message(String.Format("Added Incident {0}", qi.FiringIncident.def.defName));

                        predictionStrength *= predictionDecayFactor;
                        count++;
                    }

                    if (count >= maxNumPredictions)
                    {
                        return;
                    }
                }
            }

            
        }

        public IEnumerator GetEnumerator()
        {
            foreach (QueuedIncident qi in knownIncidents)
            {
                yield return qi;
            }
            yield break;
        }
    }
}
