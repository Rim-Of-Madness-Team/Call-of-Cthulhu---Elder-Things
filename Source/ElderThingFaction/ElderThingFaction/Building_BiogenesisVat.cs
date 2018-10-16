using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ElderThingFaction
{
    public class Building_BiogenesisVat : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
    {
        public CompPowerTrader powerComp;

        private CompRefuelable refuelableComp;

        private CompBreakdownable breakdownableComp;

        private List<IntVec3> cachedAdjCellsCardinal;

        private ThingDef consoleDef;

        public Building_BiogenesisVat()
        {
            this.billStack = new BillStack(this);
        }
        

        public override IntVec3 InteractionCell
        {
            get
            {
                Building adjacentConsole = AdjacentConsole();
                if ((adjacentConsole) != null)
                {
                    return adjacentConsole.InteractionCell;
                }
                return base.InteractionCell;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (AdjacentConsole() == null)
            {
                //Messages.Message("Cannot control the biogenesis process without a console.", MessageTypeDefOf.RejectInput); //MessageSound.RejectInput);
                this.SetForbidden(true, false);
                return;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool blabla)
        {
            base.SpawnSetup(map, blabla);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.refuelableComp = base.GetComp<CompRefuelable>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.consoleDef = DefDatabase<ThingDef>.GetNamed("ET_Console");
        }

        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                List<IntVec3> result = null;
                if (this.cachedAdjCellsCardinal == null)
                {
                    var cellsAdjacentCardinal = new List<IntVec3>(GenAdj.CellsAdjacentCardinal(this));
                    if (cellsAdjacentCardinal?.Count > 0)
                    {
                        result = new List<IntVec3>();
                        foreach (var cell in cellsAdjacentCardinal)
                        {
                            if (cell.InBounds(base.Map))
                                result.Add(cell);
                        }

                        cachedAdjCellsCardinal = result;
                    }
                }
                return cachedAdjCellsCardinal;
            }
        }

        public Building AdjacentConsole()
        {
            for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
            {
                IntVec3 c = this.AdjCellsCardinalInBounds[i];
                Building edifice = c.GetEdifice(base.Map);
                if (edifice != null && edifice.def == consoleDef)
                {
                    return (Building)edifice;
                }
            }
            return null;
        }

        public Building AdjacentReachableConsole(Pawn reacher)
        {
            for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
            {
                IntVec3 c = this.AdjCellsCardinalInBounds[i];
                Building edifice = c.GetEdifice(base.Map);
                if (edifice != null && edifice.def == consoleDef && reacher.CanReach(edifice, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    return (Building)edifice;
                }
            }
            return null;
        }
        
    }
}