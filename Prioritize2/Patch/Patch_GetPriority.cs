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
            if (MainMod.ModConfig.patchGenClosest) return;

            //__instance.Prioritized may return true even if it's in no patch list because we give control back in Patch_Prioritized
            if (!MainMod.ModConfig.IsPatchAllowed(__instance.GetType()) || !pawn.CanAffectedByPriority()) return;
            
            Map map = pawn.Map ?? t.Map;
            //Is this possible to have both null map for pwan and target??
            if (map == null)
            {
                return;
            }

            float modPriority = MainMod.Data.GetPriorityOnCell(map, t.Cell);

            if (t.HasThing && PriorityData.CanPrioritize(t.Thing))
            {
                modPriority += MainMod.Data.GetPriority(t.Thing);
            }

            modPriority *= MainMod.ModConfig.priorityMultiplier;

            __result += modPriority;
        }
    }
}
