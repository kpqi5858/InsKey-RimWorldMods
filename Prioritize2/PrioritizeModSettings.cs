using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace Prioritize2
{
    public partial class PrioritizeModSettings : ModSettings
    {
        //Subclass of Workgiver_Scanner
        public List<BannedWorkGiverEntry> NoPriorityPatchOnEntries = new List<BannedWorkGiverEntry>();
        private List<Type> NoPriorityPatchOnInt;
        private bool NoPriorityPatchOnCacheDirty = true;

        //R, G, B, A
        private uint lowPriorityColorHex = 0xff0000ff;
        private uint highPriorityColorHex = 0x00ff00ff;

        //TODO print warning messages if theese values are huge
        public int priorityMax = 5;
        public int priorityMin = -5;

        public bool affectAnimals = false;

        public bool patchGenClosest = false;

        public bool universalConstructWorkgiver = true;

        //Multiplied to custom priority value
        //Can cause mod compatiblity issues if value is high. Can cause prioritizing doesn't work well if value is too low
        public float priorityMultiplier = 0.1f;

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

        //I wanna keep these list small but I maybe should consider using HashSet
        public List<Type> NoPriorityPatchOnTypes
        {
            get
            {
                if (NoPriorityPatchOnCacheDirty)
                {
                    NoPriorityPatchOnCacheDirty = false;

                    NoPriorityPatchOnInt = new List<Type>();

                    foreach (var entry in NoPriorityPatchOnEntries)
                    {
                        Type type = entry.GetTypeIfEnabled();

                        if (type != null)
                        {
                            NoPriorityPatchOnInt.Add(type);
                        }

                        //Log.Message(entry.ToString());
                    }
                }

                return NoPriorityPatchOnInt;
            }
        }
        public PrioritizeModSettings()
        {
            ResetNoPriorityPatchOnEntries();
        }

        public void ResetNoPriorityPatchOnEntries()
        {
            NoPriorityPatchOnEntries = BannedWorkGiverEntry.GetDefaultEntries().ToList();
        }

        public bool IsPatchAllowed(Type type)
        {
            return !NoPriorityPatchOnTypes.Contains(type);
        }

        public void DoSettingsWindow(Rect rect)
        {

        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref NoPriorityPatchOnEntries, "bannedworkgivers");

            Scribe_Values.Look(ref lowPriorityColorHex, "lowPriorityColor", lowPriorityColorHex);
            Scribe_Values.Look(ref highPriorityColorHex, "highPriorityColor", highPriorityColorHex);

            Scribe_Values.Look(ref priorityMax, "priorityMax", priorityMax);
            Scribe_Values.Look(ref priorityMin, "priorityMin", priorityMin);

            Scribe_Values.Look(ref affectAnimals, "affectAnimals", affectAnimals);

            Scribe_Values.Look(ref patchGenClosest, "patchGenClosest", patchGenClosest);

            Scribe_Values.Look(ref priorityMultiplier, "priorityMultiplier", priorityMultiplier);

            Scribe_Values.Look(ref universalConstructWorkgiver, "universalConstructWorkgiver", universalConstructWorkgiver);
            NoPriorityPatchOnCacheDirty = true;
        }
    }
}
