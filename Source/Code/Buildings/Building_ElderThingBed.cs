using System.Linq;
using RimWorld;
using Verse;

namespace ElderThingFaction
{
    public class Building_ElderThingBed : Building_Bed
    {
        private CompSecondLayer bedDoor;

        public override void SpawnSetup(Map map, bool blabla)
        {
            base.SpawnSetup(map: map, respawningAfterLoad: blabla);
            bedDoor = GetComp<CompSecondLayer>();
        }

        
        public override void Draw()
        {
            base.Draw();
            if (bedDoor.ShowNow)
            {
                bedDoor.Props.graphicData.GraphicColoredFor(this).Draw(loc: GenThing.TrueCenter(loc: Position, rotation: Rotation, thingSize: def.size, altitude: bedDoor.Props.Altitude),
                    rot: Rotation, thing: this);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (bedDoor == null)
            {
                Log.ErrorOnce("No bed door to display", 94273);
                return;
            }
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                bedDoor.ShowNow = AnyOccupants;
            }
        }
    }
}