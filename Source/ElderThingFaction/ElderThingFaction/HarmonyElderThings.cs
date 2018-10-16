using Harmony;
using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Linq;

namespace ElderThingFaction
{
    [StaticConstructorOnStartup]
    public static class HarmonyElderThings
    {
        static HarmonyElderThings()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.cthulhu.elderthings");
            harmony.Patch(AccessTools.Method(typeof(StorytellerUtility), "DefaultParmsNow"), null,
                new HarmonyMethod(typeof(HarmonyElderThings).GetMethod("DefaultParmsNow_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                new HarmonyMethod(typeof(HarmonyElderThings).GetMethod("RandomSettlementTileFor_PreFix")), null);

            harmony.Patch(
                AccessTools.Method(typeof(PawnApparelGenerator),
                    nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                new HarmonyMethod(typeof(HarmonyElderThings).GetMethod("GenerateStartingApparelFor_PreFix")), null);
            
            harmony.Patch(
                AccessTools.Method(typeof(PawnHairChooser),
                    nameof(PawnHairChooser.RandomHairDefFor)),
                new HarmonyMethod(typeof(HarmonyElderThings).GetMethod(nameof(RandomHairDefFor))), null);
        }


        public static bool RandomHairDefFor(Pawn pawn, FactionDef factionType, ref HairDef __result)
        {
            if (ElderThingUtility.IsElderThing(pawn))
            {
                __result = DefDatabase<HairDef>.GetNamedSilentFail("Shaved");
                return false;   
            }
            return true;
        }

        //PawnApparelGenerator.
        public static bool GenerateStartingApparelFor_PreFix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.def.defName == "Alien_ElderThing_Race_Standard")
            {
                Log.Message("Elder Thing Detected: Apparel not generated");
                return false;
            }
            return true;
        }

        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            if (faction != null)
            {
                if (faction.def != null)
                {
                    if (faction.def.defName == "ElderThing_Faction")
                    {
                        __result = RandomSettlementTileFor_ElderThings(faction);
                        return false;
                    }
                }
            }
            return true;
        }

        public static int RandomSettlementTileFor_ElderThings(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (int i = 0; i < 500; i++)
            {
                int num;
                if ((from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    List<int> neighbors = new List<int>();
                    Find.WorldGrid.GetTileNeighbors(x, neighbors);
                    //Log.Message("Neighbors " + neighbors.Count.ToString());
                    if (neighbors != null && neighbors.Count > 0)
                    {
                        foreach (int y in neighbors)
                        {
                            Tile tile2 = Find.WorldGrid[y];
                            if (tile2.hilliness == Hilliness.Mountainous &&
                                tile2.biome == BiomeDefOf.IceSheet)
                                return 1000f;
                        }
                    }

                    return tile.biome.settlementSelectionWeight;
                }, out num))
                {
                    if (TileFinder.IsValidTileForNewSettlement(num, null))
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        // RimWorld.StorytellerUtility
//        public static void DefaultParmsNow_PostFix(ref IncidentParms __result, StorytellerDef tellerDef, IncidentCategory incCat, IIncidentTarget target)
//        {
//            Map map = (Map)parms.target;
//            if (map != null)
//            {
//                if (__result.points > 0)
//                {
//                    try
//                    {
//                        int numElderThings = map.mapPawns.FreeColonistsSpawned.ToList().FindAll(p => p.def.defName == "Alien_ElderThing_Race_Standard").Count;
//                        __result.points += numElderThings * 80;
//                    }
//                    catch (NullReferenceException)
//                    { }
//                }
//            }
//        }
    }
}