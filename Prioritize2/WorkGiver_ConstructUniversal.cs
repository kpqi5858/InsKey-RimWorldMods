using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace Prioritize2
{
    //It's very weird way for substituting this as original WorkGivers
    //But idk
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

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
        {
            if (originalDef == null)
            {
                originalDef = def;
            }

            foreach (var wgiver in CheckList)
            {
                var res = wgiver.JobOnThing(pawn, t, forced);

                if (res != null)
                {
                    def = wgiver.def;
                    return res;
                }
            }

            def = originalDef;
            return null;
        }
    }
}
