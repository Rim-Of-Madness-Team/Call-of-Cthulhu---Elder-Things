using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ElderThingFaction
{
    public class PlaceWorker_NextToConsoleAccepter : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            for (int i = 0; i < 4; i++)
            {
                IntVec3 c = loc + GenAdj.CardinalDirections[i] + GenAdj.CardinalDirections[i];
                if (c.InBounds(map))
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing = thingList[j];
                        ThingDef thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef;
                        if (thingDef != null && thingDef.building != null)
                        {
                            if (thingDef.defName == "ET_BiogenesisVat") return true;
                        }
                    }
                }
            }
            return "MustPlaceNextToVatAccepter".Translate();
        }
    }
}