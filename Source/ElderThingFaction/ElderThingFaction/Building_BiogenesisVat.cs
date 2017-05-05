using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ElderThingFaction
{
    public class Building_BiogenesisVat : Building_WorkTable
    {
        public CompPowerTrader powerComp;

        private CompRefuelable refuelableComp;

        private CompBreakdownable breakdownableComp;

        private List<IntVec3> cachedAdjCellsCardinal;

        private ThingDef consoleDef;

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

        public override bool UsableNow
        {
            get
            {
                if (AdjacentConsole() == null)
                {
                    Messages.Message("Cannot control the biogenesis process without a console.", MessageSound.RejectInput);
                    return false;
                }
                return base.UsableNow;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.refuelableComp = base.GetComp<CompRefuelable>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.consoleDef = DefDatabase<ThingDef>.GetNamed("ET_Console");
        }

        public bool CanSpawnNow
        {
            get
            {
                return this.powerComp.PowerOn;
            }
        }

        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (this.cachedAdjCellsCardinal == null)
                {
                    this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
                                                   where c.InBounds(base.Map)
                                                   select c).ToList<IntVec3>();
                }
                return this.cachedAdjCellsCardinal;
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