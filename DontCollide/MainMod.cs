using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;

namespace DontCollide
{
    public class MainMod : Mod
    {
        private Harmony harmony;

        public MainMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("InsertKey.DontCollide");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(PawnUtility), "ShouldCollideWithPawns")]
    public class Patch_ShouldCollideWithPawns
    {
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (__result)
            {
                if (p.Faction?.IsPlayer == true)
                {
                    __result = false;
                }
            }
        }
    }
}
