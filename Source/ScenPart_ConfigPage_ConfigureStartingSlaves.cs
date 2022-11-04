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
            base.DoEditInterface(listing);
            var scenPartRect = listing.GetScenPartRect(this, RowHeight);
            Widgets.TextFieldNumeric(scenPartRect, ref slaveCount, ref pawnCountBuffer, 1f, 10f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref slaveCount, "slaveCount");
        }

        public override string Summary(Scenario scen)
        {
            return "ScenPart_StartWithSlaves".Translate(slaveCount);
        }

        public override void Randomize()
        {
            slaveCount = Rand.RangeInclusive(1, 6);
        }

        public override void PostWorldGenerate()
        {
            var num = 0;
            while (true)
            {
                //StartingPawnUtility.ClearAllStartingPawns();
                for (var i = 0; i < slaveCount; i++)
                {
                    Find.GameInitData.startingAndOptionalPawns.Add(NewGeneratedStartingSlave());
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
            var request = new PawnGenerationRequest(PawnKindDefOf.Slave, Faction.OfPlayer,
                PawnGenerationContext.PlayerStarter, -1, true, false, false, false, true, false, 26f);
            Pawn pawn;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error(
                    "There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " +
                    arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }

            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }
    }
}