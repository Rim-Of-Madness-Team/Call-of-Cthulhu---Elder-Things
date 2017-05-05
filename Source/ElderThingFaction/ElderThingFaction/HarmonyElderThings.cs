using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    [StaticConstructorOnStartup]
    public static class HarmonyElderThings
    {
        static HarmonyElderThings()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.cthulhu.elderthings");
            harmony.Patch(AccessTools.Method(typeof(StorytellerUtility), "DefaultParmsNow"), null, new HarmonyMethod(typeof(HarmonyElderThings).GetMethod("DefaultParmsNow_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(TileFinder), "RandomFactionBaseTileFor"), new HarmonyMethod(typeof(HarmonyElderThings).GetMethod("RandomFactionBaseTileFor_PreFix")), null);
        }


        public static bool RandomFactionBaseTileFor_PreFix(ref int __result, Faction faction)
        {
            if (faction.def.defName == "ElderThing_Faction")
            {
                __result = RandomFactionBaseTileFor_ElderThings(faction);
                return false;
            }
            return true;
        }

        public static int RandomFactionBaseTileFor_ElderThings(Faction faction)
        {
            for (int i = 0; i < 150; i++)
            {
                int num;
                if (Find.WorldGrid.TileIndices.TryRandomElementByWeight(delegate (int x)
                {
                    Tile tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }
                    List<int> neighbors = new List<int>();
                    Find.WorldGrid.GetTileNeighbors(x, neighbors);
                    //Log.Message("Neighbors " + neighbors.Count.ToString());
                    foreach (int y in neighbors)
                    {
                        Tile tile2 = Find.WorldGrid[y];
                        if (tile2.hilliness == Hilliness.Mountainous &&
                            tile2.biome == BiomeDefOf.IceSheet)
                            return 1000f;
                    }
                    return tile.biome.factionBaseSelectionWeight;
                }, out num))
                {
                    if (Find.FactionManager.FactionAtTile(num) == null)
                    {
                        return num;
                    }
                }
            }
            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        // RimWorld.StorytellerUtility
        public static void DefaultParmsNow_PostFix(ref IncidentParms __result, StorytellerDef tellerDef, IncidentCategory incCat, IIncidentTarget target)
        {
            Map map = target as Map;
            if (map != null)
            {
                if (__result.points > 0)
                {
                    try
                    {
                        int numElderThings = map.mapPawns.FreeColonistsSpawned.ToList().FindAll(p => p.def.defName == "Alien_ElderThing_Race_Standard").Count;
                        __result.points += numElderThings * 80;
                    }
                    catch (NullReferenceException)
                    { }
                }
            }
        }
    }
}