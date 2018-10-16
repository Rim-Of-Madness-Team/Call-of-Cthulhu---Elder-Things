using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class ScenPart_StartingSlave : ScenPart
    {
        private int count = 1;
    
        private string countBuf;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.count, "count", 0, false);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            base.DoEditInterface(listing);
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
            Widgets.TextFieldNumeric<int>(scenPartRect, ref this.count, ref this.countBuf, 1f, 10f);
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen, "PlayerStartsWith", ScenPart_StartingThing_Defined.PlayerStartWithIntro);
        }

        [DebuggerHidden]
        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag == "PlayerStartsWith")
            {
                yield return "Slave".Translate() + " x" + this.count;
            }
            yield break;
        }

        //public override void Randomize()
        //{
        //    if (Rand.Value < 0.5f)
        //    {
        //        this.pawnKind = null;
        //    }
        //    else
        //    {
        //        this.pawnKind = this.PossibleAnimals().RandomElement<PawnKindDef>();
        //    }
        //    this.count = ScenPart_StartingSlave.PetCountChances.RandomElementByWeight((Pair<int, float> pa) => pa.Second).First;
        //    this.bondToRandomPlayerPawnChance = 0f;
        //}

        public override bool TryMerge(ScenPart other)
        {
            ScenPart_StartingSlave ScenPart_StartingSlave = other as ScenPart_StartingSlave;
            if (ScenPart_StartingSlave != null)
            {
                this.count += ScenPart_StartingSlave.count;
                return true;
            }
            return false;
        }

        public override IEnumerable<Thing> PlayerStartingThings()
        {
            for (int i = 0; i < this.count; i++)
            {
                Pawn newSlave = ElderThingFaction.ScenPart_StartingSlave.NewGeneratedStartingSlave();
                Find.GameInitData.startingAndOptionalPawns.Add(newSlave);
                yield return newSlave;
            }
            yield break;
        }
        
        // Verse.StartingSlaveUtility
        public static Pawn NewGeneratedStartingSlave()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(
                PawnKindDefOf.Slave, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, true, false, false, false, true,
                false, 26f, false, true, true, false, false, false, false, (Pawn X) => !X.story.WorkTagIsDisabled(WorkTags.Hauling));
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }



        //[DebuggerHidden]
        //public override IEnumerable<Thing> PlayerStartingThings()
        //{
        //    for (int i = 0; i < this.count; i++)
        //    {
        //        PawnKindDef kindDef;
        //        if (this.pawnKind != null)
        //        {
        //            kindDef = this.pawnKind;
        //        }
        //        else
        //        {
        //            kindDef = this.RandomPets().RandomElementByWeight((PawnKindDef td) => td.RaceProps.petness);
        //        }
        //        Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfPlayer);
        //        if (pawn.Name == null || pawn.Name.Numerical)
        //        {
        //            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, null);
        //        }
        //        if (Rand.Value < this.bondToRandomPlayerPawnChance)
        //        {
        //            Pawn pawn2 = Find.GameInitData.StartingSlaves.RandomElement<Pawn>();
        //            if (!pawn2.story.traits.HasTrait(TraitDefOf.Psychopath))
        //            {
        //                pawn2.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn);
        //            }
        //        }
        //        yield return pawn;
        //    }
        //    yield break;
        //}
    }
}
