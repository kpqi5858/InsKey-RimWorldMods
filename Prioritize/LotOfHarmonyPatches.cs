using System;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Prioritize
{

    public static class PriorityUtils
    {
        public static float GetPriority(Thing t)
        {
            float pr = (MainMod.save.TryGetThingPriority(t, out int pri) ? pri : 0);
            if (t.Map != null && t.Position.InBounds(t.Map))
            {
                pr += MainMod.save.GetOrCreatePriorityMapData(t.Map).GetPriorityAt(t.Position);
            }

            if (MainMod.UseLowerAsHighPriority.Value) pr = -pr;

            return pr;
        }
    }
    #region UnsafePatches

    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global")]
    public class Patch_GenClosest1
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = t => PriorityUtils.GetPriority(t) + p(t);
        }
    }

    [HarmonyPatch(typeof(GenClosest), "ClosestThing_Global_Reachable")]
    public class Patch_GenClosest2
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = t => PriorityUtils.GetPriority(t) + p(t);
        }
    }

    [HarmonyPatch(typeof(GenClosest), "RegionwiseBFSWorker")]
    public class Patch_GenClosest3
    {
        public static void Prefix(ref Func<Thing, float> priorityGetter)
        {
            if (!MainMod.UseUnsafePatches) return;
            var p = priorityGetter;
            if (p == null) p = delegate (Thing t)
            {
                return 0f;
            };
            priorityGetter = t => PriorityUtils.GetPriority(t) + p(t);
        }
    }
    #endregion

    #region SafePatches

    [HarmonyPatch(typeof(WorkGiver_Scanner))]
    [HarmonyPatch("Prioritized", MethodType.Getter)]
    public class Patch_Prioritized
    {
        public static bool Prefix(WorkGiver_Scanner __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
    }


    [HarmonyPatch(typeof(WorkGiver_Scanner), "GetPriority", new Type[] { typeof(Pawn), typeof(TargetInfo) })]
    public class Patch_GetPriority
    {
        public static void Postfix(Pawn pawn, TargetInfo t, ref float __result)
        {
            if (__result < 0) return;
            if (pawn.Faction != null && !pawn.Faction.IsPlayer) return;

            Map m = pawn.Map; if (m == null) m = t.Map;

            float priority = 0f;

            if (t.HasThing)
            {
                priority += MainMod.save.TryGetThingPriority(t.Thing, out int pri) ? pri + 0.1f : 0;
            }
            priority += MainMod.save.GetOrCreatePriorityMapData(m).GetPriorityAt(t.Cell);

            if (MainMod.UseLowerAsHighPriority.Value)
            {
                __result -= priority;
            }
            else
            {
                __result += priority;
            }
        }
    }

    #endregion

    #region UtilityPatches

    [HarmonyPatch(typeof(Blueprint), "TryReplaceWithSolidThing")]
    public class Patch_Blueprint
    {
        public static void Postfix(Blueprint __instance, Thing createdThing)
        {
            if (MainMod.save.TryGetThingPriority(__instance, out int pri))
                MainMod.save.SetThingPriority(createdThing, pri);
        }
    }

    [HarmonyPatch(typeof(Frame), "FailConstruction")]
    public class Patch_Frame
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int patchphase = 0;
            foreach (var inst in instructions)
            {
                yield return inst;
                if (patchphase == 1 && inst.opcode == OpCodes.Pop)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_Frame).GetMethod("FixPriority"));
                    patchphase = 2;
                }
                if (inst.operand == typeof(GenSpawn).GetMethod("Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) }))
                {
                    patchphase = 1;
                }
                
            }
        }

        public static void FixPriority(Thing fromFrame, Thing toBlueprint)
        {
            if (MainMod.save.TryGetThingPriority(fromFrame, out int pri))
                MainMod.save.SetThingPriority(toBlueprint, pri);

        }
    }

    [HarmonyPatch(typeof(Designation), "Notify_Removing")]
    public class Patch_RemoveDesignation
    {
        public static void Prefix(Designation __instance)
        {
            if (__instance.target == null) return;
            if (MainMod.save == null) return;

            if (__instance.target.HasThing)
            {
                MainMod.save.SetThingPriority(__instance.target.Thing, 0);
            }
        }
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public class Patch_PlaySettingsControls
    {
        public static readonly Texture2D ShowPriority = ContentFinder<Texture2D>.Get("UI/Prioritize/ShowPriority");
        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (!worldView)
            {
                if (row.ButtonIcon(ShowPriority, "P_ShowPriority".Translate()))
                {
                    var listOptions = new List<FloatMenuOption>();
                    listOptions.Add(new FloatMenuOption("None".Translate(), delegate () { MainMod.ForcedDrawMode = PriorityDrawMode.None; }));
                    listOptions.Add(new FloatMenuOption("P_Cell".Translate(), delegate () { MainMod.ForcedDrawMode = PriorityDrawMode.Cell; }));
                    listOptions.Add(new FloatMenuOption("P_Thing".Translate(), delegate () { MainMod.ForcedDrawMode = PriorityDrawMode.Thing; }));
                    Find.WindowStack.Add(new FloatMenu(listOptions));
                }
            }
        }
    }
    #endregion

}
