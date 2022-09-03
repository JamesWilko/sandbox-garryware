using Sandbox;

namespace Garryware.Microgames;

public class SkyHigh : Microgame
{
    private bool wantsPlayersInSky = true;
    
    public SkyHigh()
    {
        Rules = MicrogameRules.LoseOnTimeout;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty, MicrogameRoom.Platform };
        WarmupLength = 3;
        GameLength = 1.5f;
    }

    public override void Setup()
    {
        ShowInstructions("#microgame.instructions.sky-high.intro");
        GiveWeapon<RocketLauncher>(To.Everyone);
    }

    public override void Start()
    {
        wantsPlayersInSky = Rand.Float() > 0.35f;
        ShowInstructions(wantsPlayersInSky ? "#microgame.instructions.sky-high.sky" : "#microgame.instructions.sky-high.ground");
    }

    public override void Finish()
    {
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                if ((wantsPlayersInSky && player.Position.z > 200.0f)
                    || (!wantsPlayersInSky && player.Position.z < 100.0f))
                {
                    player.FlagAsRoundWinner();
                }
            }
        }
    }

    public override void Cleanup()
    {
        RemoveAllWeapons();
    }
}