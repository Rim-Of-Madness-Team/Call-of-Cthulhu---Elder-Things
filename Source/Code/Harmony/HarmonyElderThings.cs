using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            var harmony = new Harmony(id: "rimworld.jecrell.cthulhu.elderthings");


            DebugMessage(s: "HAR :: -===- ENTER Harmony Test -===- ");


            //Elder Things Weapons need to fire at five targets when equipped
            Patch(instance: harmony, original: typeof(Projectile).GetMethods(bindingAttr: AccessTools.all).First(
                    predicate: mi =>
                        mi.Name == "Launch" && mi.GetParameters().ElementAt(index: 1).ParameterType == typeof(Vector3)),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings), methodName: nameof(Launch_Prefix)));

            //Elder Things add to the difficulty of a colony
            Patch(instance: harmony,
                original: AccessTools.Method(type: typeof(StorytellerUtility),
                    name: "DefaultParmsNow"),
                preFix: null,
                postFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(DefaultParmsNow_PostFix)));

            //Elder things have colonies that spawn in the colder parts of the world
            Patch(instance: harmony,
                original: AccessTools.Method(type: typeof(TileFinder), name: "RandomSettlementTileFor"),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(RandomSettlementTileFor_PreFix)));

            //Elder Things do not wear apparel
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(PawnApparelGenerator),
                    name: nameof(PawnApparelGenerator.GenerateStartingApparelFor)),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(GenerateStartingApparelFor_PreFix)));

            //Elder Things have no hair.
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(PawnStyleItemChooser),
                    name: nameof(PawnStyleItemChooser.RandomHairFor)),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(RandomHairDefFor_PreFix)));

            //Elder Things have no hair, seriously
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(HumanlikeMeshPoolUtility),
                    name: nameof(HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn)),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(GetHumanlikeHairSetForPawn_PreFix)));
            
            //Elder Things have Ideology support for their Life Stages
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(Pawn_IdeoTracker),
                    name: "get_CertaintyChangeFactor"),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(get_CertaintyChangeFactor_PreFix)));
            
            //Elder Things have crazy long lives -- so we need to make RimWorld accomodate for it
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(PawnGenerator),
                    name: "GenerateRandomAge"),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(GenerateRandomAge_PreFix)),
                 transpiler: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                     methodName: nameof(GenerateRandomAgeTranspiler)));
            
                        
            //Elder Thing pods don't have baby blankets
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(PawnRenderer),
                    name: "SwaddleBaby"),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(SwaddleBaby_PreFix)));
         
            //Elder things receive Elder Thing biology genetics
            Patch(instance: harmony, original: AccessTools.Method(type: typeof(PawnGenerator),
                    name: "GenerateGenes"),
                preFix: new HarmonyMethod(methodType: typeof(HarmonyElderThings),
                    methodName: nameof(GenerateGenes_PreFix)));

            DebugMessage(s: "HAR :: -===- EXIT Harmony Test -===- ");
        }

        public static bool GenerateGenes_PreFix(Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn.IsElderThing())
            {
                pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("ET_SporeReproduction"), false);
                pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("ET_LeatheryWings"), false);
                pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("ET_BarrelBody"), false);
                pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("ET_FiveFoldSymmetry"), false);
                pawn.genes.AddGene(DefDatabase<GeneDef>.GetNamed("ET_TopAppendage"), false);
                return false;
            }
            return true;
        }

        //PawnRenderer.SwaddleBaby
        private static bool SwaddleBaby_PreFix(Pawn ___pawn, PawnRenderer __instance, Vector3 shellLoc, Rot4 facing, Quaternion quat, PawnRenderFlags flags)
        {
            return !___pawn.IsElderThing();
        }
        
        public static Pawn generateRandomAgePawn;

        public static void GenerateRandomAge_PreFix(Pawn pawn) => 
            generateRandomAgePawn = pawn;
        
        public static int GetRandomAgeAttempts()
        {
            if (generateRandomAgePawn.IsElderThing())
            {
                return 9000; //9,000 is ~3x the Elder Thing age range;
            } 
            return 0; //300 (default) is ~3x human age range
        }
        
        //Elder Things transpiler for number of attempts to generate an age (because they are very old....)
        public static IEnumerable<CodeInstruction> GenerateRandomAgeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.OperandIs(300) && instruction.opcode == OpCodes.Ldc_I4)
                {
                    yield return CodeInstruction.Call(typeof(HarmonyElderThings), nameof(GetRandomAgeAttempts));
                    yield return new CodeInstruction(OpCodes.Add);
                }
            }
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
                instance.Patch(original: original, prefix: preFix, postfix: postFix, transpiler: transpiler, finalizer: finalizer);
                DebugMessage(s: "HAR :: " + original + " " + patchType + " " + " -- PASS");
            }
            catch (Exception e)
            {
                DebugMessage(s: "HAR :: " + original + " " + patchType + " " + " -- FAIL \n\n " + e.Message);
            }
        }

        public static void DebugMessage(string s)
        {
            if (DEBUGMODE)
            {
                Log.Message(text: s);
            }
        }

        //Projectile
        public static bool Launch_Prefix(Projectile __instance, Thing launcher, Vector3 origin,
            LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, Thing equipment,
            ThingDef targetCoverDef)
        {
            DebugMessage(s: "14a :: -- Enter Launch_Prefix --");
            if (firingAllLasers)
            {
                DebugMessage(s: "14a :: firingAllLasers was True");
                DebugMessage(s: "14a :: -- Exit Launch_Prefix --");
                return true;
            }

            if (launcher == null || launcher is not Pawn p || p.def.defName != "Alien_ElderThing_Race_Standard")
            {
                return true;
            }

            DebugMessage(s: "14a :: Launcher is Elder Thing");
            if (equipment != null)
            {
                DebugMessage(s: "14a :: Equipment is not null");
                if (equipment.def.defName == "ElderThing_LaserGun")
                {
                    DebugMessage(s: "14a :: Equipped LaserGun");
                    if (p.MapHeld is { } m)
                    {
                        DebugMessage(s: "14a :: Map is valid");
                        var chosenTargets = new Dictionary<Pawn, ShootLine>();

                        var potentialTargets =
                            from target in m.mapPawns.AllPawnsSpawned
                            where
                                target.Faction != null &&
                                target.Faction.HostileTo(other: p.Faction) &&
                                new LocalTargetInfo(thing: target) != usedTarget
                            select target;

                        if (potentialTargets.Any())
                        {
                            DebugMessage(s: $"14a :: Potential targets: {potentialTargets.Count()}");
                            firingAllLasers = true;
                            DebugMessage(s: "14a :: firingAllLasers Set: True");
                            foreach (var target in potentialTargets)
                            {
                                var resultingLine = new ShootLine();
                                var verb = p.equipment.PrimaryEq.PrimaryVerb;
                                if (verb.verbProps.stopBurstWithoutLos &&
                                    !verb.TryFindShootLineFromTo(root: p.Position, targ: target, resultingLine: out resultingLine))
                                {
                                    DebugMessage(s: $"14a :: Target {target} out of sight/range.");
                                    continue;
                                }

                                DebugMessage(s: $"14a :: Target {target} added to target list.");
                                chosenTargets.Add(key: target, value: resultingLine);
                            }

                            for (var shotsFired = 0;
                                shotsFired < chosenTargets.Count && shotsFired < 4;
                                shotsFired++)
                            {
                                var currentTarget = chosenTargets.ElementAt(index: shotsFired).Key;
                                var currentLine = chosenTargets.ElementAt(index: shotsFired).Value;
                                DebugMessage(s: $"14a :: Fired {shotsFired + 1} at {currentTarget}");
                                var projectile2 =
                                    (Projectile) GenSpawn.Spawn(def: __instance.def, loc: currentLine.Source, map: p.Map);
                                projectile2.Launch(launcher: launcher, usedTarget: currentTarget, intendedTarget: currentTarget,
                                    hitFlags: hitFlags, preventFriendlyFire: false, equipment: equipment);
                            }

                            firingAllLasers = false;
                            DebugMessage(s: "14a :: firingAllLasers Set: False");
                        }
                    }
                }
            }

            DebugMessage(s: "14a :: -- Exit Launch_Prefix --");

            return true;
        }

        
        // //PawnGenerator.GenerateRandomAge
        // public static bool GenerateRandomAge_PreFix(Pawn pawn, PawnGenerationRequest request)
        // {
        //     DebugMessage(s: "993 :: -- Enter GenerateRandomAge_PreFix --");
        //     if (ElderThingUtility.IsElderThing(pawn: pawn))
        //     {
        //         if ((request.AllowedDevelopmentalStages & DevelopmentalStage.Newborn) != 0)
        //         {
        //             pawn.ageTracker.AgeBiologicalTicks = 0L;
        //             pawn.babyNamingDeadline = Find.TickManager.TicksGame + 60000;
        //         }
        //         else if (request.FixedBiologicalAge.HasValue)
        //         {
        //             pawn.ageTracker.AgeBiologicalTicks = 
        //         }
        //             
        //         DebugMessage(s: "993 :: -- Exit GenerateRandomAge_PreFix ((False)) --");
        //         return false;
        //     }
        //
        //     DebugMessage(s: "993 :: -- Exit GenerateRandomAge_PreFix ((True)) --");
        //     return true;
        // }
        
        //Pawn_IdeoTracker
        public static bool get_CertaintyChangeFactor_PreFix(ref SimpleCurve ___curve, Pawn ___pawn, Pawn_IdeoTracker __instance, ref float __result)
        {
            DebugMessage(s: "446 :: -- Enter get_CertaintyChangeFactor_PreFix --");
            if (ModsConfig.BiotechActive && ___pawn.IsElderThing())
            {
                if (___curve == null)
                {
                    ___curve = new SimpleCurve
                    {
                        {
                            new CurvePoint(___pawn.ageTracker.LifeStageMinAge(
                                DefDatabase<LifeStageDef>.GetNamed("ElderThingJuvenile")), 2f),
                            true
                        },
                        {
                            new CurvePoint(___pawn.ageTracker.LifeStageMinAge(
                                DefDatabase<LifeStageDef>.GetNamed("ElderThingFullyFormed")), 1f),
                            true
                        }
                    };
                }
                __result = ___curve.Evaluate(___pawn.ageTracker
                    .AgeBiologicalYearsFloat);
                
                DebugMessage(s: "446 :: -- Exit get_CertaintyChangeFactor_PreFix ((False)) --");
                return false;
            }
            DebugMessage(s: "446 :: -- Exit get_CertaintyChangeFactor_PreFix ((True)) --");
            return true;
        }
        
        //HumanlikeMeshPoolUtility
        public static bool GetHumanlikeHairSetForPawn_PreFix(Pawn pawn, ref GraphicMeshSet __result)
        {
            DebugMessage(s: "77 :: -- Enter GetHumanlikeHairSetForPawn_PreFix --");
            if (pawn.IsElderThing())
            {
                __result = MeshPool.GetMeshSetForWidth(1,1);
                DebugMessage(s: "77 :: -- Exit GetHumanlikeHairSetForPawn_PreFix ((False)) --");
                return false;
            }
            DebugMessage(s: "77 :: -- Exit GetHumanlikeHairSetForPawn_PreFix ((True)) --");
            return true;
        }
        
        //PawnHairChooser.RandomHairDefFor
        public static bool RandomHairDefFor_PreFix(Pawn pawn, ref HairDef __result)
        {
            DebugMessage(s: "876 :: -- Enter RandomHairDefFor_Prefix --");
            if (pawn.IsElderThing())
            {
                __result = DefDatabase<HairDef>.GetNamedSilentFail(defName: "Shaved");
                DebugMessage(s: "876 :: -- Exit RandomHairDefFor_Prefix ((False)) --");
                return false;
            }

            DebugMessage(s: "876 :: -- Exit RandomHairDefFor_Prefix ((True)) --");
            return true;
        }


        //PawnApparelGenerator.GenerateStartingApparelFor
        public static bool GenerateStartingApparelFor_PreFix(Pawn pawn, PawnGenerationRequest request)
        {
            DebugMessage(s: "24f :: -- Enter GenerateStartingApparelFor_PreFix --");

            if (pawn.def.defName == "Alien_ElderThing_Race_Standard")
            {
                //Log.Message("Elder Thing Detected: Apparel not generated");
                DebugMessage(s: "24f :: -- Exit GenerateStartingApparelFor_PreFix ((False)) --");
                return false;
            }

            DebugMessage(s: "24f :: -- Exit GenerateStartingApparelFor_PreFix ((True)) --");
            return true;
        }

        //TileFinder.RandomSettlementTileFor
        public static bool RandomSettlementTileFor_PreFix(ref int __result, Faction faction)
        {
            DebugMessage(s: "217 :: -- Enter RandomSettlementTileFor_PreFix --");

            if (faction is {def: {defName: "ElderThing_Faction"}})
            {
                DebugMessage(s: "217 :: Elder Thing Faction found");
                __result = RandomSettlementTileFor_ElderThings(faction: faction);
                DebugMessage(s: $"217 :: Elder Thing Faction Tile Set: {__result}");
                DebugMessage(s: "217 :: -- Exit RandomSettlementTileFor_PreFix ((False)) --");
                return false;
            }

            DebugMessage(s: "217 :: -- Exit RandomSettlementTileFor_PreFix ((True)) --");
            return true;
        }

        public static int RandomSettlementTileFor_ElderThings(Faction faction, bool mustBeAutoChoosable = false)
        {
            for (var i = 0; i < 500; i++)
            {
                if (!(from _ in Enumerable.Range(start: 0, count: 100)
                    select Rand.Range(min: 0, max: Find.WorldGrid.TilesCount)).TryRandomElementByWeight(weightSelector: delegate(int x)
                {
                    var tile = Find.WorldGrid[tileID: x];
                    if (!tile.biome.canBuildBase || tile.hilliness == Hilliness.Impassable)
                    {
                        return 0f;
                    }

                    var neighbors = new List<int>();
                    Find.WorldGrid.GetTileNeighbors(tileID: x, outNeighbors: neighbors);
                    //Log.Message("Neighbors " + neighbors.Count.ToString());
                    if (neighbors.Count <= 0)
                    {
                        return tile.biome.settlementSelectionWeight;
                    }

                    foreach (var y in neighbors)
                    {
                        var tile2 = Find.WorldGrid[tileID: y];
                        if (tile2.hilliness == Hilliness.Mountainous &&
                            tile2.biome == BiomeDefOf.IceSheet)
                        {
                            return 1000f;
                        }
                    }

                    return tile.biome.settlementSelectionWeight;
                }, result: out var num))
                {
                    continue;
                }

                if (TileFinder.IsValidTileForNewSettlement(tile: num))
                {
                    return num;
                }
            }

            Log.Error(text: "Failed to find faction base tile for " + faction);
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
                    .FindAll(match: p => p.def.defName == "Alien_ElderThing_Race_Standard")
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