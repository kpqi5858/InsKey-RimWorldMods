using HugsLib.Utils;
using Verse;
using System.Collections.Generic;

namespace Prioritize
{

    public class PSaveData : GameComponent
    {
        public Dictionary<int, PriorityMapData> PriorityMapDataDict = new Dictionary<int, PriorityMapData>();
        public Dictionary<int, int> ThingPriority = new Dictionary<int, int>();

        public PSaveData() {  }

        public PSaveData(Game game) { }

        public int GetOrCreateThingPriority(Thing t)
        {
            if (t == null)
            {
                Log.ErrorOnce("GetOrCreateThingPriority called with null Thing.", "P_GOCTP".GetHashCode());
                return 0;
            }
            if (ThingPriority.TryGetValue(t.thingIDNumber, out int val)) return val;
            ThingPriority.Add(t.thingIDNumber, 0); return 0;
        }

        public bool TryGetThingPriority(Thing t, out int pri)
        {
            if (t == null)
            {
                Log.ErrorOnce("TryGetThingPriority called with null Thing.", "P_TGTP".GetHashCode());
                pri = 0;
                return false;
            }
            return ThingPriority.TryGetValue(t.thingIDNumber, out pri);
        }
        public void SetThingPriority(Thing t, int p)
        {
            if (t == null)
            {
                Log.ErrorOnce("SetThingPriority called with null Thing.", "P_STP".GetHashCode());
                return;
            }
            if (ThingPriority.ContainsKey(t.thingIDNumber))
            {
                if (p == 0)
                {
                    ThingPriority.Remove(t.thingIDNumber);
                    return;
                }
                ThingPriority[t.thingIDNumber] = p;
            }
            else if (p == 0) return;
            else ThingPriority.Add(t.thingIDNumber, p);
        }
        public PriorityMapData GetOrCreatePriorityMapData(Map m)
        {
            if (m == null)
            {
                Log.Error("GetOrCreatePriorityMapData called with null Map.");
                return null;
            }
            if (PriorityMapDataDict.TryGetValue(Find.Maps.IndexOf(m), out PriorityMapData grid)) return grid;
            PriorityMapData pg = new PriorityMapData(m);
            PriorityMapDataDict.Add(Find.Maps.IndexOf(m), pg); return pg;
        }

        public void ClearUnusedThingPriority()
        {
            var newThingPri = new Dictionary<int, int>();
            foreach(Map map in Find.Maps)
            {
                var things = map.spawnedThings;
                for (int i = 0; i < things.Count; i++)
                {
                    var t = things[i];
                    if (ThingPriority.TryGetValue(t.thingIDNumber, out int v)) newThingPri.Add(t.thingIDNumber, v);
                }
            }
            ThingPriority = newThingPri;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<int, PriorityMapData>(ref PriorityMapDataDict, "prioritymapdata", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int, int>(ref ThingPriority, "thingPriority", LookMode.Value, LookMode.Value);
        }

        public void ResolvePriorityGridMaps(int mapid)
        {
            var grid = GetOrCreatePriorityMapData(Find.Maps[mapid]);
            //if (grid == null) throw new Exception("wtf");
            grid.map = Find.Maps[grid.mapId];
        }
    }
}
