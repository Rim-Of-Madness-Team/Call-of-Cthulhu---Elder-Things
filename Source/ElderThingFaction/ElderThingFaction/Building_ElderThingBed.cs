using Verse;
using RimWorld;

namespace ElderThingFaction
{
    public class Building_ElderThingBed : Building_Bed
    {
        private CompSecondLayer bedDoor;

        public override void SpawnSetup(Map map, bool blabla)
        {
            base.SpawnSetup(map, blabla);
            this.bedDoor = base.GetComp<CompSecondLayer>();
        }

        public bool IsPawnSleeping()
        {
            if (CurOccupants?.RandomElement() != null) return true;
            return false;
        }

        public override void Tick()
        {
            base.Tick();
            if (this.bedDoor == null) return;
            if (!IsPawnSleeping()) { this.bedDoor.ShowNow = false; return; }
            this.bedDoor.ShowNow = true;
        }
    }
}
