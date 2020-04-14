using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace Prioritize2
{
    public class PrioritizeModSettings : ModSettings
    {
        //Subclass of Workgiver_Scanner
        private List<Type> NoPriorityPatchOn = new List<Type>();

        //R, G, B, A
        private uint lowPriorityColorHex = 0xff0000ff;
        private uint highPriorityColorHex = 0x00ff00ff;

        public int priorityMax = 10;
        public int priorityMin = -10;

        public Color LowPriorityColor
        {
            get
            {
                return GenPriorityMod.FromHex(lowPriorityColorHex);
            }
        }

        public Color HighPriorityColor
        {
            get
            {
                return GenPriorityMod.FromHex(highPriorityColorHex);
            }
        }

        public IEnumerable<Type> AllWGScannerType
        {
            get
            {
                return from def in DefDatabase<WorkGiverDef>.AllDefs where def.giverClass.IsSubclassOf(typeof(WorkGiver_Scanner)) select def.giverClass;
            }
        }

        public PrioritizeModSettings()
        {
            //Set default values

            //Modifing priority on it can cause pawns to remove wrong roofs,
            NoPriorityPatchOn.Add(typeof(WorkGiver_RemoveRoof));
        }

        public bool IsPatchAllowed(Type type)
        {
            return NoPriorityPatchOn.Contains(type);
        }

        public void DoSettingsWindow(Rect rect)
        {

        }

        public override void ExposeData()
        {
            List<string> typesString = new List<string>();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var type in NoPriorityPatchOn)
                {
                    typesString.Add(type.FullName);
                }
            }

            Scribe_Collections.Look(ref typesString, "bannedworkgivers");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                NoPriorityPatchOn.Clear();

                foreach (var str in typesString)
                {
                    Type type = GenTypes.GetTypeInAnyAssembly(str);
                    if (type == null)
                    {
                        Log.Warning("Cannot find type for : " + str + ". Ignoring.");
                    }
                    else
                    {
                        NoPriorityPatchOn.Add(type);
                    }
                }
            }

            Scribe_Values.Look(ref lowPriorityColorHex, "lowPriorityColor", lowPriorityColorHex);
            Scribe_Values.Look(ref highPriorityColorHex, "highPriorityColor", highPriorityColorHex);

            Scribe_Values.Look(ref priorityMax, "priorityMax", priorityMax);
            Scribe_Values.Look(ref priorityMin, "priorityMin", priorityMin);
        }
    }
}
