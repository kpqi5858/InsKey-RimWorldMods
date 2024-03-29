﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Prioritize2
{
    public class WorkGiver_ConstructUniversal : WorkGiver_ConstructDeliverResources
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Construction);

        private static List<WorkGiver_Scanner> CheckList = new List<WorkGiver_Scanner>();

        private WorkGiverDef originalDef = null;

        public WorkGiver_ConstructUniversal()
        {
            //ConstructFinishFrames
            //ConstructDeliverResourcesToFrames
            //ConstructDeliverResourcesToBlueprints
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructFinishFrames").Worker as WorkGiver_Scanner);
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructDeliverResourcesToFrames").Worker as WorkGiver_Scanner);
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructDeliverResourcesToBlueprints").Worker as WorkGiver_Scanner);
        }

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        //It's very weird way of substituting this as original WorkGivers..But idk
        //Much better than old Prioritize. Because it handles "def" while old one doesn't
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
        {
            if (!MainMod.ModConfig.universalConstructWorkgiver) return null;

            if (originalDef == null)
            {
                originalDef = def;
            }

            WorkGiverDef failDef = null;

            foreach (var wgiver in CheckList)
            {
                var res = wgiver.JobOnThing(pawn, t, forced);

                if (res != null)
                {
                    def = wgiver.def;
                    return res;
                }
                else
                {
                    failDef = wgiver.def;
                }
            }

            def = failDef;

            return null;
        }
    }
}
