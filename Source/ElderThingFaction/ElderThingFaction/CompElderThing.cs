using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class CompElderThing : CompAbilityUser
    {
        private bool elderThingPowersInitialized = false;


        private ElderThingData elderThingData = null;

        public bool IsElderThing => ElderThingUtility.IsElderThing(this.Pawn);

        public ElderThingData ElderThingData
        {
            get
            {
                if (this.elderThingData == null && this.IsElderThing)
                {
                    this.elderThingData = new ElderThingData(this);
                }

                return this.elderThingData;
            }
        }

        public override bool TryTransformPawn() => IsElderThing;

        public override void CompTick()
        {
            if (AbilityUser != null)
            {
                if (AbilityUser.Spawned)
                {
                    if (Find.TickManager.TicksGame > 200)
                    {
                        if (elderThingPowersInitialized == false)
                        {
                            PostInitializeTick();
                        }
                            foreach (var power in this.AbilityData.AllPowers)
                            {
                                if (power.CooldownTicksLeft > -1)
                                {
                                    power.CooldownTicksLeft--;
                                }
                            }
                    }
                }
            }
        }

        public void PostInitializeTick()
        {
            if (this.AbilityUser != null)
            {
                if (this.AbilityUser.Spawned)
                {
                    if (this.AbilityUser.story != null)
                    {
                        elderThingPowersInitialized = true;
                        this.Initialize();
                        // if (ForceData.Alignment == 0.0f) ForceData.Alignment = 0.5f;
                        this.ResolvePowers();
                    }
                }
            }
        }

        private void ResolvePowers()
        {
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_ShortFlight"));
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_PsionicBlast"));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.elderThingPowersInitialized, "elderThingPowersInitialized", false);
            Scribe_Deep.Look<ElderThingData>(ref this.elderThingData, "elderThingData", new object[] { this });
        }
    }
}