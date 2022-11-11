using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElderThingFaction
{
    public class Verb_CastAbilityShortFlight : Verb_CastAbility
    {
        public override bool MultiSelect
        {
            get { return true; }
        }

        protected override float EffectiveRange
        {
            get
            {
                if (this.cachedEffectiveRange < 0f)
                {
                    if (base.EquipmentSource != null)
                    {
                        this.cachedEffectiveRange = base.EquipmentSource.GetStatValue(StatDefOf.JumpRange, true, -1);
                    }
                    else
                    {
                        this.cachedEffectiveRange = this.verbProps.range;
                    }
                }

                return this.cachedEffectiveRange;
            }
        }

        protected override bool TryCastShot()
        {
            return base.TryCastShot() && ShortFlightUtility.DoShortFlight(this.CasterPawn, this.currentTarget,
                base.ReloadableCompSource, this.verbProps);
        }

        public override void OnGUI(LocalTargetInfo target)
        {
            if (this.CanHitTarget(target) && ShortFlightUtility.ValidFlightTarget(this.caster.Map, target.Cell))
            {
                base.OnGUI(target);
                return;
            }

            GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            ShortFlightUtility.OrderShortFlight(this.CasterPawn, target, this, this.EffectiveRange);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return this.caster != null && this.CanHitTarget(target) &&
                   ShortFlightUtility.ValidFlightTarget(this.caster.Map, target.Cell) &&
                   ReloadableUtility.CanUseConsideringQueuedJobs(this.CasterPawn, base.EquipmentSource, true);
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            return ShortFlightUtility.CanHitTargetFrom(this.CasterPawn, root, targ, this.EffectiveRange);
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target.IsValid && ShortFlightUtility.ValidFlightTarget(this.caster.Map, target.Cell))
            {
                GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
            }

            GenDraw.DrawRadiusRing(this.caster.Position, this.EffectiveRange, Color.white,
                (IntVec3 c) => GenSight.LineOfSight(this.caster.Position, c, this.caster.Map, false, null, 0, 0) &&
                               ShortFlightUtility.ValidFlightTarget(this.caster.Map, c));
        }

        private float cachedEffectiveRange = -1f;
    }
}