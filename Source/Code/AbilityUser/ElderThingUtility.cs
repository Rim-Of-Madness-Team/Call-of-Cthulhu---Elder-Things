using Verse;

namespace ElderThingFaction
{
    public static class ElderThingUtility
    {
        public static string ElderThingKindDefString = "Alien_ElderThing_Race_Standard";
        
        public static bool IsElderThing(this Pawn pawn)
        {
            return pawn?.def?.defName == ElderThingKindDefString;
        }
    }
}