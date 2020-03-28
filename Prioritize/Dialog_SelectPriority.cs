using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace Prioritize
{
    public class Dialog_SelectPriority : Window
    {
        public Dialog_SelectPriority()
        {
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(300, 110);

        private string editBuffer;

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(0, 5, 30f, 30), "-1"))
            {
                MainMod.SelectedPriority -= (short)GenUI.CurrentAdjustmentMultiplier();
                editBuffer = ((int)MainMod.SelectedPriority).ToStringCached();
            }
            if (Widgets.ButtonText(new Rect(210, 5, 30f, 30), "+1"))
            {
                MainMod.SelectedPriority += (short)GenUI.CurrentAdjustmentMultiplier();
                editBuffer = ((int)MainMod.SelectedPriority).ToStringCached();
            }
            int pri = MainMod.SelectedPriority;

            Widgets.TextFieldNumeric<int>(new Rect(45, 5, 150, 30), ref pri, ref editBuffer, -32766, 32768);

            MainMod.SelectedPriority = (short) pri;
            Rect offset = new Rect(0f, 50f, 20f, 20f);
            float spacing = 3f + offset.width;

            for (int i = -5; i <= 5; i++)
            {
                if (Widgets.ButtonText(offset, i.ToString()))
                {
                    MainMod.SelectedPriority = (short) i;
                    editBuffer = i.ToStringCached();
                }
                offset.x += spacing;
            }
        }
    }
}
