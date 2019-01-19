using System;
using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class AbilityEffect_ShortFlight : AbilityUser.Verb_UseAbility
    {
        protected override bool TryCastShot()
        {

            base.TryCastShot();
            this.Ability.CooldownTicksLeft = this.Ability.MaxCastingTicks;
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                Pawn caster = CasterPawn;
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    FlyingObject flyingObject =
                        GenSpawn.Spawn(ThingDef.Named("ElderThing_PFlyingObject"), CasterPawn.Position,
                            CasterPawn.Map) as FlyingObject;
                    flyingObject.Launch(CasterPawn, t.Cell, CasterPawn);
                }, "LaunchingFlyer", false, null);
            }

            return true;
        }
        
    }
}