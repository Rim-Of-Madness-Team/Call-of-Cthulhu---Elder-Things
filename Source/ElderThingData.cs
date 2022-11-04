using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class ElderThingData : AbilityData
    {
        public bool elderThingPowersInitialized;
        private Pawn pawn;

        public ElderThingData()
        {
        }


        public ElderThingData(CompElderThing newUser)
        {
            pawn = newUser.AbilityUser;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "elderThingDataPawn");
            Scribe_Values.Look(ref elderThingPowersInitialized, "elderThingPowersInitialized");
        }
    }
}