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
            pawn = newUser.Pawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look(refee: ref pawn, label: "elderThingDataPawn");
            Scribe_Values.Look(value: ref elderThingPowersInitialized, label: "elderThingPowersInitialized");
        }
    }
}