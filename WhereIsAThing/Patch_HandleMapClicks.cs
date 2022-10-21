using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(MapInterface), "HandleMapClicks")]
    class Patch_HandleMapClicks
    {
        public static void Prefix()
        {
            if (Find.CurrentMap == null || MainMod.instance == null) return;
            MainMod.instance.DoIfNotConsumed();
        }
    }
}
