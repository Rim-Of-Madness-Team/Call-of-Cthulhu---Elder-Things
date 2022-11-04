using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ElderThingFaction
{
    internal class CompAssignableToElder : CompAssignableToPawn_Bed
    {
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
                        orderby CanAssignTo(p).Accepted descending
                        select p;
                }

                if (parent.def.defName.StartsWith("ET_"))
                {
                    return from Pawn pawn in parent.Map.mapPawns.FreeColonists
                        where pawn.kindDef.defName.StartsWith("ElderThing_")
                        orderby CanAssignTo(pawn).Accepted && !IdeoligionForbids(pawn) descending
                        select pawn;
                }

                return from p in parent.Map.mapPawns.FreeColonists
                    where !p.kindDef.defName.StartsWith("ElderThing_")
                    orderby CanAssignTo(p).Accepted && !IdeoligionForbids(p) descending
                    select p;
            }
        }
    }
}