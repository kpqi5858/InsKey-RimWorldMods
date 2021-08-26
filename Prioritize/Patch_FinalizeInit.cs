using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Prioritize
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public class Patch_FinalizeInit
    {
        public static void Prefix()
        {
            if (MainMod.save == null)
            {
                Log.Message("FinalizeInit called but no Prioritize mod save loaded, Probably new game start, or bug. (Should be harmless message)");
                return;
            }
            MainMod.save.ClearUnusedThingPriority();
        }
    }


    [HarmonyPatch(typeof(Thing), "Destroy")]
    public class Patch_ThingDestroy
    {
        public static void Prefix(Thing __instance)
        {
            if (MainMod.save == null) return;
            MainMod.DestroyedThingId.Add(__instance.thingIDNumber);
            if (MainMod.DestroyedThingId.Count > 50000)
            {
                Log.Warning("Too many items in DestroyedThingId.");
                MainMod.RemoveThingPriorityNow();
            }
        }
    }
}
