using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class Building_ElderThingChair : Building
    {
        public override void Draw()
        {
            if (this.Rotation == Rot4.North)
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint);
                this.DrawAt(result);
            }
            this.DrawAt(this.DrawPos);
        }
    }
}
