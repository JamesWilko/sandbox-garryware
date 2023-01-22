using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class BalloonPop : Microgame
{

    public BalloonPop()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes };
        GameLength = 5.5f;
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.balloon-pop");
        
        int balloonsToSpawn = GetRandomAdjustedClientCount(0.6f, 0.9f);
        for (int i = 0; i < balloonsToSpawn; ++i)
        {
            var balloon = new Balloon
            {
                Position = Room.OnBoxSpawnsDeck.Next().Position,
                AutoPop = true,
                TimeUntilPop = Game.Random.Float(3.0f, 5.0f)
            };
            AutoCleanup(balloon);
            balloon.OnBroken += OnBalloonPopped;
        }

        GiveWeapon<Pistol>(To.Everyone);
    }

    private void OnBalloonPopped(BreakableProp self, Entity attacker)
    {
        if (attacker is GarrywarePlayer player && !player.HasLockedInResult)
        {
            player.FlagAsRoundWinner();
            player.RemoveWeapons();
        }
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
    
}
