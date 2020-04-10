using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public static class GenPriorityMod
    {
        //Just maybe..GetComponent() is slower than this?
        private static Dictionary<Map, PriorityMapData> PriorityMapDataCache = new Dictionary<Map, PriorityMapData>();

        public static PriorityMapData GetPriorityData(this Map map)
        {
            if (PriorityMapDataCache.TryGetValue(map, out PriorityMapData data))
            {
                return data;
            }
            else
            {
                var comp = map.GetComponent<PriorityMapData>();
                PriorityMapDataCache.Add(map, comp);

                return comp;
            }
        }

        //Remove from PriorityMapDataCache
        public static void MapRemoved(Map map)
        {
            PriorityMapDataCache.Remove(map);
        }
    }
}
