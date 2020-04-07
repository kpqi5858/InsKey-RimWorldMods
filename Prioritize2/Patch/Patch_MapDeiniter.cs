using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using HarmonyLib;

namespace Prioritize2.Patch
{
    [HarmonyPatch(typeof(MapDeiniter), "NotifyEverythingWhichUsesMapReference")]
    public class Patch_MapDeiniter
    {
        public static void Prefix(Map map)
        {
            MainMod.Data.Notify_MapRemoved(map);
        }
    }
}
