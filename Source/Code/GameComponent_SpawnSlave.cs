using System.Linq;
using RimWorld;
using Verse;

namespace ElderThingFaction;

public class GameComponent_SpawnSlave : GameComponent
{
    
    public GameComponent_SpawnSlave(Game game)
    {
    }

    private bool started = false;

    public override void StartedNewGame()
    {
        if (Find.Scenario.name == "Elder Things")
        {
            started = true;
        }
        base.StartedNewGame();
    }

    public override void GameComponentTick()
    {
        if (started && Find.TickManager.TicksGame > 30)
        {
            started = false;
            Pawn newPawn = PawnGenerator.GeneratePawn(
                new PawnGenerationRequest(
                    kind: PawnKindDefOf.Slave,
                    faction: Faction.OfPlayer
                )
                );
            GenSpawn.Spawn(newPawn, 
                Find.CurrentMap.mapPawns.FreeColonists.RandomElement().Position.RandomAdjacentCell8Way(), Find.CurrentMap);
            GenGuest.EnslavePrisoner(Find.CurrentMap.mapPawns.FreeColonists.RandomElement(),newPawn);
        }
        base.GameComponentTick();
    }
}