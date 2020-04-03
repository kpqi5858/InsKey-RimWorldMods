using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Prioritize2.Patch
{
    [HarmonyPatch(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class Patch_GetPriority
    {
        public static void Postfix(Pawn pawn, TargetInfo t, ref float __result)
        {
            if (!(pawn != null && pawn.Faction?.IsPlayer == true)) return;

            Map map = pawn.Map;

            if (map == null)
            {
                map = t.Map;
            }


        }
    }
}
