using System;
using HarmonyLib;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(WidgetRow), "ToggleableIcon")]
    public class Patch_WidgetRow
    {
        public static bool RRFlag = false;

        /// <summary>
        /// 
        /// Rect rect = new Rect(this.LeftX(24f), this.curY, 24f, 24f);
        /// Patch_PlaySettings.IconPatch(rect); //Inserts this code
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            //new Rect(float, float, float, float)
            var RectCtor = typeof(Rect).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });

            int PatchPhase = 0;

            foreach (var inst in instructions)
            {
                yield return inst;

                if (inst.operand == RectCtor && PatchPhase == 0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_WidgetRow).GetMethod("IconPatch"));

                    PatchPhase = 1;
                }
            }
        }

        public static void IconPatch(Rect rect)
        {
            if (!RRFlag) return;

            RRFlag = false;

            Event current = Event.current;

            if (!(current.button == 1 && Mouse.IsOver(rect))) return;

            if (current.type == EventType.MouseDown)
            {
                current.Use();
            }
            else if (current.type == EventType.MouseUp)
            {
                current.Use();

                Find.WindowStack.Add(new Dialog_LoadoutFilter());
            }
        }
    }
}
