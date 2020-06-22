using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace CrystalBall
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.crystallball.main");

#if DEBUG
            Log.Message("Loading Harmony Patches For CrystalBall");
            harmony.Patch(
                original: AccessTools.Method(type: typeof(IncidentWorker), name: nameof(IncidentWorker.CanFireNow)),
                prefix: new HarmonyMethod(methodType: patchType, methodName: nameof(CanFireNowPrefix)),
                postfix: new HarmonyMethod(methodType: patchType, methodName: nameof(CanFireNowPostfix))
                );

            harmony.Patch(
                original: AccessTools.Method(type: typeof(IncidentWorker), name: nameof(IncidentWorker.TryExecute)),
                prefix: new HarmonyMethod(methodType: patchType, methodName: nameof(TryExecutePrefix)),
                postfix: new HarmonyMethod(methodType: patchType, methodName: nameof(TryExecutePostfix))
            );
#endif
            harmony.Patch(
                original: AccessTools.Method(type: typeof(Storyteller), name: nameof(Storyteller.TryFire)),
                prefix: new HarmonyMethod(methodType: patchType, methodName: nameof(TryFirePrefix)),
                postfix: new HarmonyMethod(methodType: patchType, methodName: nameof(TryFirePostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(type: typeof(Storyteller), name: nameof(Storyteller.StorytellerTick)),
                prefix: new HarmonyMethod(methodType: patchType, methodName: nameof(StoryTellerTickPrefix)),
                postfix: new HarmonyMethod(methodType: patchType, methodName: nameof(StoryTellerTickPostfix))
            );

            //harmony.PatchAll();
        }

#if DEBUG
        public static bool CanFireNowPrefix(IncidentParms parms, bool forced, ref bool __result, IncidentWorker __instance)
        {
            string debugStr = String.Format("CanFireNowPrefix: Type={0}", __instance.GetType());
            Log.Message(debugStr);

            return true;
        }

        public static bool CanFireNowPostfix(bool __result, IncidentParms parms, bool forced, ref  IncidentWorker __instance)
        {
            string debugStr = String.Format("CanFireNowPostfix: Type={0}, Result={1}", __instance.GetType(), __result.ToString());
            Log.Message(debugStr);

            return __result;
        }

        public static bool TryExecutePrefix(IncidentParms parms, ref bool __result, IncidentWorker __instance)
        {

            string debugStr = String.Format("TryExecutePrefix: Type={0}, NullParms={1}", __instance.GetType().ToString(), (parms == null).ToString());
            Log.Message(debugStr);

            return true;
        }

        public static bool TryExecutePostfix(bool __result, IncidentParms parms, IncidentWorker __instance)
        {
            string debugStr = String.Format("TryExecutePostfix: Type={0}, Result={1}", __instance.GetType(), __result.ToString());
            Log.Message(debugStr);

            return __result;
        }
#endif

        public static bool TryFirePrefix(FiringIncident fi, ref bool __result, Storyteller __instance)
        {
            WarnedIncidentQueueWorldComponent warnedIncidentQueue = Find.World.GetComponent<WarnedIncidentQueueWorldComponent>();

#if DEBUG
            string debugStr = String.Format("TryFirePrefix: Type={0}, WarnedQueueIsTicking={1}", fi.def.workerClass, warnedIncidentQueue.IsFiringEvents().ToString());
            Log.Message(debugStr);
#endif
            

            bool continueExecution = true;

            if(fi.parms == null)
            {
#if DEBUG
                Log.Message("Parameters are null for some reason");
#endif
            }

            if(warnedIncidentQueue.IsFiringEvents())
            {
#if DEBUG
                Log.Message("Firing from WarnedIncidents");
#endif

                if (fi.def.Worker.CanFireNow(fi.parms, true) == true)
                {
                    if(fi.def.Worker.TryExecute(fi.parms) == true)
                    {
#if DEBUG
                        Log.Message("Fired");
#endif
                        __result = true;
                    }
                }
                else
                {
#if DEBUG
                    Log.Message("Not Fired");
#endif
                    __result = false;
                }
                continueExecution = false;
            }
            else if (fi.def.Worker.CanFireNow(fi.parms, false) == true)
            {
#if DEBUG
                Log.Message("We can fire the event now");
#endif

                bool incidentQueued = warnedIncidentQueue.AddIncidentToQueue(fi);
                if (incidentQueued)
                {
#if DEBUG
                    Log.Message("Game attempted to fire event. Captured and queued.");
#endif

                    fi.parms.target.StoryState.Notify_IncidentFired(fi); //Notify the storyteller that the incident fired, so it will carry on planning the next disasters
                    __result = true;
                    continueExecution = false;
                }
                else
                {
#if DEBUG
                    Log.Message("Incident not queued. Firing Immidiately.");
#endif
                }
            }
            else //fi.def.Worker.CanFireNow(fi.parms, false) == false
            {
                //if we can't fire this now.... why are we trying to fire it at all...
                __result = false;
                continueExecution = false;

#if DEBUG
                Log.Message("Skipping the rest of execution because the event can't be fired right now.");
#endif
            }

#if DEBUG
            Log.Message(String.Format("Continuing Execution = {0}", continueExecution.ToString()));
#endif

            return continueExecution;
        }

        public static bool TryFirePostfix(bool __result, FiringIncident fi, Storyteller __instance)
        {
#if DEBUG
            string debugStr = String.Format("TryFirePostfix: Type={0}, Result={1}", fi.def.workerClass, __result.ToString());
            Log.Message(debugStr);
#endif
            return __result;
        }

        public static bool StoryTellerTickPrefix(Storyteller __instance)
        {
            WarnedIncidentQueueWorldComponent warnedIncidentQueue = Find.World.GetComponent<WarnedIncidentQueueWorldComponent>();
            warnedIncidentQueue.WarnedIncidentQueueTick();

            return true;
        }

        public static void StoryTellerTickPostfix(Storyteller __instance)
        {
            // Do Nothing for now. Just a placeholder.
        }
    }

}
