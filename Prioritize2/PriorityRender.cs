using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityRender
    {
        public bool AnyDirty
        {
            get; private set;
        }

        //If null, mark all maps dirty
        public void MarkDirty(Map map)
        {
            if (map == null)
            {
                foreach (var m in Find.Maps)
                {
                    m.GetPriorityData().RenderCache.IsDirty = true;
                }
            }
            else
            {
                map.GetPriorityData().RenderCache.IsDirty = true;
            }
            AnyDirty = true;
        }

        public void Recalculate()
        {
            LongEventHandler.QueueLongEvent(
                RecalculateInternal, 
                "P2_Recalculate", 
                false, 
                delegate (Exception e) 
                { 
                    Log.Error("Exception while recalculating priority render : " + e);
                },
                false);
        }

        private void RecalculateInternal()
        {
            var data = MainMod.Data;

            var maps = from map in Find.Maps where map.GetPriorityData().RenderCache.IsDirty select map;

            foreach (var map in maps)
            {
                var priCache = map.GetPriorityData().RenderCache;

                priCache.ThingCache.Clear();
                priCache.IsDirty = false;

                foreach (var thing in map.listerThings.AllThings)
                {
                    if (MainMod.Data.Filter.Allows(thing))
                    {
                        priCache.ThingCache.Add(thing);
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

        public void RemoveFromCache(Thing t)
        {
            if (t.Map == null) return;

            t.Map.GetPriorityData().RenderCache.ThingCache.Remove(t);
        }

        public void Tick()
        {
            if (Find.Maps.NullOrEmpty()) return;

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
