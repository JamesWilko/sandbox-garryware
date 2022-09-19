using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// Players have to sprint around the place without stopping. If they stop they lose. After a short while a number of chairs spawn that players have to sit in to win.
/// </summary>
public class MusicalChairs : Microgame
{
    private bool dontStopSprinting;

    public MusicalChairs()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Sprint;
        ShowActionsToPlayer = ShowGameActions.AfterSetup;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        WarmupLength = 2;
        GameLength = 7;
    }

    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.musical-chairs.phase-1");
    }

    public override void Start()
    {
        RunGamePhases();
    }

    private async void RunGamePhases()
    {
        dontStopSprinting = true;
        await GameTask.DelaySeconds(3f);
        dontStopSprinting = false;
        
        int chairsToSpawn = GetRandomAdjustedClientCount(0.25f, 0.75f, 1, Room.OnFloorSpawns.Count);
        for (int i = 0; i < chairsToSpawn; ++i)
        {
            var spawn = Room.OnFloorSpawnsDeck.Next();
            var chair = new Chair()
            {
                Position = spawn.Position,
                Rotation = Rotation.From(0, Rand.Float(-359f, 359f), 0)
            };
            AutoCleanup(chair);
        }
        ShowInstructions("#microgame.instructions.musical-chairs.phase-2");
    }
    
    public override void Tick()
    {
        if (dontStopSprinting)
        {
            foreach (var client in Client.All)
            {
                if (client.Pawn is GarrywarePlayer player && !player.IsMovingAtSprintSpeed && !player.IsInAChair)
                {
                    player.FlagAsRoundLoser();
                }
            }
        }
    }

    public override void Finish()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                if (player.IsInAChair)
                {
                    player.FlagAsRoundWinner();
                }
            }
        }
    }

    public override void Cleanup()
    {
    }
}
