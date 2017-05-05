using Verse;

namespace ElderThingFaction
{
    public class ThingDef_LaserProjectile : ThingDef
    {
        public float preFiringInitialIntensity = 0f;
        public float preFiringFinalIntensity = 0f;
        public float postFiringInitialIntensity = 0f;
        public float postFiringFinalIntensity = 0f;
        public string warmupGraphicPathSingle = null;
        public int preFiringDuration = 0;
        public int postFiringDuration = 0;
        public float StartFireChance;
        public bool CanStartFire = false;
    }
}