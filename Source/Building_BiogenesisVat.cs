using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ElderThingFaction
{
    public class Building_BiogenesisVat : Building_WorkTable
    {
        private CompBreakdownable breakdownableComp;

        private List<IntVec3> cachedAdjCellsCardinal;

        private ThingDef consoleDef;
        public CompPowerTrader powerComp;

        private CompRefuelable refuelableComp;

        public Building_BiogenesisVat()
        {
            billStack = new BillStack(this);
        }


        public override IntVec3 InteractionCell
        {
            get
            {
                var adjacentConsole = AdjacentConsole();
                if (adjacentConsole != null)
                {
                    return adjacentConsole.InteractionCell;
                }

                return base.InteractionCell;
            }
        }

        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (cachedAdjCellsCardinal != null)
                {
                    return cachedAdjCellsCardinal;
                }

                var cellsAdjacentCardinal = new List<IntVec3>(GenAdj.CellsAdjacentCardinal(this));
                if (!(cellsAdjacentCardinal.Count > 0))
                {
                    return cachedAdjCellsCardinal;
                }

                var result = new List<IntVec3>();
                foreach (var cell in cellsAdjacentCardinal)
                {
                    if (cell.InBounds(Map))
                    {
                        result.Add(cell);
                    }
                }

                cachedAdjCellsCardinal = result;

                return cachedAdjCellsCardinal;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (AdjacentConsole() != null)
            {
                return;
            }

            //Messages.Message("Cannot control the biogenesis process without a console.", MessageTypeDefOf.RejectInput); //MessageSound.RejectInput);
            this.SetForbidden(true, false);
        }

        public override void SpawnSetup(Map map, bool blabla)
        {
            base.SpawnSetup(map, blabla);
            powerComp = GetComp<CompPowerTrader>();
            refuelableComp = GetComp<CompRefuelable>();
            breakdownableComp = GetComp<CompBreakdownable>();
            powerComp = GetComp<CompPowerTrader>();
            consoleDef = DefDatabase<ThingDef>.GetNamed("ET_Console");
        }

        public Building AdjacentConsole()
        {
            foreach (var c in AdjCellsCardinalInBounds)
            {
                var edifice = c.GetEdifice(Map);
                if (edifice != null && edifice.def == consoleDef)
                {
                    return edifice;
                }
            }

            return null;
        }

        public Building AdjacentReachableConsole(Pawn reacher)
        {
            foreach (var c in AdjCellsCardinalInBounds)
            {
                var edifice = c.GetEdifice(Map);
                if (edifice != null && edifice.def == consoleDef &&
                    reacher.CanReach(edifice, PathEndMode.Touch, Danger.Deadly))
                {
                    return edifice;
                }
            }

            return null;
        }
    }
}