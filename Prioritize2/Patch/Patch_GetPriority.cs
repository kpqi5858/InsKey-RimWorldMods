using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Prioritize2.Patch
{
    //In vanila, higher priority value means higher priority

    //"Safe" patch
    [HarmonyPatch(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class Patch_GetPriority
    {
        public static void Postfix(Pawn pawn, TargetInfo t, ref float __result, WorkGiver_Scanner __instance)
        {
            if (!__instance.Prioritized || !pawn.CanAffectedByPriority()) return;

            Map map = pawn.Map;

            if (map == null)
            {
                map = t.Map;
            }

            float modPriority = MainMod.Data.GetPriorityOnCell(map, t.Cell);

            if (t.HasThing && !MainMod.ModConfig.patchGenClosest && PriorityData.CanPrioritize(t.Thing))
            {
                modPriority += MainMod.Data.GetPriority(t.Thing);
            }

            modPriority *= MainMod.ModConfig.priorityMultiplier;

            __result += modPriority;
        }
    }
}
