using AbilityUser;
using Verse;

namespace ElderThingFaction
{
    public class AbilityEffect_ShortFlight : Verb_UseAbility
    {
        protected override bool TryCastShot()
        {
            base.TryCastShot();
            Ability.CooldownTicksLeft = Ability.MaxCastingTicks;
            if (TargetsAoE[0] is var t && t.Cell != default)
            {
                var unused = CasterPawn;
                LongEventHandler.QueueLongEvent(delegate
                {
                    var flyingObject =
                        GenSpawn.Spawn(ThingDef.Named("ElderThing_PFlyingObject"), CasterPawn.Position,
                            CasterPawn.Map) as FlyingObject;
                    flyingObject?.Launch(CasterPawn, t.Cell, CasterPawn);
                }, "LaunchingFlyer", false, null);
            }

            return true;
        }
    }
}