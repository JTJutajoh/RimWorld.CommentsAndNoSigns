using System;
using Verse;

namespace Dark.Signs
{
    // Special comment class with graphic-swapping capability more or less copied from power switches/air vents
    // Allows comments to be hidden altogether by swapping to an invisible graphic
    public class Building_Comment : Building
    {
        private Comp_Sign signComp;

        public override Graphic Graphic
        {
            get
            {
                return this.signComp.CurrentGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            signComp = base.GetComp<Comp_Sign>();
        }
    }
}
