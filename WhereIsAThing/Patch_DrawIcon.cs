using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItemListSelector
{
    [HarmonyPatch(typeof(ResourceReadout), "DrawIcon", null)]
    public class Patch_DrawIcon
    {
        public static void Postfix(float x, float y, ThingDef thingDef)
        {
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                if (!Mouse.IsOver(new Rect(x, y, 50f, 27f)))
                {
                    return;
                }
                Event.current.Use();
                MainMod.SelectThisInStorage(thingDef, Find.CurrentMap);
            }
        }
    }
}
