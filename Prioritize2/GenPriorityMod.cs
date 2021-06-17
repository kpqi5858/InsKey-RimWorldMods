using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

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

        public static Color GetPriorityColor(this int val)
        {
            Color white = Color.white;
            Color dest = val >= 0 ? MainMod.ModConfig.HighPriorityColor : MainMod.ModConfig.LowPriorityColor;

            float fVal = val;
            float alpha = 0;
            
            if (val > 0)
            {
                alpha = fVal / MainMod.ModConfig.priorityMax;
            }
            else if (val < 0)
            {
                alpha = fVal / MainMod.ModConfig.priorityMin;
            }

            return Color.Lerp(white, dest, alpha);
        }

        public static Color GetPriorityColor_Area(this int val)
        {
            Color color = val.GetPriorityColor();

            float fVal = val;
            float alpha = 0;

            if (val > 0)
            {
                alpha = fVal / MainMod.ModConfig.priorityMax;
            }
            else if (val < 0)
            {
                alpha = fVal / MainMod.ModConfig.priorityMin;
            }

            color.a = alpha;

            return color;
        }

        public static Color FromHex(uint hexColor)
        {
            uint r = (hexColor >> 24) & 0x000000ff;
            uint g = (hexColor >> 16) & 0x000000ff;
            uint b = (hexColor >> 8) & 0x000000ff;
            uint a = (hexColor >> 0) & 0x000000ff;

            return new Color(r, g, b, a) / 255;
        }

        public static bool CanAffectedByPriority(this Pawn pawn)
        {
            if (pawn == null) return false;

            if (pawn.Faction?.IsPlayer == false) return false;

            if (pawn.RaceProps.Animal && !MainMod.ModConfig.affectAnimals) return false;

            return true;
        }

        public static void ClampPriority(this ref int priority)
        {
            if (priority < MainMod.ModConfig.priorityMin)
            {
                priority = MainMod.ModConfig.priorityMin;
            }
            if (priority > MainMod.ModConfig.priorityMax)
            {
                priority = MainMod.ModConfig.priorityMax;
            }
        }
    }
}
