using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Prioritize2.Patch
{
    //"Safe" patch
    [HarmonyPatch(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class Patch_GetPriority
    {
        private static bool LoggedNegWarn = false;

        public static void Postfix(Pawn pawn, TargetInfo t, ref float __result, WorkGiver_Scanner __instance)
        {
            if (!(pawn != null && pawn.Faction?.IsPlayer == true)) return;

            Map map = pawn.Map;

            if (map == null)
            {
                map = t.Map;
            }

            float modPriority = MainMod.Data.GetPriorityOnCell(map, t.Cell);

            if (t.HasThing)
            {
                modPriority += MainMod.Data.GetPriority(t.Thing);
            }

            if (__result < 0f && !LoggedNegWarn)
            {
                Log.Warning("Patching priority but old priority was less than 0. This can cause unexpected behavior. WorkGiver type was " + __instance.GetType().FullName);
                LoggedNegWarn = true;
            }

            __result += modPriority;
        }
    }
}
