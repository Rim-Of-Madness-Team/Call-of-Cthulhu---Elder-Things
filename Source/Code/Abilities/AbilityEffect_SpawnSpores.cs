
using System.Text;
using AbilityUser;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class AbilityEffect_SpawnSpores : Verb_UseAbility
    {
        public override bool Available()
        {
            return CasterPawn.DevelopmentalStage == DevelopmentalStage.Adult &&
                   !CasterPawn.health.hediffSet.HasHediff(def: HediffDef.Named(defName: "ROMET_SporesReplicating")) &&
                    base.Available();
        }

        protected override bool TryCastShot()
        {
            base.TryCastShot();
            Ability.CooldownTicksLeft = Ability.MaxCastingTicks;
            if (TargetsAoE[index: 0] is var t && t.Cell != default)
            {
                var position = CasterPawn.Position;
                Plant_ElderThingSporePod pod = (Plant_ElderThingSporePod)GenSpawn.Spawn(def: ThingDef.Named(defName: "ET_PlantElderThingSporePod"),
                    loc: t.Cell,
                    map: CasterPawn.Map);
                pod.Parent = CasterPawn;

                var cellsToHit = DamageDefOf.Flame.Worker.ExplosionCellsToHit(
                    t.Cell, CasterPawn.MapHeld, 30, null, null, null);
                
                var cellsToDirty = DamageDefOf.Flame.Worker.ExplosionCellsToHit(
                    t.Cell, CasterPawn.MapHeld, 2, null, null, null);

                foreach (var cell in cellsToHit)
                {
                    Effecter effecter = EffecterDefOf.AcidSpray_Directional.Spawn();
                    effecter.Trigger(
                        new TargetInfo(position, CasterPawn.MapHeld, false), 
                        new TargetInfo(cell, CasterPawn.MapHeld, false), -1
                        );
                }

                foreach (var cell in cellsToDirty)
                {
                    FilthMaker.TryMakeFilth(
                        cell, CasterPawn.Map, ThingDefOf.Filth_Slime, this.CasterPawn.LabelIndefinite(), 1,
                        FilthSourceFlags.None);
                }

            }
            this.CasterPawn.GetComp<CompElderThing>().SporesReplicating();
            return true;
        }
    }
}