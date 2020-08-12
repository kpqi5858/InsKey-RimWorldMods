using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;

namespace RWSCompressor
{
    //PLEASE use transpiler next time. Try not to mod in this way.

    public class Compat_BetterModMismatchWindow
    {
        public static void SetupIfNeeded(Harmony harmony)
        {
            //TODO: Improve this finding logic?
            if (LoadedModManager.RunningModsListForReading.FirstOrDefault((ModContentPack mod) => mod.PackageIdPlayerFacing.Equals("Madeline.ModMismatchFormatter")) != null)
            {
                Log.Message("Compat patch with Better ModMismatch Window");

                MethodBase funcBeginReading = GenTypes.GetTypeInAnyAssembly("Madeline.ModMismatchFormatter.MetaHeaderUtility").GetMethod("BeginReading");
                HarmonyMethod transpiler = new HarmonyMethod(typeof(Compat_BetterModMismatchWindow).GetMethod("BeginReading_Transpiler"));

                harmony.Patch(funcBeginReading, null, null, transpiler, null);
            }
        }

        public static IEnumerable<CodeInstruction> BeginReading_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            foreach (var inst in instructions)
            {
                if (inst.operand == typeof(XDocument).GetMethod("Load", new Type[] { typeof(string) }))
                {
                    inst.operand = typeof(Compat_BetterModMismatchWindow).GetMethod("Load_Patch");
                }
                yield return inst;
            }
            yield break;
        }

        public static XDocument Load_Patch(string filePath)
        {
            return XDocument.Load(MainMod.GetRightReadStream(filePath));
        }
    }
}
