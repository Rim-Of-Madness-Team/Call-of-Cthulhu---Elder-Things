using System;
using RimWorld;
using Verse;

namespace ElderThingFaction
{
    public class CompAbilityEffect_SpawnSpores : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            parent.pawn.health.AddHediff(def: HediffDef.Named(defName: "ROMET_SporesReplicating"));
            
            var position = parent.pawn.Position;
            Plant_ElderThingSporePod pod = (Plant_ElderThingSporePod)GenSpawn.Spawn(def: ThingDef.Named(defName: "ET_PlantElderThingSporePod"),
                loc: target.Cell,
                map: parent.pawn.Map);
            pod.Parent = parent.pawn;

            var cellsToHit = DamageDefOf.Flame.Worker.ExplosionCellsToHit(
                target.Cell, parent.pawn.MapHeld, 30, null, null, null);
                
            var cellsToDirty = DamageDefOf.Flame.Worker.ExplosionCellsToHit(
                target.Cell, parent.pawn.MapHeld, 2, null, null, null);

            foreach (var cell in cellsToHit)
            {
                Effecter effecter = EffecterDefOf.AcidSpray_Directional.Spawn();
                effecter.Trigger(
                    new TargetInfo(position, parent.pawn.MapHeld, false), 
                    new TargetInfo(cell, parent.pawn.MapHeld, false), -1
                );
            }

            foreach (var cell in cellsToDirty)
            {
                FilthMaker.TryMakeFilth(
                    cell, parent.pawn.Map, ThingDefOf.Filth_Slime, parent.pawn.LabelIndefinite(), 1,
                    FilthSourceFlags.None);
            }

        }
        
        public override bool GizmoDisabled(out string reason)
        {
            if (parent?.pawn?.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                reason = "ET_NotAnAdult".Translate(parent.pawn);
                return true;
            }
            if (parent?.pawn?.health?.hediffSet?.HasHediff(def: HediffDef.Named(defName: "ROMET_SporesReplicating")) == true)
            {
                reason = "ET_SporesAreCurrentlyReplicating".Translate(parent.pawn);
                return true;
            }
            reason = null;
            return base.GizmoDisabled(out reason);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            //Not on buildings or occupied squares
            if (target.Cell.Filled(parent.pawn.Map) || (target.Cell.GetFirstBuilding(parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message(
                        "CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityOccupiedCells".Translate(),
                        target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            //Must be on fertile soil
            if (target.Cell.GetFertility(parent.pawn.MapHeld) <= 0f)
            {
                if (throwMessages)
                {
                    Messages.Message(
                        "CannotUseAbility".Translate(parent.def.label) + ": " + "ET_AbilityFertileSoil".Translate(),
                        target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                }
            }
            return true;
        }
    }
}
