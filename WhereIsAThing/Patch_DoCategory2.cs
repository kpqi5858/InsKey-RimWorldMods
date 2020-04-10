using System;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
    public class Patch_DoCategory2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var GetCntIn = typeof(ResourceCounter).GetMethod("GetCount", new Type[] { typeof(ThingDef) });
            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == GetCntIn);

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_DoCategory1).GetMethod("PatchFunc2"));

            return InstList;
        }
    }
}
