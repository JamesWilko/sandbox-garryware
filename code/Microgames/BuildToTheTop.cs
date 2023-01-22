using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class BuildToTheTop : Microgame
{
    
    public BuildToTheTop()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.Punt | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        WarmupLength = 3;
        GameLength = 20;
        // MinimumPlayers = 2;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.build-to-the-top");
        
        // Spawn a bunch of platforms in the air
        int platforms = GetRandomAdjustedClientCount(0.3f, 0.6f);
        for (int i = 0; i < platforms; ++i)
        {
            var platform = new Platform()
            {
                Position = Room.InAirSpawnsDeck.Next().Position - Vector3.Up * 256.0f
            };
            AutoCleanup(platform);
        }
        
        // Spawn a ton of small props the players can pick up and place
        int props = GetRandomAdjustedClientCount(5.0f, 10.0f);
        for (int i = 0; i < props; ++i)
        {
            // @todo: add spawn fx
            var prop = new BuildProp
            {
                Position = Room.AboveBoxSpawnsDeck.Next().Position,
                Model = CommonEntities.Crate,
                Indestructible = true
            };
            prop.ApplyAbsoluteImpulse(Vector3.Random.Normal * Game.Random.Float(200f, 500f));
            AutoCleanup(prop);
        }

    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.build-to-the-top.hint");
        GiveWeapon<GravityGun>(To.Everyone);
    }

    public override void Finish()
    {
        foreach (var client in Game.Clients)
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
        
        return player.GroundEntity is Platform;
    }

    public override void Cleanup()
    {
    }
    
}
