using System;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Utils;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
    public class Patch_RecourceReadout
    {
        //TODO : Convert to transpiler
        public static void Postfix(ThingDef thingDef, int nestLevel, Listing_ResourceReadout __instance)
        {
            ModLogger l = MainMod.logger;
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                try
                {
                    Traverse tr = Traverse.Create(__instance);
                    Rect rect = new Rect(0f, tr.Field("curY").GetValue<float>() - 24f, tr.Property("LabelWidth").GetValue<float>(), tr.Field("lineHeight").GetValue<float>())
                    {
                        xMin = tr.Method("XAtIndentLevel", new Type[] { typeof(int) }).GetValue<float>(nestLevel) + 18f
                    };
                    if (!Mouse.IsOver(rect)) return;
                }
                catch (Exception e) { l.ReportException(e); return; }
                Event.current.Use();
                MainMod.SelectThisInStorage(thingDef, Find.CurrentMap);
            }
        }
    }
}
