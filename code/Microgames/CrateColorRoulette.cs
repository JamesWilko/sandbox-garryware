using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class CrateColorRoulette : Microgame
{
    private ColorRouletteProp rouletteCrate;
    private GameColor targetColor;

    private readonly ShuffledDeck<float> rotationSpeedsDeck;

    public CrateColorRoulette()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack;
        GameLength = 10;
        
        rotationSpeedsDeck = new ShuffledDeck<float>();
        rotationSpeedsDeck.Add(1.0f, 1);
        rotationSpeedsDeck.Add(0.8f, 3);
        rotationSpeedsDeck.Add(0.6f, 3);
        rotationSpeedsDeck.Add(0.4f, 1);
    }
    
    public override void Setup()
    {
        // Spawn the roulette crate
        var spawn = CommonEntities.AboveBoxSpawnsDeck.Next();
        rouletteCrate = new ColorRouletteProp()
        {
            Position = spawn.Position,
            Rotation = spawn.Rotation,
            Model = CommonEntities.Crate,
            CanGib = false,
            PhysicsEnabled = false,
            Indestructible = true,
            RaisesClientAuthDamageEvent = true,
            RotationTime = rotationSpeedsDeck.Next() // Choose a random rotation speed from the speeds deck to randomize the difficulty a bit
        };
        AutoCleanup(rouletteCrate);
        rouletteCrate.PlayerSentRouletteResult += OnPlayerRouletteResult;
        
        ShowInstructions("Get ready...");
    }
    
    public override void Start()
    {
        // Set the roulette crate off and get a random target color it will cycle through
        rouletteCrate.GenerateNewRotation();
        rouletteCrate.StartRoulette();
        targetColor = rouletteCrate.GetRandomColorInRotation();

        GiveWeapon<Pistol>(To.Everyone);
        ShowInstructions($"Shoot {targetColor.AsName()}!");
    }

    private void OnPlayerRouletteResult(GarrywarePlayer player, ColorRouletteProp prop, GameColor color)
    {
        if (!player.HasLockedInResult)
        {
            if (color == targetColor)
            {
                player.FlagAsRoundWinner();
                player.RemoveWeapons();
            }
            else
            {
                player.FlagAsRoundLoser();
                player.RemoveWeapons();
            }
        }
    }
    
    public override void Finish()
    {
        rouletteCrate.StopRoulette();
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
        rouletteCrate = null;
    }
}