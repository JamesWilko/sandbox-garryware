using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class RocketJump : Microgame
{
    public RocketJump()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        WarmupLength = 2;
        GameLength = 5;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
        
        int platforms = GetRandomAdjustedClientCount(0.3f, 0.6f);
        for (int i = 0; i < platforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.InAirSpawnsDeck.Next().Position - Vector3.Up * 128.0f
            };
            AutoCleanup(platform);
        }
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.rocket-jump");
        GiveWeapon<RocketLauncher>(To.Everyone);
    }

    public override void Finish()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player && IsPlayerOnPlatform(player))
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    private bool IsPlayerOnPlatform(GarrywarePlayer player)
    {
        if (player.GroundEntity == null)
            return false;

        // If we're standing on somebody then check if they're standing on a platform and use their result
        if (player.GroundEntity is GarrywarePlayer otherPlayer)
            return IsPlayerOnPlatform(otherPlayer);

        // Platforms are the only props in this game, so just check we're standing on one
        return player.GroundEntity is BreakableProp;
    }

    public override void Cleanup()
    {
    }
}
