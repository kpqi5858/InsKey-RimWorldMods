using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace BuilderTweaks
{
    [StaticConstructorOnStartup]
    public class BuilderTweaksMod : ModBase
    {
        public override string ModIdentifier => "BuilderTweaks";

        #region BuildingDesignatorTweak

        public static BuilderTweaksMod instance;

        private static Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
        private MethodInfo CheckSelectedDesignatorValid = AccessTools.Method(typeof(DesignatorManager), "CheckSelectedDesignatorValid");

        private IntVec3 DragStart = IntVec3.Invalid;
        public bool IsCancelDragging = false;
        private HashSet<Thing> selectedThings = new HashSet<Thing>();
        private IntVec3 PreviousCell = IntVec3.Invalid;
        private bool ClickedFlag = false;
        private Vector2 previousMousePos;

        //Drag with right click to cancel building designation
        private SettingHandle<bool> enableCancelDragging;
        //^ Only cancel the same selected building designation
        private SettingHandle<bool> cancelOnlySameKind;
        //Drag with left click to continuously place designation
        private SettingHandle<bool> enableContinuousDesignating;
        //Middle click to select designator of the building below
        private SettingHandle<bool> enableMiddleClick;

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            instance = this;

            enableCancelDragging = Settings.GetHandle("enableCancelDragging", "BT_enableCancelDragging_title".Translate(), "BT_enableCancelDragging_desc".Translate(), true);
            cancelOnlySameKind = Settings.GetHandle("cancelOnlySameKind", "BT_cancelOnlySameKind_title".Translate(), "BT_cancelOnlySameKind_desc".Translate(), true);
            enableContinuousDesignating = Settings.GetHandle("enableContinuousDesignating", "BT_enableContinuousDesignating_title".Translate(), "BT_enableContinuousDesignating_desc".Translate(), true);
            enableMiddleClick = Settings.GetHandle("enableMiddleClick", "BT_enableMiddleClick_title".Translate(), "BT_enableMiddleClick_desc".Translate(), true);
        }

        //Designator_Cancel.CanDesignateThing
        private bool ShouldCancelBlueprint(Thing t)
        {
            if (t.Faction != Faction.OfPlayer) return false;
            if (!(t is Frame || t is Blueprint)) return false;
            if (cancelOnlySameKind.Value)
            {
                BuildableDef selectedThing = (Find.DesignatorManager?.SelectedDesignator as Designator_Build)?.PlacingDef;
                return selectedThing == null ? true : t.def.entityDefToBuild == selectedThing;
            }

            return true;
        }

        private void RenderCancelHighlights()
        {
            selectedThings.Clear();
            CellRect dragRect = CellRect.FromLimits(DragStart, UI.MouseCell());

            foreach (IntVec3 CurCell in dragRect)
            {
                if (!CurCell.InBounds(Find.CurrentMap))
                {
                    continue;
                }
                var thingslist = CurCell.GetThingList(Find.CurrentMap);
                for (int i = 0; i < thingslist.Count; i++)
                {
                    var t = thingslist[i];
                    if (ShouldCancelBlueprint(t) && !selectedThings.Contains(t))
                    {
                        selectedThings.Add(t);
                        Vector3 drawPos = t.DrawPos;
                        drawPos.y = AltitudeLayer.MetaOverlays.AltitudeFor();
                        Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, DesignatorUtility.DragHighlightThingMat, 0);
                    }
                }
            }
        }

        public override void Update()
        {
            if (IsCancelDragging)
            {
                RenderCancelHighlights();
            }
        }

        public bool BuildingDesignatorControl2()
        {
            if (Find.CurrentMap == null || Find.DesignatorManager == null) return true;

            var Dem = Find.DesignatorManager.SelectedDesignator;

            //This is not Building designator, or invalid
            if (!(Dem is Designator_Build) || !(bool)CheckSelectedDesignatorValid.Invoke(Find.DesignatorManager, null))
            {
                IsCancelDragging = false;
                return true;
            }

            if (IsCancelDragging)
            {
                GenUI.DrawMouseAttachment(CancelIcon, string.Empty, 0);
            }

            return MiddleClickFeature() && CancelDragFeature() && LeftDragFeature();
        }

        public bool MiddleClickFeature()
        {
            if (!enableMiddleClick.Value) return true;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                previousMousePos = UI.MousePositionOnUI;
            }
            //Middle click to select designator
            if (Event.current.type == EventType.MouseUp && Event.current.button == 2 && previousMousePos == UI.MousePositionOnUI)
            {
                IntVec3 UICell = UI.MouseCell();
                Map map = Find.CurrentMap;
                if (!UICell.InBounds(map)) return false;

                //Search for Blueprint, Frame, and Building in order
                //Why I need to cast first to Thing? weird..
                Thing targetThing = UICell.GetFirstThing<Blueprint>(map) as Thing
                                 ?? UICell.GetFirstThing<Frame>(map)
                                 ?? UICell.GetFirstBuilding(map);

                //Can't find things
                if (targetThing == null)
                {
                    Event.current.Use();
                    return false;
                }

                //Find designator
                Designator_Build designator = BuildCopyCommandUtility.FindAllowedDesignator(targetThing.def);

                if (designator == null && (targetThing is Blueprint || targetThing is Frame))
                {
                    designator = BuildCopyCommandUtility.FindAllowedDesignator(targetThing.def.entityDefToBuild);
                }

                if (designator != null && (targetThing.def.entityDefToBuild?.BuildableByPlayer ?? targetThing.def.BuildableByPlayer))
                {
                    //Set stuff

                    ThingDef stuff = targetThing.Stuff
                                  ?? (targetThing as Blueprint_Build)?.stuffToUse
                                  ?? (targetThing as Blueprint_Install)?.Stuff
                                  ?? (targetThing as Frame)?.Stuff;

                    if (stuff != null) designator.SetStuffDef(stuff);

                    Find.DesignatorManager.Select(designator);
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }

                Event.current.Use();
                return false;
            }
            return true;
        }

        public bool CancelDragFeature()
        {
            if (!enableCancelDragging.Value) return true;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                //Start cancel dragging

                IsCancelDragging = true;
                DragStart = UI.MouseCell();
                SoundDefOf.Click.PlayOneShotOnCamera(null);

                Event.current.Use();
            }
            //Right click up
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && IsCancelDragging)
            {
                IsCancelDragging = false;
                PreviousCell = IntVec3.Invalid;
                if (selectedThings.Any())
                {
                    selectedThings.Do(delegate (Thing t) { t.Destroy(DestroyMode.Cancel); });
                    selectedThings.Clear();
                    SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
                    Event.current.Use();
                }
                else
                {
                    SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
                    Find.DesignatorManager.Deselect();
                    return false;
                }
            }
            //While cancel dragging, left click to abort
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && IsCancelDragging)
            {
                selectedThings.Clear();
                IsCancelDragging = false;
                SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
                Find.DesignatorManager.Deselect();
                Event.current.Use();
                return false;
            }
            return true;
        }

        public bool LeftDragFeature()
        {
            if (!enableContinuousDesignating.Value) return true;

            var designator = (Designator_Build)Find.DesignatorManager.SelectedDesignator;
            if (designator.DraggableDimensions != 0) return true;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                PreviousCell = IntVec3.Invalid;
                ClickedFlag = true;
                Event.current.Use();
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                PreviousCell = IntVec3.Invalid;
                ClickedFlag = false;
                Event.current.Use();
            }

            if (Input.GetMouseButton(0) && !Mouse.IsInputBlockedNow && PreviousCell != UI.MouseCell() && ClickedFlag)
            {
                var acceptanceReport = designator.CanDesignateCell(UI.MouseCell());

                if (DebugSettings.godMode && acceptanceReport.Accepted) //Handle god mode
                {
                    Traverse t = Traverse.Create(designator);
                    BuildableDef entDef = t.Field("entDef").GetValue<BuildableDef>();
                    Rot4 rot = t.Field("placingRot").GetValue<Rot4>();
                    CellRect cellRect = GenAdj.OccupiedRect(UI.MouseCell(), rot, entDef.Size);
                    foreach (IntVec3 c in cellRect)
                    {
                        var thinglist = c.GetThingList(Find.CurrentMap);
                        for (int i = 0; i < thinglist.Count; i++)
                        {
                            var thing3 = thinglist[i];
                            if (!GenConstruct.CanPlaceBlueprintOver(entDef, thing3.def))
                            {
                                acceptanceReport = new AcceptanceReport("SpaceAlreadyOccupied_DevFail");
                            }
                        }
                    }
                }
                if (acceptanceReport.Accepted)
                {
                    designator.DesignateSingleCell(UI.MouseCell());
                    designator.Finalize(true);
                }
                else
                {
                    //If this is first cell clicked
                    if (PreviousCell == IntVec3.Invalid)
                    {
                        Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.SilentInput, false);
                        designator.Finalize(false);
                    }
                }

                PreviousCell = UI.MouseCell();
                return false;
            }
            return true;
        }

        #endregion
    }

    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    public class Patch_DesignatorManager
    {
        public static bool Prefix()
        {
            return BuilderTweaksMod.instance.BuildingDesignatorControl2();
        }
    }
}
