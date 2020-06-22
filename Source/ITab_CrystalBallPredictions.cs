using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using RimWorld;
using CrystalBall;

namespace Crystalball
{
    class ITab_CrystalBallPredictions : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private const float TopPadding = 20f;

        public static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        public static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private const float ThingIconSize = 28f;

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;


        public ITab_CrystalBallPredictions()
        {
            size = new Vector2(460f, 450f);
            labelKey = "TabPredictions";
            tutorTag = "Predictions";
        }

        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f).ContractedBy(10f);
            Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.BeginGroup(position);

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

            float num = 0.0f;

            DrawKnownIncidentList(ref num, viewRect.width);

            if (Event.current.type == EventType.Layout)
            {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawKnownIncidentList(ref float y, float width)
        {
            WarnedIncidentQueueWorldComponent warnedIncidentQueue = Find.World.GetComponent<WarnedIncidentQueueWorldComponent>();

            int currTick = Find.TickManager.TicksGame;

            foreach (QueuedIncident qi in warnedIncidentQueue)
            {
                int ticksLeft = (qi.FireTick - currTick);
                string timeStr;

                if (ticksLeft < 60000)
                {
                    timeStr = ticksLeft.ToStringTicksToPeriodVague();
                }
                else
                {
                    timeStr = ticksLeft.ToStringTicksToPeriod(false, false, false, true);
                }

                string incidentLabelStr = qi.FiringIncident.def.label;
                string outputString = String.Format("Expecting {0} in {1}", incidentLabelStr, timeStr);

                Widgets.LongLabel(0.0f, width, outputString, ref y);
            }
        }
    }


}