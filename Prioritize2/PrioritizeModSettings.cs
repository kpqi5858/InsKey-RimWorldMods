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
        //Workgiver_Scanner
        private List<Type> BannedPrioritizeWorkGivers = new List<Type>();

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
            BannedPrioritizeWorkGivers.Add(typeof(WorkGiver_RemoveRoof));
        }

        public bool IsPatchAllowed(Type type)
        {
            return BannedPrioritizeWorkGivers.Contains(type);
        }

        public void DoSettingsWindow(Rect rect)
        {

        }

        public override void ExposeData()
        {
            List<string> typesString = new List<string>();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                foreach (var type in BannedPrioritizeWorkGivers)
                {
                    typesString.Add(type.FullName);
                }
            }

            Scribe_Collections.Look(ref typesString, "bannedworkgivers");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                BannedPrioritizeWorkGivers.Clear();

                foreach (var str in typesString)
                {
                    Type type = GenTypes.GetTypeInAnyAssembly(str);
                    if (type == null)
                    {
                        Log.Warning("Cannot find type for : " + str + ". Ignoring.");
                    }
                    else
                    {
                        BannedPrioritizeWorkGivers.Add(type);
                    }
                }
            }

        }
    }
}
