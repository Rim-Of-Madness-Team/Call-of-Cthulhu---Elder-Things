using System;
using RimWorld;
using Verse;

namespace ElderThingFaction
{
    public class ScenPart_ConfigPage_ConfigureStartingSlaves : ScenPart_ConfigPage
    {
        private const int MaxPawnCount = 10;

        private string pawnCountBuffer;

        public int slaveCount = 1;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            base.DoEditInterface(listing: listing);
            var scenPartRect = listing.GetScenPartRect(part: this, height: RowHeight);
            Widgets.TextFieldNumeric(rect: scenPartRect, val: ref slaveCount, buffer: ref pawnCountBuffer, min: 1f, max: 10f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref slaveCount, label: "slaveCount");
        }

        public override string Summary(Scenario scen)
        {
            return "ScenPart_StartWithSlaves".Translate(arg1: slaveCount);
        }

        public override void Randomize()
        {
            slaveCount = Rand.RangeInclusive(min: 1, max: 6);
        }

        public override void PostWorldGenerate()
        {
            var num = 0;
            while (true)
            {
                //StartingPawnUtility.ClearAllStartingPawns();
                for (var i = 0; i < slaveCount; i++)
                {
                    Find.GameInitData.startingAndOptionalPawns.Add(item: NewGeneratedStartingSlave());
                }

                num++;
                if (num > 20)
                {
                    break;
                }

                if (StartingPawnUtility.WorkTypeRequirementsSatisfied())
                {
                    return;
                }
            }
        }

        // Verse.StartingPawnUtility
        public static Pawn NewGeneratedStartingSlave()
        {
            var request = new PawnGenerationRequest(kind: PawnKindDefOf.Slave, faction: Faction.OfPlayer,
                context: PawnGenerationContext.PlayerStarter, tile: -1, forceGenerateNewPawn: true, allowDead: false, 
                allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: true, colonistRelationChanceFactor: 20f, 
                forceAddFreeWarmLayerIfNeeded: false);
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
    }
}