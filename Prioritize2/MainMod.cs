using System;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace Prioritize2
{
    public class MainMod : Mod
    {
        public static PrioritizeModSettings ModConfig;

        public static PriorityData Data;

        public static MainMod Instance;

        public static int SelectedPriority = 0;

        public Harmony harmony;

        public MainMod(ModContentPack content) : base(content)
        {
            harmony = new Harmony("InsertKey.Prioritize2");
            harmony.PatchAll();

            ModConfig = GetSettings<PrioritizeModSettings>();
        }

        public override string SettingsCategory()
        {
            return "Prioritize 2";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModConfig.DoSettingsWindow(inRect);
        }
    }
}
