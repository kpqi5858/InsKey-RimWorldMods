using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Prioritize2.Patch
{
    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global")]
    public class Patch_GenClosest1
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {

        }
    }

    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global_Reachable")]
    public class Patch_GenClosest2
    {
        public static void Prefix(ref TraverseParms traverseParams, ref Func<Thing, float> priorityGetter)
        {

        }
    }


    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Regionwise_ReachablePrioritized")]
    public class Patch_GenClosest3
    {
        public static void Prefix(ref TraverseParms traverseParams, ref Func<Thing, float> priorityGetter)
        {

        }
    }
}
