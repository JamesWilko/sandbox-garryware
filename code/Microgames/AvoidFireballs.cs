using System.Collections.Generic;
using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class AvoidFireballs : Microgame
{
    private List<Fireball> fireballs = new();

    public AvoidFireballs()
    {
        Rules = MicrogameRules.WinOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.Reload | PlayerAction.Sprint;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 10f;
    }

    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.avoid-fireballs");
        GiveWeapon<Pistol>(To.Everyone);
    }

    public override void Start()
    {
        int numberOfFireballs = Game.Clients.Count switch
        {
            < 3 => 2,
            < 6 => 3,
            < 9 => 5,
            < 12 => 6,
            _ => 8
        };

        for (int i = 0; i < numberOfFireballs; ++i)
        {
            var fireball = new Fireball()
            {
                Position = Room.InAirSpawnsDeck.Next().Position,
                Rotation = Rotation.Random,
            };
            AutoCleanup(fireball);
            fireballs.Add(fireball);
            fireball.TouchedPlayer += OnTouchedPlayer;
        }
    }

    private void OnTouchedPlayer(GarrywarePlayer player)
    {
        if(!IsGameFinished() && !player.HasLockedInResult)
            player.FlagAsRoundLoser();
    }

    public override void Finish()
    {
        RemoveAllWeapons();

        foreach (var fireball in fireballs)
        {
            fireball.DisableCollisions();
        }
    }

    public override void Cleanup()
    {
        fireballs.Clear();
    }
    
}
