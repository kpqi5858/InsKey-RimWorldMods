using System;
using HugsLib;
using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using HugsLib.Utils;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld.Planet;

namespace ItemListSelector
{
    public class Dialog_LoadoutFilter : Window
    {
        public override Vector2 InitialSize => new Vector2(400, 650);

        private Vector2 scrollPosition;

        public Dialog_LoadoutFilter()
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var modSave = MainMod.Save;
            int OpenMask = 9;
            float yOffset = 10;

            ThingFilterUI.DoThingFilterConfigWindow(new Rect(inRect.x, inRect.y + yOffset, inRect.width, inRect.height - yOffset), ref scrollPosition, modSave.CategoryFilter, modSave.CategoryFilterGlobal, OpenMask);
        }
    }


    public class CategorizedOpenSave : GameComponent
    {
        public List<string> OpenNodesName = new List<string>();
        public static readonly int OpenMaskVal = 32;

        private static Action CallbackAction = SettingChangedCallback;

        public ThingFilter CategoryFilter;
        public ThingFilter CategoryFilterGlobal;

        public CategorizedOpenSave(Game game) 
        {
            if (CategoryFilterGlobal == null)
            {
                CategoryFilterGlobal = new ThingFilter();
            }

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.CountAsResource)
                {
                    CategoryFilterGlobal.SetAllow(def, true);
                }
            }

            InitCategoryFilter();
        }

        private void InitCategoryFilter()
        {
            CategoryFilter = new ThingFilter(CallbackAction);
            CategoryFilter.CopyAllowancesFrom(CategoryFilterGlobal);
        }

        public static IEnumerable<ThingCategoryDef> AllCategoryDefs
        {
            get
            {
                return DefDatabase<ThingCategoryDef>.AllDefs;
            }
        }

        private IEnumerable<ThingCategoryDef> OpenNodes
        {
            get
            {
                foreach (var name in OpenNodesName)
                {
                    var def = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(name);
                    if (def != null) yield return def;
                }
                yield break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                OpenNodesName = (from def in AllCategoryDefs where def.treeNode?.IsOpen(OpenMaskVal) == true select def.defName).ToList();
                Scribe_Collections.Look(ref OpenNodesName, "OpenNodes");
            }
            else
            {
                Scribe_Collections.Look(ref OpenNodesName, "OpenNodes");
            }

            Scribe_Deep.Look(ref CategoryFilter, "CategoryFilter", CallbackAction);
            if (CategoryFilter == null)
            {
                Log.Message("CategoryFilter is null, probably old save. Fixing..");

                if (Scribe.mode != LoadSaveMode.LoadingVars)
                {
                    Log.Warning("But Scribe.mode is not LoadingVars. shouldn't happen?");
                }

                InitCategoryFilter();
            }
        }

        public void ExpandCategories()
        {
            foreach (var node in AllCategoryDefs)
            {
                node?.treeNode?.SetOpen(OpenMaskVal, false);
            }

            var Temp = OpenNodes;
            foreach (var node in Temp)
            {
                //Node maybe null if ThingCategoryDef removed by disabling mods
                node?.treeNode?.SetOpen(OpenMaskVal, true);
            }
        }

        public static void SettingChangedCallback()
        {
            Find.CurrentMap?.resourceCounter?.UpdateResourceCounts();
        }
    }

    public class MainMod : ModBase
    {
        public override string ModIdentifier => "ItemListSelector";

        public static ModLogger logger;

        public static List<string> t = new List<string>();
        public static CategorizedOpenSave Save;

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            logger = Logger;
            foreach(string s in t)
            {
                Logger.Message(s);
            }
        }

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            Save = Current.Game.GetComponent<CategorizedOpenSave>();
            Save.ExpandCategories();
        }

        public static void SelectThisInStorage(ThingDef thingDef, Map map)
        {
            Selector s = Find.Selector;
            if (s == null) return;

            if (!Input.GetKey(KeyCode.LeftShift)) s.ClearSelection();
            Func<Thing, bool> predicate = delegate (Thing t)
            {
                if (t == null) return false;
                if (t.def != thingDef || s.IsSelected(t))
                {
                    return false;
                }
                return true;
            };
            var thingsGroupsList = Find.CurrentMap.haulDestinationManager.AllGroupsListForReading;

            for (int i = 0; i < thingsGroupsList.Count; i++)
            {
                var sg = thingsGroupsList[i];
                sg.HeldThings.DoIf(predicate, (Thing t) => s.Select(t));
            }
            s.dragBox.active = false;
        }
    }

    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
    public class Patch_RecourceReadout
    {
        //TODO : Convert to transpiler
        public static void Postfix(ThingDef thingDef, int nestLevel, Listing_ResourceReadout __instance)
        {
            ModLogger l = MainMod.logger;
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                try
                {
                    Traverse tr = Traverse.Create(__instance);
                    Rect rect = new Rect(0f, tr.Field("curY").GetValue<float>() - 24f, tr.Property("LabelWidth").GetValue<float>(), tr.Field("lineHeight").GetValue<float>())
                    {
                        xMin = tr.Method("XAtIndentLevel", new Type[] { typeof(int) }).GetValue<float>(nestLevel) + 18f
                    };
                    if (!Mouse.IsOver(rect)) return;
                }
                catch (Exception e) { l.ReportException(e); return; }
                Event.current.Use();
                MainMod.SelectThisInStorage(thingDef, Find.CurrentMap);
            }
        }
    }

    [HarmonyPatch(typeof(ResourceReadout), "DrawIcon", null)]
    public class Patch_DrawIcon
    {
        public static void Postfix(float x, float y, ThingDef thingDef)
        {
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                if (!Mouse.IsOver(new Rect(x, y, 50f, 27f)))
                {
                    return;
                }
                Event.current.Use();
                MainMod.SelectThisInStorage(thingDef, Find.CurrentMap);
            }
        }
    }


    [HarmonyPatch(typeof(WidgetRow), "ToggleableIcon")]
    public class Patch_WidgetRow
    {
        public static bool RRFlag = false;

        /// <summary>
        /// 
        /// Rect rect = new Rect(this.LeftX(24f), this.curY, 24f, 24f);
        /// Patch_PlaySettings.IconPatch(rect); //Inserts this code
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            //new Rect(float, float, float, float)
            var RectCtor = typeof(Rect).GetConstructor(new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) });

            int PatchPhase = 0;

            foreach (var inst in instructions)
            {
                yield return inst;

                if (inst.operand == RectCtor && PatchPhase == 0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Patch_WidgetRow).GetMethod("IconPatch"));

                    PatchPhase = 1;
                }
            }
        }

        public static void IconPatch(Rect rect)
        {
            if (!RRFlag) return;

            RRFlag = false;

            Event current = Event.current;

            if (!(current.button == 1 && Mouse.IsOver(rect))) return;

            if (current.type == EventType.MouseDown)
            {
                current.Use();
            }
            else if (current.type == EventType.MouseUp)
            {
                current.Use();

                Find.WindowStack.Add(new Dialog_LoadoutFilter());
            }
        }
    }

    [HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
    public class Patch_PlaySettings
    {
        /// <summary>
        /// 
        /// bool flag3 = resourceReadoutCategorized;
        /// Patch_WidgetRow.RRFlag = true; //Inserts this code
        /// row.ToggleableIcon(ref resourceReadoutCategorized, TexButton.CategorizedResourceReadout, "CategorizedResourceReadoutToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle, null);
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            int Find = InstList.FirstIndexOf((CodeInstruction inst) => (inst.operand as string)?.Equals("CategorizedResourceReadoutToggleButton") == true);
            Find -= 3;

            InstList.Insert(Find, new CodeInstruction(OpCodes.Stsfld, typeof(Patch_WidgetRow).GetField("RRFlag")));
            InstList.Insert(Find, new CodeInstruction(OpCodes.Ldc_I4_1));

            return InstList;
        }
    }


    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoCategory")]
    public class Patch_DoCategory1
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var GetCntIn = typeof(ResourceCounter).GetMethod("GetCountIn", new Type[] { typeof(ThingCategoryDef) });
            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == GetCntIn);

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_DoCategory1).GetMethod("PatchFunc"));

            return InstList;
        }

        public static int PatchFunc(ResourceCounter counter, ThingCategoryDef def)
        {
            return GetCountIn(counter, def);
        }

        public static int PatchFunc2(ResourceCounter counter, ThingDef def)
        {
            return GetCount(counter, def);
        }

        private static int GetCountIn(ResourceCounter counter, ThingCategoryDef cat)
        {
            int num = 0;
            for (int i = 0; i < cat.childThingDefs.Count; i++)
            {
                num += GetCount(counter, cat.childThingDefs[i]);
            }
            for (int j = 0; j < cat.childCategories.Count; j++)
            {
                if (!cat.childCategories[j].resourceReadoutRoot)
                {
                    num += GetCountIn(counter, cat.childCategories[j]);
                }
            }
            return num;
        }

        private static int GetCount(ResourceCounter counter, ThingDef rDef)
        {
            var countedAmounts = counter.AllCountedAmounts;

            if (rDef.resourceReadoutPriority == ResourceCountPriority.Uncounted)
            {
                return 0;
            }

            //Patch
            if (MainMod.Save?.CategoryFilter.Allows(rDef) == false)
            {
                return 0;
            }
            int result;
            if (countedAmounts.TryGetValue(rDef, out result))
            {
                return result;
            }
            Log.Error("[ILS Patch_DoCategory1] Looked for nonexistent key " + rDef + " in counted resources.", false);
            countedAmounts.Add(rDef, 0);
            return 0;
        }
    }

    [HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
    public class Patch_DoCategory2
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var GetCntIn = typeof(ResourceCounter).GetMethod("GetCount", new Type[] { typeof(ThingDef) });
            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == GetCntIn);

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_DoCategory1).GetMethod("PatchFunc2"));

            return InstList;
        }
    }

    [HarmonyPatch(typeof(ResourceReadout), "DoReadoutSimple")]
    public class Patch_ReadoutSimple
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var InstList = instructions.ToList();

            var ThatField = typeof(ThingDef).GetField("resourceReadoutAlwaysShow");

            int Find = InstList.FirstIndexOf((CodeInstruction inst) => inst.operand == ThatField);

            for (int i = Find - 1; i >= Find - 5; i--)
            {
                InstList[i] = new CodeInstruction(OpCodes.Nop);
            }

            InstList[Find] = new CodeInstruction(OpCodes.Call, typeof(Patch_ReadoutSimple).GetMethod("PatchFunc"));

            return InstList;
        }

        public static bool PatchFunc(ref KeyValuePair<ThingDef, int> pair)
        {
            bool originalRet = pair.Value > 0 || pair.Key.resourceReadoutAlwaysShow;

            return originalRet && MainMod.Save?.CategoryFilter.Allows(pair.Key) == true;
        }
    }
}
