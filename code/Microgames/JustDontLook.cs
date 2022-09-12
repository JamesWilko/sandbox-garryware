using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class JustDontLook : Microgame
{
    private bool hasStarted;
    
    public JustDontLook()
    {
        Rules = MicrogameRules.WinOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Empty, MicrogameRoom.Boxes };
        WarmupLength = 2;
        GameLength = 6;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.just-dont-look");
        
        var eye = new FloatingEye()
        {
            Position = Room.InAirSpawnsDeck.Next().Position,
            Scale = 5.0f
        };
        AutoCleanup(eye);
        eye.PlayerLookedAt += OnPlayerLookedAtEye;
    }
    
    public override void Start()
    {
        hasStarted = true;
        GiveWeapon<RocketLauncher>(To.Everyone);
    }

    private void OnPlayerLookedAtEye(GarrywarePlayer player)
    {
        if(hasStarted && !player.HasLockedInResult)
            player.FlagAsRoundLoser();
    }
    
    public override void Finish()
    {
        hasStarted = false;
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
    
}