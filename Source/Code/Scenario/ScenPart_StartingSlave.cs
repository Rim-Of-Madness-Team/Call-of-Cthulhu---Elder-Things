using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
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
            Scribe_Values.Look(value: ref count, label: "count");
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            base.DoEditInterface(listing: listing);
            var scenPartRect = listing.GetScenPartRect(part: this, height: RowHeight);
            Widgets.TextFieldNumeric(rect: scenPartRect, val: ref count, buffer: ref countBuf, min: 1f, max: 10f);
        }

        public override string Summary(Scenario scen)
        {
            return ScenSummaryList.SummaryWithList(scen: scen, tag: "PlayerStartsWith",
                intro: ScenPart_StartingThing_Defined.PlayerStartWithIntro);
        }

        [DebuggerHidden]
        public override IEnumerable<string> GetSummaryListEntries(string tag)
        {
            if (tag != "PlayerStartsWith")
            {
                yield break;
            }

            yield return "Slave".Translate() + " x" + count;
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
            if (!(other is ScenPart_StartingSlave ScenPart_StartingSlave))
            {
                return false;
            }

            count += ScenPart_StartingSlave.count;
            return true;
        }

        public override IEnumerable<Thing> PlayerStartingThings()
        {
            for (var i = 0; i < count; i++)
            {
                var newSlave = NewGeneratedStartingSlave();
                Find.GameInitData.startingAndOptionalPawns.Add(item: newSlave);
                yield return newSlave;
            }
        }

        // Verse.StartingSlaveUtility
        public static Pawn NewGeneratedStartingSlave()
        {
            var request = new PawnGenerationRequest(
                kind: PawnKindDefOf.Slave, faction: Faction.OfPlayer, context: PawnGenerationContext.PlayerStarter, tile: -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: false, mustBeCapableOfViolence: true,
                colonistRelationChanceFactor: 20f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true, allowAddictions: false, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, biocodeWeaponChance: 0, biocodeApparelChance: 0, extraPawnForExtraRelationChance: null, relationWithExtraPawnChanceFactor: 1,
                validatorPreGear: X => !X.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Hauling));
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request: request);
            }
            catch (Exception arg)
            {
                Log.Error(
                    text: "There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " +
                          arg);
                pawn = PawnGenerator.GeneratePawn(request: request);
            }

            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn: pawn);
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