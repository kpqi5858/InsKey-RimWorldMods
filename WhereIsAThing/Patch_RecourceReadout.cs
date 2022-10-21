using System;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
    public class Patch_RecourceReadout
    {
        //TODO : Convert to transpiler
        public static void Postfix(ThingDef thingDef, int nestLevel, Listing_ResourceReadout __instance)
        {
            if (Event.current.button == 0 && Event.current.type == EventType.MouseUp)
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
                catch (Exception e)
                {
                    Log.ErrorOnce("ItemListSelector Patch_RecourceReadout failed: " + e, "ILS_Patch_ResourceReadout".GetHashCode());
                    return;
                }

                //Event.current.Use();
                MainMod.instance?.WillSelectThisInStorage(thingDef);
            }
        }
    }
}
