using System;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace ItemListSelector
{
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
        }

        //For CustomThingFilters.
        //I tried to just prefix the ThingFilter_CopyAllowancesFrom_Patch
        //But why modder hid(privated) everything?
        public override void FinalizeInit()
        {
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
}
