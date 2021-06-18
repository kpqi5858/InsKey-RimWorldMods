using System;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace Prioritize2
{
    public class MainMod : Mod
    {
        public static PriorityData Data;

        public static MainMod Instance;

        public static int SelectedPriority = 0;

        public Harmony harmony;

        private static PrioritizeModSettings modConfigInt;

        public static PrioritizeModSettings ModConfig
        {
            get
            {
                if (modConfigInt == null)
                {
                    modConfigInt = Instance.GetSettings<PrioritizeModSettings>();
                }

                return modConfigInt;
            }
        }

        public MainMod(ModContentPack content) : base(content)
        {
            Instance = this;

            harmony = new Harmony("InsertKey.Prioritize2");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "P2_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModConfig.DoSettingsWindow(inRect);
        }
    }
}
