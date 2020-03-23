using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace ElderThingFaction
{
    [StaticConstructorOnStartup]
    public static class HarmonyElderThings
    {
        public static bool DEBUGMODE = false;

        static HarmonyElderThings()
        {
            var h = new Harmony("rimworld.jecrell.cthulhu.elderthings");


            DebugMessage("HAR :: -===- ENTER Harmony Test -===- ");


            //Elder Things Weapons need to fire at five targets when equipped
            Patch(h, typeof(Projectile).GetMethods(bindingAttr: AccessTools.all).First(
                        mi => mi.Name == "Launch" && mi.GetParameters().ElementAt(1).ParameterType == typeof(Vector3)),
                    new HarmonyMethod(typeof(HarmonyElderThings), nameof(Launch_Prefix)), null);

            //Elder Things add to the difficulty of a colony
            Patch(h, AccessTools.Method(typeof(StorytellerUtility), "DefaultParmsNow"), null,
                    new HarmonyMethod(typeof(HarmonyElderThings), nameof(DefaultParmsNow_PostFix)));

            //Elder things have colonies that spawn in the colder parts of the world
            Patch(h, AccessTools.Method(typeof(TileFinder), "RandomSettlementTileFor"),
                    new HarmonyMethod(typeof(HarmonyElderThings), nameof(RandomSettlementTileFor_PreFix)), null);
            
            //Elder Things do not wear apparel
            Patch(h, AccessTools.Method(typeof(PawnApparelGenerator),
                        nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                    new HarmonyMethod(typeof(HarmonyElderThings), nameof(GenerateStartingApparelFor_PreFix)), null);

            //Elder Things have no hair.
            Patch(h, AccessTools.Method(typeof(PawnHairChooser),
                        nameof(PawnHairChooser.RandomHairDefFor)),
                    new HarmonyMethod(typeof(HarmonyElderThings), nameof(RandomHairDefFor_PreFix)), null);



            DebugMessage("HAR :: -===- EXIT Harmony Test -===- ");
        }

        public static void Patch(Harmony instance, MethodBase original, HarmonyMethod preFix = null, HarmonyMethod postFix = null, HarmonyMethod transpiler = null, HarmonyMethod finalizer = null)
        {
            string patchType;
            if (preFix != null)
                patchType = "PreFix";
            else if (postFix != null)
                patchType = "PostFix";
            else if (transpiler != null)
                patchType = "Transpiler";
            else
                patchType = "Finalizer";

            try
            {
                instance.Patch(original, preFix, postFix, transpiler, finalizer);
                DebugMessage("HAR :: " + original.ToString() + " " + patchType + " " + " -- PASS");
            }
            catch (Exception e)
            {
                DebugMessage("HAR :: " + original.ToString() + " " + patchType + " " + " -- FAIL \n\n "+ e.Message);
            }
        }

        public static void DebugMessage(string s)
        {
            if (DEBUGMODE)
                Log.Message(s);
        }

        private static bool firingAllLasers = false;
        //Projectile
        public static bool Launch_Prefix(Projectile __instance, Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment, ThingDef targetCoverDef)
        {
            DebugMessage("14a :: -- Enter Launch_Prefix --");
            if (firingAllLasers)
            {
                DebugMessage("14a :: firingAllLasers was True");
                DebugMessage("14a :: -- Exit Launch_Prefix --");
                return true;
            }

            if (launcher != null && launcher is Pawn p && p.def.defName == "Alien_ElderThing_Race_Standard")
            {
               DebugMessage("14a :: Launcher is Elder Thing");
                if (equipment != null)
                {
                    DebugMessage("14a :: Equipment is not null");
                    if (equipment.def.defName == "ElderThing_LaserGun")
                    {
                        DebugMessage("14a :: Equipped LaserGun");
                        if (p.MapHeld is Map m)
                        {
                            DebugMessage("14a :: Map is valid");
                            Dictionary<Pawn, ShootLine> chosenTargets = new Dictionary<Pawn, ShootLine>();

                            var potentialTargets =
                                from target in m.mapPawns.AllPawnsSpawned
                                where 
                                    target.Faction != null &&
                                    target.Faction.HostileTo(p.Faction) &&
                                    new LocalTargetInfo(target) != usedTarget
                                select target;

                            if (potentialTargets != null && potentialTargets.Count() > 0)
                            {
                               DebugMessage($"14a :: Potential targets: {potentialTargets.Count()}");
                                firingAllLasers = true;
                                DebugMessage("14a :: firingAllLasers Set: True");
                                foreach (var target in potentialTargets)
                                {
                                    var verb = p.equipment.PrimaryEq.PrimaryVerb;
                                    ShootLine resultingLine;
                                    bool flag = verb.TryFindShootLineFromTo(p.Position, target, out resultingLine);
                                    if (verb.verbProps.stopBurstWithoutLos && !flag)
                                    {
                                        DebugMessage($"14a :: Target {target.ToString()} out of sight/range.");
                                        continue;
                                    }
                                    DebugMessage($"14a :: Target {target.ToString()} added to target list.");
                                    chosenTargets.Add(target, resultingLine);
                                }

                                for (int shotsFired = 0; shotsFired < chosenTargets.Count() && shotsFired < 4; shotsFired++)
                                {
                                    var currentTarget = chosenTargets.ElementAt(shotsFired).Key;
                                    var currentLine = chosenTargets.ElementAt(shotsFired).Value;
                                    DebugMessage($"14a :: Fired {shotsFired+1} at {currentTarget.ToString()}");
                                    Projectile projectile2 = (Projectile)GenSpawn.Spawn(__instance.def, currentLine.Source, p.Map);
                                    projectile2.Launch(launcher, currentTarget, currentTarget,
                                        hitFlags, equipment);
                                }
                                firingAllLasers = false;
                                DebugMessage("14a :: firingAllLasers Set: False");
                            }
                        }
                    }
                }

                DebugMessage("14a :: -- Exit Launch_Prefix --");
            }
            return true;
        }

        //PawnHairChooser.RandomHairDefFor
        public static bool RandomHairDefFor_PreFix(Pawn pawn, FactionDef factionType, ref HairDef __result)
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

            if (faction != null)
            {
                if (faction.def != null)
                {
                    if (faction.def.defName == "ElderThing_Faction")
                    {
                        DebugMessage("217 :: Elder Thing Faction found");
                        __result = RandomSettlementTileFor_ElderThings(faction);
                        DebugMessage($"217 :: Elder Thing Faction Tile Set: {__result.ToString()}");
                        DebugMessage("217 :: -- Exit RandomSettlementTileFor_PreFix ((False)) --");
                        return false;
                    }
                }
            }
            DebugMessage("217 :: -- Exit RandomSettlementTileFor_PreFix ((True)) --");
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

        //RimWorld.StorytellerUtility
        public static void DefaultParmsNow_PostFix(IncidentCategoryDef incCat, IIncidentTarget target, ref IncidentParms __result)
        {
            Map map = (Map)__result.target;
            if (map != null)
            {
                if (__result.points > 0)
                {
                    try
                    {
                        int numElderThings = map?.mapPawns?.FreeColonistsSpawned?.ToList()?.FindAll(p => p.def.defName == "Alien_ElderThing_Race_Standard")?.Count ?? 0;
                        __result.points += numElderThings * 50;
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
        }
    }
}