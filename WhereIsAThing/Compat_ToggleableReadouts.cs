using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ItemListSelector
{
    public class Compat_ToggleableReadouts
    {
        public static void PatchIfNeeded(Harmony harmony)
        {
            if (LoadedModManager.RunningModsListForReading.FirstOrDefault((ModContentPack mod) => mod.PackageId.EqualsIgnoreCase("Owlchemist.ToggleableReadouts")) != null)
            {
                Log.Message("Compat patch with Toggleable Readouts");

                var original = GenTypes.GetTypeInAnyAssembly("ToggleableReadouts.ToggleableReadoutsUtility").GetMethod("HandleClicks", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var postfix = typeof(Compat_ToggleableReadouts).GetMethod("Patch");
                
                harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            }
        }

        public static void Patch(Event eventCurrent, EventType eventType, Rect rect, Def def)
        {
            if (eventCurrent.button == 0 && def is ThingDef)
            {
                if (eventType == EventType.MouseDown) eventCurrent.Use();
                else if (eventType == EventType.MouseUp)
                {
                    //Idk but the change may break this patch..
                    MainMod.instance?.WillSelectThisInStorage((ThingDef)def);
                }
            }
        }
    }
}
