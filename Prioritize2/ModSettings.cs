using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class ModSettings
    {
        //Workgiver_Scanner
        private List<Type> BannedPrioritizeWorkGivers;

        public IEnumerable<Type> AllWGScannerType
        {
            get
            {
                return from def in DefDatabase<WorkGiverDef>.AllDefs where def.giverClass.IsSubclassOf(typeof(WorkGiver_Scanner)) select def.giverClass;
            }
        }

        public bool IsPatchAllowed(Type type)
        {
            return BannedPrioritizeWorkGivers.Contains(type);
        }

    }
}
