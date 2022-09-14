using Garryware.Entities;
using Sandbox;

namespace Garryware.Microgames;

public class HitTheTargetPhysics : Microgame
{
    private readonly ShuffledDeck<float> scalesDeck = new();
    private readonly ShuffledDeck<Model> rubbishModels = new();
    
    public HitTheTargetPhysics()
    {
        Rules = MicrogameRules.LoseOnTimeout | MicrogameRules.EndEarlyIfEverybodyLockedIn;
        ActionsUsedInGame = PlayerAction.PrimaryAttack | PlayerAction.SecondaryAttack;
        AcceptableRooms = new[] { MicrogameRoom.Boxes, MicrogameRoom.Empty };
        GameLength = 6f;
    }

    private void BuildDifficultyDeck()
    {
        scalesDeck.Clear();
        scalesDeck.Add(5.0f, 3);
        scalesDeck.Add(3.0f, 3);
        scalesDeck.Add(2.0f, 1);
        scalesDeck.Shuffle();
        
        rubbishModels.Clear();
        rubbishModels.Add(Model.Load("models/citizen_props/bathroomsink01.vmdl"), 2);
        rubbishModels.Add(Model.Load("models/sbox_props/pizza_box/pizza_box.vmdl"), 5);
        rubbishModels.Add(Model.Load("models/sbox_props/bin/rubbish_bag.vmdl"), 3);
        rubbishModels.Add(Model.Load("models/sbox_props/burger_box/burger_box.vmdl"), 3);
        rubbishModels.Add(Model.Load("models/citizen_props/trashbag02.vmdl"), 5);
        rubbishModels.Shuffle();
    }
    
    public override void Setup()
    {
        ShowInstructions("#microgame.get-ready");
        BuildDifficultyDeck();
        
        // Spawn the target
        var targetEntity = new FloatingTarget
        {
            Transform = Room.InAirSpawnsDeck.Next().Transform
        };
        AutoCleanup(targetEntity);
        targetEntity.Scale = scalesDeck.Next();
        targetEntity.PhysicsCollision += OnPhysicsCollisionWithTarget;
        
        // Spawn a bunch of crap
        int amountOfRubbish = GetRandomAdjustedClientCount(0.65f, 1.35f, 5, 30);
        for (int i = 0; i < amountOfRubbish; ++i)
        {
            var rubbish = new BreakableProp
            {
                Transform = Room.InAirSpawnsDeck.Next().Transform,
                Model = rubbishModels.Next(),
                Indestructible = true,
            };
            AutoCleanup(rubbish);
            rubbish.ApplyLocalImpulse(Vector3.Random * 1024.0f);
        }
        
    }

    private void OnPhysicsCollisionWithTarget(CollisionEventData collisionData)
    {
        if(IsGameFinished())
            return;
        
        if (collisionData.Other.Entity is BreakableProp prop
            && prop.ClientLastPickedUpBy != null
            && prop.ClientLastPickedUpBy.IsValid()
            && prop.ClientLastPickedUpBy.Pawn is GarrywarePlayer player)
        {
            Particles.Create("particles/impact.smokepuff.vpcf", prop.Position).Destroy();
            Sound.FromEntity("sounds/balloon_pop_cute.sound", collisionData.This.Entity);
            prop.Delete();

            if (!player.HasLockedInResult)
            {
                player.FlagAsRoundWinner();
            }
        }
    }

    public override void Start()
    {
        ShowInstructions("#microgame.instructions.hit-the-target-physics");
        GiveWeapon<GravityGun>(To.Everyone);
    }

    public override void Finish()
    {
        RemoveAllWeapons();
    }

    public override void Cleanup()
    {
    }
    
}
