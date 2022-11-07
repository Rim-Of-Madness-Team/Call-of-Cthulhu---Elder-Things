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
            if (TargetsAoE[index: 0] is var t && t.Cell != default)
            {
                var unused = CasterPawn;
                LongEventHandler.QueueLongEvent(action: delegate
                {
                    var flyingObject =
                        GenSpawn.Spawn(def: ThingDef.Named(defName: "ElderThing_PFlyingObject"), loc: CasterPawn.Position,
                            map: CasterPawn.Map) as FlyingObject;
                    flyingObject?.Launch(launcher: CasterPawn, targ: t.Cell, flyingThing: CasterPawn);
                }, textKey: "LaunchingFlyer", doAsynchronously: false, exceptionHandler: null);
            }

            return true;
        }
    }
}