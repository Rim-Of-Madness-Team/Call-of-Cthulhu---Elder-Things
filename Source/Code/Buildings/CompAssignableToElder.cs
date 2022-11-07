using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ElderThingFaction
{
    internal class CompAssignableToElder : CompAssignableToPawn_Bed
    {
        public override AcceptanceReport CanAssignTo(Pawn pawn)
        {
            if (!pawn.IsElderThing())
            {
                return "ET_ETBedsForETs".Translate();
            }
            return base.CanAssignTo(pawn);
        }

        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!parent.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }

                if (!parent.def.building.bed_humanlike)
                {
                    return from p in parent.Map.mapPawns.SpawnedColonyAnimals
                        orderby CanAssignTo(pawn: p).Accepted descending
                        select p;
                }

                if (parent.def.defName.StartsWith(value: "ET_"))
                {
                    return from Pawn pawn in parent.Map.mapPawns.FreeColonists
                        where pawn.kindDef.defName.StartsWith(value: "ElderThing_") && 
                              (pawn.DevelopmentalStage & DevelopmentalStage.Baby) == 0 &&
                              (pawn.DevelopmentalStage & DevelopmentalStage.Newborn) == 0
                        orderby CanAssignTo(pawn: pawn).Accepted && !IdeoligionForbids(pawn: pawn) descending
                        select pawn;
                }

                return from p in parent.Map.mapPawns.FreeColonists
                    where !p.kindDef.defName.StartsWith(value: "ElderThing_") && 
                          (p.DevelopmentalStage & DevelopmentalStage.Baby) == 0 &&
                          (p.DevelopmentalStage & DevelopmentalStage.Newborn) == 0
                    orderby CanAssignTo(pawn: p).Accepted && !IdeoligionForbids(pawn: p) descending
                    select p;
            }
        }
    }
}