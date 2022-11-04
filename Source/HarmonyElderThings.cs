using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    [StaticConstructorOnStartup]
    public static class HarmonyElderThings
    {
        public static bool DEBUGMODE = false;

        private static bool firingAllLasers;

        static HarmonyElderThings()
        {
            var harmony = new Harmony("rimworld.jecrell.cthulhu.elderthings");


            DebugMessage("HAR :: -===- ENTER Harmony Test -===- ");


            //Elder Things Weapons need to fire at five targets when equipped
            Patch(harmony, typeof(Projectile).GetMethods(AccessTools.all).First(
                    mi => mi.Name == "Launch" && mi.GetParameters().ElementAt(1).ParameterType == typeof(Vector3)),
                new HarmonyMethod(typeof(HarmonyElderThings), nameof(Launch_Prefix)));

            //Elder Things add to the difficulty of a colony
            Patch(harmony, AccessTools.Method(typeof(StorytellerUtility), "DefaultParmsNow"), null,
                new HarmonyMethod(typeof(HarmonyElderThings), nameof(DefaultParmsNow_PostFix)));

            //Elder things have colonies that spawn in the colder parts of the world
            Patch(harmony, AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                new HarmonyMethod(typeof(HarmonyElderThings), nameof(RandomSettlementTileFor_PreFix)));

            //Elder Things do not wear apparel
            Patch(harmony, AccessTools.Method(typeof(PawnApparelGenerator),
                    nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                new HarmonyMethod(typeof(HarmonyElderThings), nameof(GenerateStartingApparelFor_PreFix)));

            //Elder Things have no hair.
            Patch(harmony, AccessTools.Method(typeof(PawnStyleItemChooser),
                    nameof(PawnStyleItemChooser.RandomHairFor)),
                new HarmonyMethod(typeof(HarmonyElderThings), nameof(RandomHairDefFor_PreFix)));

            DebugMessage("HAR :: -===- EXIT Harmony Test -===- ");
        }

        public static void Patch(Harmony instance, MethodBase original, HarmonyMethod preFix = null,
            HarmonyMethod postFix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null)
        {
            string patchType;
            if (preFix != null)
            {
                patchType = "PreFix";
            }
            else if (postFix != null)
            {
                patchType = "PostFix";
            }
            else if (transpiler != null)
            {
                patchType = "Transpiler";
            }
            else
            {
                patchType = "Finalizer";
            }

            try
            {
                instance.Patch(original, preFix, postFix, transpiler, finalizer);
                DebugMessage("HAR :: " + original + " " + patchType + " " + " -- PASS");
            }
            catch (Exception e)
            {
                DebugMessage("HAR :: " + original + " " + patchType + " " + " -- FAIL \n\n " + e.Message);
            }
        }

        public static void DebugMessage(string s)
        {
            if (DEBUGMODE)
            {
                Log.Message(s);
            }
        }

        //Projectile
        public static bool Launch_Prefix(Projectile __instance, Thing launcher, Vector3 origin,
            LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment,
            ThingDef targetCoverDef)
        {
            DebugMessage("14a :: -- Enter Launch_Prefix --");
            if (firingAllLasers)
            {
                DebugMessage("14a :: firingAllLasers was True");
                DebugMessage("14a :: -- Exit Launch_Prefix --");
                return true;
            }

            if (launcher == null || launcher is not Pawn p || p.def.defName != "Alien_ElderThing_Race_Standard")
            {
                return true;
            }

            DebugMessage("14a :: Launcher is Elder Thing");
            if (equipment != null)
            {
                DebugMessage("14a :: Equipment is not null");
                if (equipment.def.defName == "ElderThing_LaserGun")
                {
                    DebugMessage("14a :: Equipped LaserGun");
                    if (p.MapHeld is { } m)
                    {
                        DebugMessage("14a :: Map is valid");
                        var chosenTargets = new Dictionary<Pawn, ShootLine>();

                        var potentialTargets =
                            from target in m.mapPawns.AllPawnsSpawned
                            where
                                target.Faction != null &&
                                target.Faction.HostileTo(p.Faction) &&
                                new LocalTargetInfo(target) != usedTarget
                            select target;

                        if (potentialTargets.Any())
                        {
                            DebugMessage($"14a :: Potential targets: {potentialTargets.Count()}");
                            firingAllLasers = true;
                            DebugMessage("14a :: firingAllLasers Set: True");
                            foreach (var target in potentialTargets)
                            {
                                var resultingLine = new ShootLine();
                                var verb = p.equipment.PrimaryEq.PrimaryVerb;
                                if (verb.verbProps.stopBurstWithoutLos &&
                                    !verb.TryFindShootLineFromTo(p.Position, target, out resultingLine))
                                {
                                    DebugMessage($"14a :: Target {target} out of sight/range.");
                                    continue;
                                }

                                DebugMessage($"14a :: Target {target} added to target list.");
                                chosenTargets.Add(target, resultingLine);
                            }

                            for (var shotsFired = 0;
                                shotsFired < chosenTargets.Count && shotsFired < 4;
                                shotsFired++)
                            {
                                var currentTarget = chosenTargets.ElementAt(shotsFired).Key;
                                var currentLine = chosenTargets.ElementAt(shotsFired).Value;
                                DebugMessage($"14a :: Fired {shotsFired + 1} at {currentTarget}");
                                var projectile2 =
                                    (Projectile) GenSpawn.Spawn(__instance.def, currentLine.Source, p.Map);
                                projectile2.Launch(launcher, currentTarget, currentTarget,
                                    hitFlags, false, equipment);
                            }

                            firingAllLasers = false;
                            DebugMessage("14a :: firingAllLasers Set: False");
                        }
                    }
                }
            }

            DebugMessage("14a :: -- Exit Launch_Prefix --");

            return true;
        }

        //PawnHairChooser.RandomHairDefFor
        public static bool RandomHairDefFor_PreFix(Pawn pawn, ref HairDef __result)
        {
            DebugMessage("876 :: -- Enter RandomHairDefFor_Prefix --");
            if (ElderThingUtility.IsElderThing(pawn))
            {
                __result = DefDatabase<HairDef>.GetNamedSilentFail("Shaved");
                DebugMessage("876 :: -- Exit RandomHairDefFor_Prefix ((False)) --");
                return false;
            }

            DebugMessage("876 :: -- Exit RandomHairDefFor_Prefix ((True)) --");
            return true;
        }


        //PawnApparelGenerator.GenerateStartingApparelFor
        public static bool GenerateStartingApparelFor_PreFix(Pawn pawn, PawnGenerationRequest request)
        {
            DebugMessage("24f :: -- Enter GenerateStartingApparelFor_PreFix --");

            if (pawn.def.defName == "Alien_ElderThing_Race_Standard")
            {
                //Log.Message("Elder Thing Detected: Apparel not generated");
                DebugMessage("24f :: -- Exit GenerateStartingApparelFor_PreFix ((False)) --");
                return false;
            }

            DebugMessage("24f :: -- Exit GenerateStartingApparelFor_PreFix ((True)) --");
            return true;
        }

        //TileFinder.RandomSettlementTileFor
        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            DebugMessage("217 :: -- Enter RandomSettlementTileFor_PreFix --");

            if (faction is {def: {defName: "ElderThing_Faction"}})
            {
                DebugMessage("217 :: Elder Thing Faction found");
                __result = RandomSettlementTileFor_ElderThings(faction);
                DebugMessage($"217 :: Elder Thing Faction Tile Set: {__result}");
                DebugMessage("217 :: -- Exit RandomSettlementTileFor_PreFix ((False)) --");
                return false;
            }

            DebugMessage("217 :: -- Exit RandomSettlementTileFor_PreFix ((True)) --");
            return true;
        }

        public static int RandomSettlementTileFor_ElderThings(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (var i = 0; i < 500; i++)
            {
                if (!(from _ in Enumerable.Range(0, 100)
                    select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight(delegate(int x)
                {
                    var tile = Find.WorldGrid[x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }

                    var neighbors = new List<int>();
                    Find.WorldGrid.GetTileNeighbors(x, neighbors);
                    //Log.Message("Neighbors " + neighbors.Count.ToString());
                    if (neighbors.Count <= 0)
                    {
                        return tile.biome.settlementSelectionWeight;
                    }

                    foreach (var y in neighbors)
                    {
                        var tile2 = Find.WorldGrid[y];
                        if (tile2.hilliness == Hilliness.Mountainous &&
                            tile2.biome == BiomeDefOf.IceSheet)
                        {
                            return 1000f;
                        }
                    }

                    return tile.biome.settlementSelectionWeight;
                }, out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(num))
                {
                    return num;
                }
            }

            Log.Error("Failed to find faction base tile for " + faction);
            return 0;
        }

        //RimWorld.StorytellerUtility
        public static void DefaultParmsNow_PostFix(IncidentCategoryDef incCat, IIncidentTarget target,
            ref IncidentParms __result)
        {
            try
            {
                var map = (Map) __result.target;
                if (map == null)
                {
                    return;
                }

                if (__result.points <= 0)
                {
                    return;
                }

                var numElderThings = map.mapPawns?.FreeColonistsSpawned?.ToList()
                    .FindAll(p => p.def.defName == "Alien_ElderThing_Race_Standard")
                    .Count ?? 0;
                __result.points += numElderThings * 50;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}