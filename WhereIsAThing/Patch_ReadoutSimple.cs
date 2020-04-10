using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(ResourceReadout), "DoReadoutSimple")]
    public class Patch_ReadoutSimple
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var ThatField = typeof(ThingDef).GetField("resourceReadoutAlwaysShow");

            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == ThatField);

            for (int i = Find - 1; i >= Find - 5; i--)
            {
                InstList[i] = new CodeInstruction(OpCodes.Nop);
            }

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_ReadoutSimple).GetMethod("PatchFunc"));

            return InstList;
        }

        public static bool PatchFunc(ref KeyValuePair<ThingDef, int> pair)
        {
            bool originalRet = pair.Value > 0 || pair.Key.resourceReadoutAlwaysShow;

            return originalRet && MainMod.Save?.CategoryFilter.Allows(pair.Key) == true;
        }
    }
}
