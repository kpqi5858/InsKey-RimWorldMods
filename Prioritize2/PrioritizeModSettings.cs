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
        public class BannedWorkGiverEntry : IExposable
        {
            public string typeName;
            public string description;

            private BannedWorkGiverEntry()
            {

            }

            public static BannedWorkGiverEntry GetBannedWorkGiverEntry(string typeName, string description)
            {
                var result = new BannedWorkGiverEntry();

                result.typeName = typeName;
                result.description = description;

                return result;
            }

            public static IEnumerable<BannedWorkGiverEntry> GetDefaultEntries()
            {
                yield return GetBannedWorkGiverEntry(typeof(WorkGiver_RemoveRoof).FullName, "P2_BannedRemoveRoof".Translate());
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref typeName, "typeName");
                Scribe_Values.Look(ref description, "description");
            }
        }

        //Subclass of Workgiver_Scanner
        public List<BannedWorkGiverEntry> NoPriorityPatchOnEntries = new List<BannedWorkGiverEntry>();
        private List<Type> NoPriorityPatchOnInt;
        private bool NoPriorityPatchOnCacheDirty = true;

        //R, G, B, A
        private uint lowPriorityColorHex = 0xff0000ff;
        private uint highPriorityColorHex = 0x00ff00ff;

        public int priorityMax = 5;
        public int priorityMin = -5;

        public bool affectAnimals = false;

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
                        Type type = GenTypes.GetTypeInAnyAssembly(entry.typeName);

                        if (type != null)
                        {
                            NoPriorityPatchOnInt.Add(type);
                        }
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


            NoPriorityPatchOnCacheDirty = true;
        }
    }
}
