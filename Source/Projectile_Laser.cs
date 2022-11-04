using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ElderThingFaction
{
    public class Projectile_Laser : Projectile
    {
        public bool canStartFire;
        public float drawingIntensity;
        public Matrix4x4 drawingMatrix;
        public Vector3 drawingPosition;
        public Vector3 drawingScale;
        public Material drawingTexture;

        public Thing hitThing;
        public int postFiringDuration;
        public float postFiringFinalIntensity;
        public float postFiringInitialIntensity;

        public Material postFiringTexture;
        public int preFiringDuration;

        public float preFiringFinalIntensity;

        // Custom XML variables.
        public float preFiringInitialIntensity;

        // Draw variables.
        public Material preFiringTexture;

        public float startFireChance;

        // Variables.
        public int tickCounter;

        public override void SpawnSetup(Map map, bool blabla)
        {
            base.SpawnSetup(map, blabla);
            drawingTexture = def.DrawMatSingle;
        }

        /// <summary>
        ///     Get parameters from XML.
        /// </summary>
        public void GetParametersFromXml()
        {
            if (!(def is ThingDef_LaserProjectile additionalParameters))
            {
                return;
            }

            preFiringDuration = additionalParameters.preFiringDuration;
            postFiringDuration = additionalParameters.postFiringDuration;

            // Draw.
            preFiringInitialIntensity = additionalParameters.preFiringInitialIntensity;
            preFiringFinalIntensity = additionalParameters.preFiringFinalIntensity;
            postFiringInitialIntensity = additionalParameters.postFiringInitialIntensity;
            postFiringFinalIntensity = additionalParameters.postFiringFinalIntensity;
            startFireChance = additionalParameters.StartFireChance;
            canStartFire = additionalParameters.CanStartFire;
        }

        /// <summary>
        ///     Save/load data from a savegame file (apparently not used for projectile for now).
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter");

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            GetParametersFromXml();
        }

        /// <summary>
        ///     Main projectile sequence.
        /// </summary>
        public override void Tick()
        {
            //  Log.Message("Tickng Ma Lazor");
            // Directly call the Projectile base Tick function (we want to completely override the Projectile Tick() function).
            //((ThingWithComponents)this).Tick(); // Does not work...
            try
            {
                if (tickCounter == 0)
                {
                    GetParametersFromXml();
                    PerformPreFiringTreatment();
                }

                // Pre firing.
                if (tickCounter < preFiringDuration)
                {
                    GetPreFiringDrawingParameters();
                }
                // Firing.
                else if (tickCounter == preFiringDuration)
                {
                    Fire();
                    GetPostFiringDrawingParameters();
                }
                // Post firing.
                else
                {
                    GetPostFiringDrawingParameters();
                }

                if (tickCounter == preFiringDuration + postFiringDuration && !Destroyed)
                {
                    Destroy();
                }

                if (launcher is Pawn launcherPawn)
                {
                    if (launcherPawn.Dead && !Destroyed)
                    {
                        Destroy();
                    }
                }

                tickCounter++;
            }
            catch
            {
                Destroy();
            }
        }

        /// <summary>
        ///     Performs prefiring treatment: data initalization.
        /// </summary>
        public virtual void PerformPreFiringTreatment()
        {
            DetermineImpactExactPosition();
            var cannonMouthOffset = (destination - origin).normalized * 0.9f;
            drawingScale = new Vector3(1f, 1f,
                (destination - origin).magnitude - cannonMouthOffset.magnitude);
            drawingPosition = origin + (cannonMouthOffset / 2) + ((destination - origin) / 2) +
                              (Vector3.up * def.Altitude);
            drawingMatrix.SetTRS(drawingPosition, ExactRotation, drawingScale);
        }

        /// <summary>
        ///     Gets the prefiring drawing parameters.
        /// </summary>
        public virtual void GetPreFiringDrawingParameters()
        {
            if (preFiringDuration == 0)
            {
                return;
            }

            drawingIntensity = preFiringInitialIntensity + ((preFiringFinalIntensity - preFiringInitialIntensity) *
                tickCounter / preFiringDuration);
        }

        /// <summary>
        ///     Gets the postfiring drawing parameters.
        /// </summary>
        public virtual void GetPostFiringDrawingParameters()
        {
            if (postFiringDuration == 0)
            {
                return;
            }

            drawingIntensity = postFiringInitialIntensity +
                               ((postFiringFinalIntensity - postFiringInitialIntensity) *
                                ((tickCounter - (float) preFiringDuration) / postFiringDuration));
        }

        /// <summary>
        ///     Checks for colateral targets (cover, neutral animal, pawn) along the trajectory.
        /// </summary>
        protected void DetermineImpactExactPosition()
        {
            // We split the trajectory into small segments of approximatively 1 cell size.
            var trajectory = destination - origin;
            var numberOfSegments = (int) trajectory.magnitude;
            var trajectorySegment = trajectory / trajectory.magnitude;

            var
                temporaryDestination = origin; // Last valid tested position in case of an out of boundaries shot.
            var exactTestedPosition = origin;

            for (var segmentIndex = 1; segmentIndex <= numberOfSegments; segmentIndex++)
            {
                exactTestedPosition += trajectorySegment;
                var testedPosition = exactTestedPosition.ToIntVec3();

                if (!exactTestedPosition.InBounds(Map))
                {
                    destination = temporaryDestination;
                    break;
                }

                if (!def.projectile.flyOverhead && segmentIndex >= 5)
                {
                    var list = Map.thingGrid.ThingsListAt(Position);
                    foreach (var current in list)
                    {
                        // Check impact on a wall.
                        if (current.def.Fillage == FillCategory.Full)
                        {
                            destination = testedPosition.ToVector3Shifted() +
                                          new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                            hitThing = current;
                            break;
                        }

                        // Check impact on a pawn.
                        if (current.def.category != ThingCategory.Pawn)
                        {
                            continue;
                        }

                        var pawn = current as Pawn;
                        var chanceToHitCollateralTarget = 0.45f;
                        if (pawn is {Downed: true})
                        {
                            chanceToHitCollateralTarget *= 0.1f;
                        }

                        var targetDistanceFromShooter = (ExactPosition - origin).MagnitudeHorizontal();
                        if (targetDistanceFromShooter < 4f)
                        {
                            chanceToHitCollateralTarget *= 0f;
                        }
                        else
                        {
                            if (targetDistanceFromShooter < 7f)
                            {
                                chanceToHitCollateralTarget *= 0.5f;
                            }
                            else
                            {
                                if (targetDistanceFromShooter < 10f)
                                {
                                    chanceToHitCollateralTarget *= 0.75f;
                                }
                            }
                        }

                        if (pawn == null)
                        {
                            continue;
                        }

                        chanceToHitCollateralTarget *= pawn.RaceProps.baseBodySize;

                        if (!(Rand.Value < chanceToHitCollateralTarget))
                        {
                            continue;
                        }

                        destination = testedPosition.ToVector3Shifted() +
                                      new Vector3(Rand.Range(-0.3f, 0.3f), 0f, Rand.Range(-0.3f, 0.3f));
                        hitThing = pawn;
                        break;
                    }
                }

                temporaryDestination = exactTestedPosition;
            }
        }

        /// <summary>
        ///     Manages the projectile damage application.
        /// </summary>
        public virtual void Fire()
        {
            ApplyDamage(hitThing);
        }

        /// <summary>
        ///     Applies damage on a collateral pawn or an object.
        /// </summary>
        protected void ApplyDamage(Thing hitThing)
        {
            if (hitThing != null)
            {
                // Impact collateral target.
                Impact(hitThing);
            }
            else
            {
                ImpactSomething();
            }
        }

        /// <summary>
        ///     Computes what should be impacted in the DestinationCell.
        /// </summary>
        protected void ImpactSomething()
        {
            // Check impact on a thick mountain.
            if (def.projectile.flyOverhead)
            {
                var roofDef = Map.roofGrid.RoofAt(DestinationCell);
                if (roofDef is {isThickRoof: true})
                {
                    var info = SoundInfo.InMap(new TargetInfo(DestinationCell, Map));
                    def.projectile.soundHitThickRoof.PlayOneShot(info);
                    return;
                }
            }

            // Impact the initial targeted pawn.
            if (usedTarget != null)
            {
                if (usedTarget.Thing is Pawn {Downed: true} && (origin - destination).magnitude > 5f &&
                    Rand.Value < 0.2f)
                {
                    Impact(null);
                    return;
                }

                Impact(usedTarget.Thing);
            }
            else
            {
                // Impact a pawn in the destination cell if present.
                var thing = Map.thingGrid.ThingAt(DestinationCell, ThingCategory.Pawn);
                if (thing != null)
                {
                    Impact(thing);
                    return;
                }

                // Impact any cover object.
                foreach (var current in Map.thingGrid.ThingsAt(DestinationCell))
                {
                    if (!(current.def.fillPercent > 0f) && current.def.passability == Traversability.Standable)
                    {
                        continue;
                    }

                    Impact(current);
                    return;
                }

                Impact(null);
            }
        }

        /// <summary>
        ///     Impacts a pawn/object or the ground.
        /// </summary>
        protected override void Impact(Thing hitThing)
        {
            if (hitThing != null)
            {
                var unused = Map;
                var battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(launcher,
                    hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
                Find.BattleLog.Add(battleLogEntry_RangedImpact);
                var damageAmountBase = def.projectile.GetDamageAmount(launcher);
                var damageDef = def.projectile.damageDef;
                var y = ExactRotation.eulerAngles.y;
                var instigator = launcher;
                var weaponDef = equipmentDef;
                var dinfo = new DamageInfo(damageDef, damageAmountBase, ArmorPenetration, y, instigator, null,
                    weaponDef);
                hitThing.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_RangedImpact);

                //                else
                //                {
                //                    SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(base.Position, map, false));
                //                    MoteMaker.MakeStaticMote(this.ExactPosition, map, ThingDefOf.Mote_ShotHit_Dirt, 1f);
                //                    if (base.Position.GetTerrain(map).takeSplashes)
                //                    {
                //                        MoteMaker.MakeWaterSplash(this.ExactPosition, map, Mathf.Sqrt((float)this.def.projectile.GetDamageAmount(this.launcher) * 1f, 4f);
                //                    }
                //                }
                //int damageAmountBase = this.def.projectile.DamageAmount;
                //DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
                //hitThing.TakeDamage(dinfo);
                //hitThing.TakeDamage(dinfo);
                if (canStartFire && Rand.Range(0f, 1f) > startFireChance)
                {
                    hitThing.TryAttachFire(0.05f);
                }

                if (hitThing is not Pawn pawn)
                {
                    return;
                }

                PostImpactEffects(instigator as Pawn, pawn);
                FleckMaker.ThrowMicroSparks(destination, Map);
                FleckMaker.Static(destination, Map, FleckDefOf.ShotHit_Dirt);
            }
            else
            {
                var info = SoundInfo.InMap(new TargetInfo(Position, Map));
                SoundDefOf.BulletImpact_Ground.PlayOneShot(info);
                FleckMaker.Static(ExactPosition, Map, FleckDefOf.ShotHit_Dirt);
                FleckMaker.ThrowMicroSparks(ExactPosition, Map);
            }
        }

        /// <summary>
        ///     JECRELL:: Added this to make derived classes work easily.
        /// </summary>
        /// <param name="launcher"></param>
        /// <param name="hitTarget"></param>
        public virtual void PostImpactEffects(Pawn launcher, Pawn hitTarget)
        {
        }

        /// <summary>
        ///     Draws the laser ray.
        /// </summary>
        public override void Draw()
        {
            Comps_PostDraw();
            Graphics.DrawMesh(MeshPool.plane10, drawingMatrix,
                FadedMaterialPool.FadedVersionOf(drawingTexture, drawingIntensity), 0);
        }
    }
}