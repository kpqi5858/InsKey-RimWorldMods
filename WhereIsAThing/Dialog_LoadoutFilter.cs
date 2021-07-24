using Verse;
using UnityEngine;

namespace ItemListSelector
{
    public class Dialog_LoadoutFilter : Window
    {
        public override Vector2 InitialSize => new Vector2(400, 650);

        private ThingFilterUI.UIState thingFilterState = new ThingFilterUI.UIState();

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

            ThingFilterUI.DoThingFilterConfigWindow(new Rect(inRect.x, inRect.y + yOffset, inRect.width, inRect.height - yOffset), thingFilterState, modSave.CategoryFilter, modSave.CategoryFilterGlobal, OpenMask);
        }
    }
}
