using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class ScenPart_ConfigPage_ConfigureStartingSlaves : ScenPart_ConfigPage
    {
        private const int MaxPawnCount = 10;

        public int slaveCount = 1;

        private string pawnCountBuffer;

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            base.DoEditInterface(listing);
            Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
            Widgets.TextFieldNumeric<int>(scenPartRect, ref this.slaveCount, ref this.pawnCountBuffer, 1f, 10f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.slaveCount, "slaveCount", 0, false);
        }

        public override string Summary(Scenario scen)
        {
            return "ScenPart_StartWithSlaves".Translate(new object[]
            {
                this.slaveCount
            });
        }

        public override void Randomize()
        {
            this.slaveCount = Rand.RangeInclusive(1, 6);
        }

        public override void PostWorldLoad()
        {
            int num = 0;
            while (true)
            {
                //StartingPawnUtility.ClearAllStartingPawns();
                for (int i = 0; i < this.slaveCount; i++)
                {
                    Find.GameInitData.startingPawns.Add(ElderThingFaction.ScenPart_ConfigPage_ConfigureStartingSlaves.NewGeneratedStartingSlave());
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
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, null, true, false, false, false, true, false, 26f, false, true, true, null, null, null, null, null, null);
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
    }




}
