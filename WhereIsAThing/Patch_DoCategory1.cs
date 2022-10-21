using System;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoCategory")]
    public class Patch_DoCategory1
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var GetCntIn = typeof(ResourceCounter).GetMethod("GetCountIn", new Type[] { typeof(ThingCategoryDef) });
            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == GetCntIn);

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_DoCategory1).GetMethod("PatchFunc"));

            return InstList;
        }

        public static int PatchFunc(ResourceCounter counter, ThingCategoryDef def)
        {
            return GetCountIn(counter, def);
        }

        public static int PatchFunc2(ResourceCounter counter, ThingDef def)
        {
            return GetCount(counter, def);
        }

        private static int GetCountIn(ResourceCounter counter, ThingCategoryDef cat)
        {
            int num = 0;
            for (int i = 0; i < cat.childThingDefs.Count; i++)
            {
                num += GetCount(counter, cat.childThingDefs[i]);
            }
            for (int j = 0; j < cat.childCategories.Count; j++)
            {
                if (!cat.childCategories[j].resourceReadoutRoot)
                {
                    num += GetCountIn(counter, cat.childCategories[j]);
                }
            }
            return num;
        }

        private static int GetCount(ResourceCounter counter, ThingDef rDef)
        {
            var countedAmounts = counter.AllCountedAmounts;

            if (rDef.resourceReadoutPriority == ResourceCountPriority.Uncounted)
            {
                return 0;
            }

            //Patch
            if (CategorizedOpenSave.instance?.CategoryFilter.Allows(rDef) == false)
            {
                return 0;
            }
            int result;
            if (countedAmounts.TryGetValue(rDef, out result))
            {
                return result;
            }
            Log.Error("[ILS Patch_DoCategory1] Looked for nonexistent key " + rDef + " in counted resources.");
            countedAmounts.Add(rDef, 0);
            return 0;
        }
    }
}
