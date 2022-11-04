using Verse;

namespace ElderThingFaction
{
    public class Building_ElderThingChair : Building
    {
        public override void Draw()
        {
            if (Rotation == Rot4.North)
            {
                var result = Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint);
                DrawAt(result);
            }

            DrawAt(DrawPos);
        }
    }
}