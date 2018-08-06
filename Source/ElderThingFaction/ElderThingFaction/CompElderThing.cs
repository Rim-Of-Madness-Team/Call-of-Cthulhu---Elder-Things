using System;
using System.Runtime.InteropServices;
using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class CompElderThing : CompAbilityUser
    {
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
            //if (IsElderThing)
            //{

            if (AbilityUser != null)
            {
                if (AbilityUser.Spawned)
                {
                    if (Find.TickManager.TicksGame > 200)
                    {
                        if (ElderThingData?.elderThingPowersInitialized == false)
                        {
                            PostInitializeTick();
                        }
                    }   
                }
            }


            //}
        }

        public void PostInitializeTick()
        {
            if (this.AbilityUser != null)
            {
                if (this.AbilityUser.Spawned)
                {
                    if (this.AbilityUser.story != null)
                    {
                        this.ElderThingData.elderThingPowersInitialized = true;
                        this.Initialize();
                        // if (ForceData.Alignment == 0.0f) ForceData.Alignment = 0.5f;
                        this.ResolvePowers();
                    }
                }
            }
        }

        private void ResolvePowers()
        {
            Log.Message("Powers resolved");
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_ShortFlight"));
            AddPawnAbility(DefDatabase<AbilityDef>.GetNamed("ElderThing_PsionicBlast"));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<ElderThingData>(ref this.elderThingData, "elderThingData", new object[] {this});
        }
    }
}