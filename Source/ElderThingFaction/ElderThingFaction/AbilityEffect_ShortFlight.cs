using System;
using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class AbilityEffect_ShortFlight : AbilityUser.Verb_UseAbility
    {
        public virtual void Effect()
        {
            if (TargetsAoE[0] is LocalTargetInfo t && t.Cell != default(IntVec3))
            {
                Pawn caster = CasterPawn;
                LongEventHandler.QueueLongEvent(delegate()
                {
                    FlyingObject flyingObject =
                        GenSpawn.Spawn(ThingDef.Named("ElderThing_PFlyingObject"), CasterPawn.Position,
                            CasterPawn.Map) as FlyingObject;
                    flyingObject.Launch(CasterPawn, t.Cell, CasterPawn);
                }, "LaunchingFlyer", false, null);
            }
        }

        public override void PostCastShot(bool inResult, out bool outResult)
        {
            if (inResult)
            {
                Effect();
                outResult = true;
            }

            outResult = inResult;
        }
    }
}