using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Prioritize2.Patch
{
    [HarmonyPatch(typeof(Thing), "Destroy")]
    public class Patch_ThingDestroyed
    {
        public static void Postfix(Thing __instance)
        {
            //null?
            MainMod.Data?.ThingDestroyed(__instance);
        }
    }
}
