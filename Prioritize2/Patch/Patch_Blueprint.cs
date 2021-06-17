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
    [HarmonyPatch(typeof(Blueprint), "TryReplaceWithSolidThing")]
    public class Patch_Blueprint
    {
        public static void Postfix(Blueprint __instance, Thing createdThing)
        {
            int priority = MainMod.Data.GetPriority(__instance);

            MainMod.Data.SetPriority(createdThing, priority);
        }
    }
}
