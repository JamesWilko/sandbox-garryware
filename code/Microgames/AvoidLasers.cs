using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

/// <summary>
/// A number of lasers appear in the room, players must avoid them.
/// </summary>
public class AvoidLasers : Microgame
{
    private bool hasStarted = false;
    private List<Laser> lasers = new();

    public AvoidLasers()
    {
        Rules = MicrogameRules.WinOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.Sprint | PlayerAction.Crouch | PlayerAction.Jump;
        AcceptableRooms = new[] { MicrogameRoom.Empty };
        WarmupLength = 5f;
        GameLength = 10f;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.avoid-lasers");

        hasStarted = false;

        int numLasers = Game.Random.Float() > 0.2f ? 2 : 3;
        for (int i = 0; i < numLasers; ++i)
        {
            var laser = new Laser()
            {
                Position = Room.OnFloorSpawnsDeck.Next().Position,
                LaserColor = Color.Blue
            };
            lasers.Add(laser);
            AutoCleanup(laser);
            laser.Zapped += OnZappedPlayer;
        }
    }
    
    public override void Start()
    {
        hasStarted = true;
        foreach (var laser in lasers)
        {
            laser.LaserColor = Color.Red;
        }
    }
    
    private void OnZappedPlayer(GarrywarePlayer player)
    {
        if (hasStarted && !player.HasLockedInResult)
        {
            player.FlagAsRoundLoser();
        }
    }

    public override void Finish()
    {
        hasStarted = false;
        foreach (var laser in lasers)
        {
            laser.LaserColor = Color.Blue;
        }
    }

    public override void Cleanup()
    {
        lasers.Clear();
    }
    
}
