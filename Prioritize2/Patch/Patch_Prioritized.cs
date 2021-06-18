using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Prioritize2.Patch
{
    [HarmonyPatch(typeof(WorkGiver_Scanner))]
    [HarmonyPatch("Prioritized", MethodType.Getter)]
    public class Patch_Prioritized
    {
        public static bool Prefix(WorkGiver_Scanner __instance, ref bool __result)
        {
            bool isPatchAllowed = MainMod.ModConfig.IsPatchAllowed(__instance.GetType());

            __result = isPatchAllowed;

            return !isPatchAllowed;
        }
    }
}
