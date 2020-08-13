using HarmonyLib;
using HugsLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        private IntVec3 DragStart = IntVec3.Invalid;
        private bool IsCancelDragging = false;
        private HashSet<Thing> selectedThings = new HashSet<Thing>();
        public static BuilderTweaksMod instance;

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            instance = this;
        }
        public override void OnGUI()
        {
            //BuildingDesignatorControl();
            if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && IsCancelDragging)
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
                    Find.DesignatorManager?.Deselect();
                }
            }
        }

        //Designator_Cancel.CanDesignateThing
        private bool CanCancelBlueprint(Thing t)
        {
            return t.Faction == Faction.OfPlayer && (t is Frame || t is Blueprint);
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
                    if (CanCancelBlueprint(t) && !selectedThings.Contains(t))
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

        IntVec3 PreviousCell = IntVec3.Invalid;
        bool ClickedFlag = false;
        static Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
        IntVec3 MiddleClickCell = IntVec3.Invalid;

        public bool BuildingDesignatorControl()
        {
            if (Find.CurrentMap == null || Find.DesignatorManager == null) return true;
            var Dem = Find.DesignatorManager.SelectedDesignator;
            if (Dem != null && Dem is Designator_Build)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
                {
                    //Better than UI.mousecell
                    MiddleClickCell = Find.CameraDriver.MapPosition;
                }

                //Middle click to select designator
                if (Event.current.type == EventType.MouseUp && Event.current.button == 2 && MiddleClickCell == Find.CameraDriver.MapPosition)
                {
                    Thing targetThing = null;
                    IntVec3 UICell = UI.MouseCell();
                    Map map = Find.CurrentMap;
                    if (map == null) return false;
                    if (!UICell.InBounds(map)) return false;

                    //Search for blueprints
                    if (targetThing == null) targetThing = UICell.GetFirstThing<Blueprint>(map);

                    //Search for frames
                    if (targetThing == null) targetThing = UICell.GetFirstThing<Frame>(map);

                    //Search for Buildings
                    if (targetThing == null) targetThing = UICell.GetFirstBuilding(map);

                    //Can't find things
                    if (targetThing == null)
                    {
                        //SoundDefOf.ClickReject.PlayOneShotOnCamera();
                        Event.current.Use();
                        return false;
                    }

                    //Find designator
                    Designator_Build Desig = null;
                    Desig = BuildCopyCommandUtility.FindAllowedDesignator(targetThing.def);

                    if (Desig == null && (targetThing is Blueprint || targetThing is Frame))
                    {
                        Desig = BuildCopyCommandUtility.FindAllowedDesignator(targetThing.def.entityDefToBuild);
                    }

                    if ((targetThing.def.BuildableByPlayer || targetThing.def.entityDefToBuild?.BuildableByPlayer == true) && Desig != null)
                    {
                        //Set stuff

                        if (targetThing.Stuff != null)
                            Desig.SetStuffDef(targetThing.Stuff);
                        if ((targetThing as Blueprint_Build)?.stuffToUse != null)
                            Desig.SetStuffDef((targetThing as Blueprint_Build).stuffToUse);
                        if ((targetThing as Blueprint_Install)?.Stuff != null)
                            Desig.SetStuffDef((targetThing as Blueprint_Install).Stuff);
                        if ((targetThing as Frame)?.Stuff != null)
                            Desig.SetStuffDef((targetThing as Frame).Stuff);

                        Find.DesignatorManager.Select(Desig);
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }
                    else
                    {
                        //SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    }

                    Event.current.Use();
                    return false;
                }
                
                //Cancel drag

                //First, absorb Right click event, handle it manually
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1) Event.current.Use();

                if (IsCancelDragging)
                {
                    GenUI.DrawMouseAttachment(CancelIcon, string.Empty, 0);
                }
                if (Input.GetMouseButton(1))
                {
                    if (IsCancelDragging)
                    {
                        //RenderCancelHighlights();
                    }
                    else
                    {
                        //Start cancel dragging

                        IsCancelDragging = true;
                        DragStart = UI.MouseCell();
                        SoundDefOf.Click.PlayOneShotOnCamera(null);
                    }
                }

                //Right click up
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                {
                    IsCancelDragging = false;
                    selectedThings.Clear();
                    Event.current.Use();

                    if (selectedThings.Any())
                    {
                        selectedThings.Do(delegate (Thing t) { t.Destroy(DestroyMode.Cancel); });
                        SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
                    }
                    else
                    {
                        SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
                        Find.DesignatorManager.Deselect();
                        return false;
                    }

                }

                //While cancel dragging, left click to abort
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && IsCancelDragging)
                {
                    selectedThings.Clear();
                    IsCancelDragging = false;
                    SoundDefOf.CancelMode.PlayOneShotOnCamera(null);
                    Find.DesignatorManager.Deselect();
                    Event.current.Use();
                    return false;
                }

                //Drag to place blueprints

                var BuildDesignator = (Designator_Build)Dem;

                if (BuildDesignator.DraggableDimensions != 0) return true;

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
                    var acceptanceReport = BuildDesignator.CanDesignateCell(UI.MouseCell());

                    if (DebugSettings.godMode && acceptanceReport.Accepted) //Handle god mode
                    {
                        Traverse t = Traverse.Create(BuildDesignator);
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
                        BuildDesignator.DesignateSingleCell(UI.MouseCell());
                        BuildDesignator.Finalize(true);
                    }
                    else
                    {
                        //If this is first cell clicked
                        if (PreviousCell == IntVec3.Invalid)
                        {
                            Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.SilentInput, false);
                            BuildDesignator.Finalize(false);
                        }
                    }

                    PreviousCell = UI.MouseCell();
                    //Event.current.Use();
                }
                return false;
            }
            else //This is not Building designator
            {
                IsCancelDragging = false; 
                return true; 
            }
        }
        
        #endregion
    }

    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    public class Patch_DesignatorManager
    {
        public static bool Prefix()
        {
            return BuilderTweaksMod.instance.BuildingDesignatorControl();
        }
    }
}
