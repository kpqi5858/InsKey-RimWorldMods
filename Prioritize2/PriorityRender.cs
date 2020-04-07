using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityRender
    {
        private class MapPriorityCache
        {
            public List<Thing> CachedThings = new List<Thing>();
            public bool IsDirty = false;
        }

        private Dictionary<Map, MapPriorityCache> PriorityRenderCache = new Dictionary<Map, MapPriorityCache>();

        public bool AnyDirty
        {
            get; private set;
        }

        private MapPriorityCache GetOrCreateMapPriorityCache(Map map)
        {
            if (!PriorityRenderCache.ContainsKey(map))
            {
                PriorityRenderCache.Add(map, new MapPriorityCache());
            }
            return PriorityRenderCache[map];
        }

        //If null, mark all maps dirty
        public void MarkDirty(Map map)
        {
            if (map == null)
            {
                foreach (var m in Find.Maps)
                {
                    GetOrCreateMapPriorityCache(m).IsDirty = true;
                }
            }
            else
            {
                GetOrCreateMapPriorityCache(map).IsDirty = true;
            }
            AnyDirty = true;
        }

        public void Recalculate()
        {
            LongEventHandler.QueueLongEvent(
                RecalculateInternal, 
                "P2_Recalculate", 
                true, 
                delegate (Exception e) 
                { 
                    Log.Error("Exception while recalculating priority render : " + e);
                },
                false);
        }

        private void RecalculateInternal()
        {
            var data = MainMod.Data;

            var maps = from pair in PriorityRenderCache where pair.Value.IsDirty select pair.Key;

            foreach (var map in maps)
            {
                var priCache = GetOrCreateMapPriorityCache(map);

                priCache.CachedThings.Clear();
                priCache.IsDirty = false;

                var mapThings = map.listerThings.AllThings;

                for (int i = 0; i < mapThings.Count; i++)
                {
                    var thing = mapThings[i];
                    if (MainMod.Data.Filter.Allows(thing))
                    {
                        priCache.CachedThings.Add(thing);
                    }
                }
            }

            if (AnyDirty == false)
            {
                Log.Warning("Recalculate called with AnyDirty = false");
            }
            if (maps.Count() == 0)
            {
                Log.Warning("Recalculate called but there's no dirty map caches.");
            }

            AnyDirty = false;
        }

        public void Notify_MapRemoved(Map map)
        {
            PriorityRenderCache.Remove(map);
        }

        public void Tick()
        {
            //If Priority Marks is being rendered and AnyDirty
            if (true && AnyDirty)
            {
                Recalculate();
            }

            //If Priority Marks is being rendered
            if (true)
            {
                RenderPriorityMarks();
            }
        }

        public void RenderPriorityMarks()
        {
            
        }
    }
}
