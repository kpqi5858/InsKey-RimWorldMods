using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public class Patch_PlaySettings
    {
        /// <summary>
        /// 
        /// bool flag3 = resourceReadoutCategorized;
        /// Patch_WidgetRow.RRFlag = true; //Inserts this code
        /// row.ToggleableIcon(ref resourceReadoutCategorized, TexButton.CategorizedResourceReadout, "CategorizedResourceReadoutToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle, null);
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            int Find = InstList.FirstIndexOf((CodeInstruction inst) => (inst.operand as string)?.Equals("CategorizedResourceReadoutToggleButton") == true);
            Find -= 3;

            InstList.Insert(Find, new CodeInstruction(OpCodes.Stsfld, typeof(Patch_WidgetRow).GetField("RRFlag")));
            InstList.Insert(Find, new CodeInstruction(OpCodes.Ldc_I4_1));

            return InstList;
        }
    }
}
