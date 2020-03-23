using Verse;

namespace ElderThingFaction
{
    public static class ElderThingUtility
    {
        public static bool IsElderThing(Pawn pawn)
        {
            return pawn?.def?.defName == "Alien_ElderThing_Race_Standard";
        }
        

    }
}