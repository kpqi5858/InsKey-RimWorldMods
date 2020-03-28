using System;
using Verse;

namespace Prioritize
{
    public class PriorityMapData : IExposable
    {
        ushort[] priorityGrid = { };
        public Map map;
        public int mapId;
        int numCells = 0;
        byte[] griddata;

        public PriorityMapData(Map map)
        {
            this.map = map;
            mapId = Find.Maps.IndexOf(map);
            priorityGrid = new ushort[map.cellIndices.NumGridCells];
            for (int i = 0; i < priorityGrid.Length; i++)
            {
                priorityGrid[i] = 32768;
            }
        }

        public PriorityMapData()
        {

        }


        public short GetPriorityAt(IntVec3 loc)
        {
            ushort retval = priorityGrid[map.cellIndices.CellToIndex(loc)];
            if (retval == 0)
            {
                Log.ErrorOnce("Priority grid " + loc.ToString() + " priority is -32767, Resetting to 0..", "PG32767Error".GetHashCode());
                priorityGrid[map.cellIndices.CellToIndex(loc)] = 32768;
            }
            return (short)(priorityGrid[map.cellIndices.CellToIndex(loc)] - 32768);
        }
        public void SetPriorityAt(IntVec3 loc, short pri)
        {
            priorityGrid[map.cellIndices.CellToIndex(loc)] = (ushort)(pri + 32768);
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref mapId, "mapid", -1);
            if (map != null) numCells = map.cellIndices.NumGridCells;

            Scribe_Values.Look<int>(ref numCells, "numCells", 0);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                MapExposeUtility.ExposeUshort(map, (IntVec3 c) => priorityGrid[map.cellIndices.CellToIndex(c)], delegate (IntVec3 c, ushort val)
                {
                    priorityGrid[map.cellIndices.CellToIndex(c)] = val;
                }, "priorityGrid");
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                priorityGrid = new ushort[numCells];
                DataExposeUtility.ByteArray(ref griddata, "priorityGrid");
                DataSerializeUtility.LoadUshort(griddata, numCells, delegate (int c, ushort val)
                {
                    priorityGrid[c] = val;
                });
                griddata = null;
            }
        }

        public static void ExposeUshort(Map map, Func<IntVec3, ushort> shortReader, Action<IntVec3, ushort> shortWriter, string label)
        {
            byte[] arr = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                arr = MapSerializeUtility.SerializeUshort(map, shortReader);
            }
            DataExposeUtility.ByteArray(ref arr, label);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                MapSerializeUtility.LoadUshort(arr, map, shortWriter);
            }
        }
    }
}
