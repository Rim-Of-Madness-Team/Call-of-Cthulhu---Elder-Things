using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class CompElderThing : CompAbilityUser
    {
        private ElderThingData elderThingData;
        private bool elderThingPowersInitialized;

        //We had a previous error where the Elder Things
        // mod accidentally gave powers to all characters.
        //This redundant check ensures that Elder Things'
        // abilities are removed.
        private bool elderThingPowersInitializedRedundant;

        public bool IsElderThing => ElderThingUtility.IsElderThing(Pawn);

        public ElderThingData ElderThingData
        {
            get
            {
                if (elderThingData == null && IsElderThing)
                {
                    elderThingData = new ElderThingData(this);
                }

                return elderThingData;
            }
        }

        public override bool TryTransformPawn()
        {
            return IsElderThing;
        }

        public override void CompTick()
        {
            if (AbilityUser == null)
            {
                return;
            }

            if (!AbilityUser.Spawned)
            {
                return;
            }

            if (elderThingPowersInitializedRedundant == false)
            {
                elderThingPowersInitializedRedundant = true;
                CleanupUnusedAbilities();
            }

            if (Find.TickManager.TicksGame <= 200 || !IsElderThing)
            {
                return;
            }

            if (elderThingPowersInitialized == false)
            {
                PostInitializeTick();
            }

            foreach (var power in AbilityData.AllPowers)
            {
                if (power.CooldownTicksLeft > -1)
                {
                    power.CooldownTicksLeft--;
                }
            }
        }

        public void PostInitializeTick()
        {
            if (AbilityUser == null)
            {
                return;
            }

            if (!AbilityUser.Spawned)
            {
                return;
            }

            if (AbilityUser.story == null)
            {
                return;
            }

            elderThingPowersInitialized = true;
            Initialize();
            // if (ForceData.Alignment == 0.0f) ForceData.Alignment = 0.5f;
            ResolvePowers();
        }


        private void CleanupUnusedAbilities()
        {
            if (AbilityUser == null)
            {
                return;
            }

            if (!AbilityUser.Spawned)
            {
                return;
            }

            if (AbilityUser.story == null)
            {
                return;
            }

            if (IsElderThing)
            {
                return;
            }

            RemovePawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_ShortFlight"));
            RemovePawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_PsionicBlast"));
        }


        private void ResolvePowers()
        {
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_ShortFlight"));
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_PsionicBlast"));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref elderThingPowersInitialized, "elderThingPowersInitialized");
            Scribe_Values.Look(ref elderThingPowersInitializedRedundant, "elderThingPowersInitializedRedundant");
            Scribe_Deep.Look(ref elderThingData, "elderThingData", this);
        }
    }
}