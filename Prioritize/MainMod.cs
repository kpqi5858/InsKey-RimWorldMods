using System;
using System.Collections.Generic;
using HugsLib;
using HugsLib.Utils;
using Verse;
using UnityEngine;
using RimWorld.Planet;
using RimWorld;
using Verse.Sound;
using HugsLib.Settings;

namespace Prioritize
{
    public class MainMod : ModBase
    {
        public override string ModIdentifier => "Priortize";

        public static short SelectedPriority = 0;
        public static PSaveData save;

        /// <summary>
        /// True  -> Patch GenClosest prioritygetter directly
        /// False -> Patch WorkGiver priority
        /// </summary>
        public static SettingHandle<bool> UseUnsafePatches = null;

        public static SettingHandle<bool> UseLowerAsHighPriority = null;

        public static Func<Thing, bool> ThingShowCond = PriorityShowConditions.DefaultCondition.Cond;

        public static PriorityDrawMode PriorityDraw = PriorityDrawMode.None;

        public static PriorityDrawMode ForcedDrawMode = PriorityDrawMode.None;


        public static HashSet<int> DestroyedThingId = new HashSet<int>();

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            UseUnsafePatches = Settings.GetHandle<bool>("P_UseUnsafePatches", "P_UseUnsafePatchesTitle".Translate(), "P_UseUnsafePatchesDesc".Translate(), false);
            UseLowerAsHighPriority = Settings.GetHandle<bool>("P_UseLowerAsHighPriority", "P_UseLowerAsHighPriorityTitle".Translate(), "P_UseLowerAsHighPriorityDesc".Translate(), false);
        }
        public override void WorldLoaded()
        {
            base.WorldLoaded();
            save = Current.Game.GetComponent<PSaveData>();
        }

        public override void MapLoaded(Map map)
        {
            base.MapLoaded(map);
            //save.ClearThingPriorities(map);
            save.ResolvePriorityGridMaps(Find.Maps.IndexOf(map));
        }

        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);
            RemoveThingPriorityNow();
        }

        public static void RemoveThingPriorityNow()
        {
            if (save == null) return;
            foreach (var pair in DestroyedThingId)
            {
                save.ThingPriority.Remove(pair);
            }
            DestroyedThingId.Clear();
        }

        private void AdjustPriorityMouseControl()
        {
            if (Event.current.type == EventType.ScrollWheel && Input.GetKey(KeyCode.LeftControl))
            {
                SelectedPriority -= Event.current.delta.y >= 0 ? (short)1 : (short)-1;
                SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                Event.current.Use();
            }
        }

        public override void OnGUI()
        {
            if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow) return;
            if (Find.DesignatorManager == null) return;
            //Logger.Message(Find.DesignatorManager.SelectedDesignator.GetType().ToString());
            if (Find.DesignatorManager.SelectedDesignator is Designator_Priority_Cell) PriorityDraw = PriorityDrawMode.Cell;
            else if (Find.DesignatorManager.SelectedDesignator is Designator_Priority_Thing) PriorityDraw = PriorityDrawMode.Thing;
            else PriorityDraw = ForcedDrawMode;

            Map map = Find.CurrentMap;

            if (PriorityDraw != PriorityDrawMode.None)
            {
                AdjustPriorityMouseControl();

                CellRect rect = GetMapRect();
                if (rect.Area >= 10000) return;
                foreach (IntVec3 intVec in rect)
                {
                    if (!intVec.InBounds(map)) continue;
                    if (PriorityDraw == PriorityDrawMode.Cell)
                    {
                        Vector3 v = GenMapUI.LabelDrawPosFor(intVec);
                        int p = save.GetOrCreatePriorityMapData(map).GetPriorityAt(intVec);
                        if (p == 0) continue;
                        DrawThingLabel(v, p.ToString(), GetPriorityDrawColor(true, p));
                    }
                    else if (PriorityDraw == PriorityDrawMode.Thing)
                    {
                        var th = intVec.GetThingList(map);
                        for (int j = 0; j < th.Count; j++)
                        {
                            var thing = th[j];
                            if (ThingShowCond(thing) && save.TryGetThingPriority(thing, out int pri)) DrawThingLabel(GenMapUI.LabelDrawPosFor(thing, 0f), pri.ToString(), GetPriorityDrawColor(false, pri));
                        }
                    }
                }
            }

        }

        public static Color GetPriorityDrawColor(bool IsCell, float pri)
        {
            Color CellColorUpper = new Color(0, 0, 1); //Blue
            Color CellColorDown =  new Color(1, 0.5f, 0); //Orange

            Color ThingColorUpper = new Color(0, 1, 0); //Green
            Color ThingColorDown  = new Color(1, 0, 0); //Red


            Color ColorUpper = IsCell ? CellColorUpper : ThingColorUpper;
            Color ColorDown  = IsCell ? CellColorDown  : ThingColorDown;

            float ThresholdPri = 6.25f;
            if (UseLowerAsHighPriority) pri = -pri;

            Color res = Color.white;
            if (pri > 0)
            {
                res = Color.Lerp(res, ColorUpper, pri / ThresholdPri);
            }
            if (pri < 0)
            {
                res = Color.Lerp(res, ColorDown, -pri / ThresholdPri);
            }

            return res;
        }

        private static CellRect GetMapRect()
        {
            Rect rect = new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight);
            Vector2 screenLoc = new Vector2(rect.x, (float)UI.screenHeight - rect.y);
            Vector2 screenLoc2 = new Vector2(rect.x + rect.width, (float)UI.screenHeight - (rect.y + rect.height));
            Vector3 vector = UI.UIToMapPosition(screenLoc);
            Vector3 vector2 = UI.UIToMapPosition(screenLoc2);
            return new CellRect
            {
                minX = Mathf.FloorToInt(vector.x),
                minZ = Mathf.FloorToInt(vector2.z),
                maxX = Mathf.FloorToInt(vector2.x),
                maxZ = Mathf.FloorToInt(vector.z)
            };
        }

        public static void DrawThingLabel(Vector2 screenPos, string text, Color textColor)
        {
            SetProperDrawSize();
            float x = Text.CalcSize(text).x;
            Rect position = new Rect(screenPos.x - x / 2f - 4f, screenPos.y, x + 8f, 12f);
            //GUI.DrawTexture(position, TexUI.GrayTextBG);
            GUI.color = textColor;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect rect = new Rect(screenPos.x - x / 2f, screenPos.y - 3f, x, 999f);
            Widgets.Label(rect, text);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private static void SetProperDrawSize()
        {
            if (GetMapRect().Area > 10000) Text.Font = GameFont.Tiny;
            else if (GetMapRect().Area > 5000) Text.Font = GameFont.Small;
            else Text.Font = GameFont.Medium;
        }
    }
}
