using Verse;

namespace ElderThingFaction
{
    public class ElderThingData : IExposable
    {
        private Pawn pawn;
        public bool elderThingPowersInitialized = false;
        
        public ElderThingData()
        {

        }

        public ElderThingData(CompElderThing newUser)
        {
            this.pawn = newUser.AbilityUser;
        }
                
        public void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.pawn, "elderThingDataPawn");
            Scribe_Values.Look<bool>(ref elderThingPowersInitialized, "elderThingPowersInitialized", false);
        }
    }
}