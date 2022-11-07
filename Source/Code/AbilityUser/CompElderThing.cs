using AbilityUser;
using RimWorld;
using Verse;
using AbilityDef = AbilityUser.AbilityDef;

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

        public bool IsElderThing => Pawn.IsElderThing();

        public ElderThingData ElderThingData
        {
            get
            {
                if (elderThingData == null && IsElderThing)
                {
                    elderThingData = new ElderThingData(newUser: this);
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
            if (Pawn is not { Spawned: true })
                return;

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
        }

        public void PostInitializeTick()
        {
            if (Pawn == null)
            {
                return;
            }

            if (!Pawn.Spawned)
            {
                return;
            }

            if (Pawn.story == null)
            {
                return;
            }

            elderThingPowersInitialized = true;
            Initialize();
            ResolvePowers();

        }

        public void SporesReplicating()
        {
            if (Pawn.DevelopmentalStage == DevelopmentalStage.Adult)
            {
                //Give the Elder Thing a Hediff to prevent it from spawning spores immediately
                Pawn.health.AddHediff(HediffDef.Named("ROMET_SporesReplicating"));
            }
        }
        
        private void CleanupUnusedAbilities()
        {
            if (Pawn == null)
            {
                return;
            }

            if (!Pawn.Spawned)
            {
                return;
            }

            if (Pawn.story == null)
            {
                return;
            }

            if (IsElderThing)
            {
                return;
            }

            RemovePawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_ShortFlight"));
            RemovePawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_PsionicBlast"));
            RemovePawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_SporeReproduction"));
        }


        private void ResolvePowers()
        {
            AddPawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_ShortFlight"));
            AddPawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_PsionicBlast"));
            AddPawnAbility(abilityDef: DefDatabase<AbilityDef>.GetNamed(defName: "ElderThing_SporeReproduction"));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(value: ref elderThingPowersInitialized, label: "elderThingPowersInitialized");
            Scribe_Values.Look(value: ref elderThingPowersInitializedRedundant, label: "elderThingPowersInitializedRedundant");
            Scribe_Deep.Look(target: ref elderThingData, label: "elderThingData", this);
        }
    }
}