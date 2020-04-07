using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Prioritize2
{
    public class PriorityMapGrid : IExposable
    {
        private int mapUID = -1;
        private Map cachedMap;

        private int[] priorityGrid;

        public Map Map
        {
            get
            {
                Map result = null;

                if (cachedMap != null)
                {
                    if (cachedMap.uniqueID != mapUID)
                    {
                        //Log.Error("PriorityMapGrid : cachedMap uniqueID isn't equal to mapUID.");
                    }
                    else
                    {
                        result = cachedMap;
                    }
                }
                if (result == null)
                {
                    result = Find.Maps.Find((Map m) => m.uniqueID == mapUID);

                    if (result == null)
                    {
                        Log.Error("PriorityMapGrid : Cannot find map, this means this grid is invalid.");
                    }

                    cachedMap = result;
                }

                return result;
            }
        }

        public PriorityMapGrid()
        {

        }

        //Called only when it is first initialized
        public PriorityMapGrid(Map map)
        {
            mapUID = map.uniqueID;
            cachedMap = map;

            InitGrid();
        }

        private void InitGrid()
        {
            priorityGrid = new int[Map.cellIndices.NumGridCells];
        }

        public int GetPriorityAt(IntVec3 cell)
        {
            int index = Map.cellIndices.CellToIndex(cell);
            if (index >= priorityGrid.Length || index < 0)
            {
                Log.ErrorOnce("PriorityMapGrid : GetPriorityAt with invalid cell (out of range).", "P2_PMGPA".GetHashCode());
                return 0;
            }
            return priorityGrid[index];
        }

        public void SetPriorityAt(IntVec3 cell, int pri)
        {
            int index = Map.cellIndices.CellToIndex(cell);
            if (index >= priorityGrid.Length || index < 0)
            {
                Log.ErrorOnce("PriorityMapGrid : SetPriorityAt with invalid cell (out of range).", "P2_PMSPA".GetHashCode());
                return;
            }
            priorityGrid[index] = pri;
        }

        public void ExposeData()
        {
            cachedMap = null;
            Scribe_Values.Look(ref mapUID, "mapUID", -1);

            Map map = Map;

            byte[] arr = null;

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (priorityGrid == null || priorityGrid.Length != map.cellIndices.NumGridCells)
                {
                    Log.Error("PriorityMapGrid : Invalid priorityGrid. fixing..");
                    InitGrid();
                }
                arr = DataSerializeUtility.SerializeInt(priorityGrid);
            }
            DataExposeUtility.ByteArray(ref arr, "priorityGrid");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                priorityGrid = DataSerializeUtility.DeserializeInt(arr);
            }
        }
    }
}
