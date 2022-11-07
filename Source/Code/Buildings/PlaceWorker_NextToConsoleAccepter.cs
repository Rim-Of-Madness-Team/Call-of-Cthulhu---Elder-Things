using RimWorld;
using Verse;

namespace ElderThingFaction
{
    public class PlaceWorker_NextToConsoleAccepter : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
            for (var i = 0; i < 4; i++)
            {
                var c = loc + GenAdj.CardinalDirections[i] + GenAdj.CardinalDirections[i];
                if (!c.InBounds(map: map))
                {
                    continue;
                }

                var thingList = c.GetThingList(map: map);
                foreach (var groundThing in thingList)
                {
                    if (!(GenConstruct.BuiltDefOf(def: groundThing.def) is ThingDef thingDef) || thingDef.building == null)
                    {
                        continue;
                    }

                    if (thingDef.defName == "ET_BiogenesisVat")
                    {
                        return true;
                    }
                }
            }

            return "MustPlaceNextToVatAccepter".Translate();
        }
    }
}