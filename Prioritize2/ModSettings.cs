using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class ModSettings
    {
        private List<Type> BannedPrioritizeWorkGivers;

        public bool IsPatchAllowed(Type type)
        {
            if (!type.IsSubclassOf(typeof(WorkGiver_Scanner)))
            {
                Log.ErrorOnce("type Is not a subclass of WorkGiver_Scanner, This shouldn't happen!", "P2_IPA".GetHashCode());
                return false;
            }
            return BannedPrioritizeWorkGivers.Contains(type);
        }

    }
}
