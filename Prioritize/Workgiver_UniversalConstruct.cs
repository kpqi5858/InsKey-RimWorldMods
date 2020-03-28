using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Prioritize
{
    public class Workgiver_UniversalConstruct : WorkGiver_ConstructDeliverResources
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Construction);

        private static List<WorkGiver_Scanner> CheckList = new List<WorkGiver_Scanner>();

        public Workgiver_UniversalConstruct()
        {
            //ConstructFinishFrames
            //ConstructDeliverResourcesToFrames
            //ConstructDeliverResourcesToBlueprints
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructFinishFrames").Worker as WorkGiver_Scanner);
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructDeliverResourcesToFrames").Worker as WorkGiver_Scanner);
            CheckList.Add(DefDatabase<WorkGiverDef>.GetNamed("ConstructDeliverResourcesToBlueprints").Worker as WorkGiver_Scanner);
        }

        private Job NoCostFrameMakeJobFor(Pawn pawn, IConstructible c)
        {
            if (c is Blueprint_Install)
            {
                return null;
            }
            if (c is Blueprint && c.MaterialsNeeded().Count == 0)
            {
                return new Job(JobDefOf.PlaceNoCostFrame)
                {
                    targetA = (Thing)c
                };
            }
            return null;
        }


        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            foreach (var wgiver in CheckList)
            {
                var res = wgiver.JobOnThing(pawn, t, forced);
                if (res != null) return res;
            }
            return null;
        }
    }
}
