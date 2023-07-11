using System;
using Sandbox;

namespace Garryware.Entities;

public partial class Laser : ModelEntity
{
    [Net] public Vector3 LaserColor { get; set; } = Color.Red;
    
    public delegate void LaserHitDelegate(GarrywarePlayer player);
    public event LaserHitDelegate Zapped;
    
    private Particles LaserParticles { get; set; }
    
    private static ShuffledDeck<float> speedMultiplierDeck;

    private float currentTime;
    private float speed;
    private int direction;
    private float currentRotation;
    private float currentHeight;

    static Laser()
    {
        speedMultiplierDeck = new();
        speedMultiplierDeck.Add(1.0f, 2);
        speedMultiplierDeck.Add(1.3f, 3);
        speedMultiplierDeck.Add(1.6f, 2);
        speedMultiplierDeck.Add(2.0f, 2);
        speedMultiplierDeck.Add(3.0f, 1);
        speedMultiplierDeck.Shuffle();
    }
    
    public override void Spawn()
    {
        base.Spawn();
        
        Model = CommonEntities.Watermelon;

        currentTime = Game.Random.Float(0f, 60f);
        speed = speedMultiplierDeck.Next();
        direction = Game.Random.Float() > 0.5f ? 1 : -1;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        LaserParticles?.Destroy(true);
    }

    [GameEvent.Tick.Server]
    protected void UpdateRotation()
    {
        // Rotate and move about
        currentTime += Game.TickInterval * speed;
        
        currentRotation = currentTime * 30f * direction;
        Rotation = new Angles(0f, currentRotation, 0f).ToRotation();

        currentHeight = 60.0f + MathF.Sin(currentTime) * 40f * direction;
        Position = Position.WithZ(currentHeight);
        
        // See if we zapped a player
        var (tr1, tr2) = GetTraces();
        ZapPlayerIfPossible(tr1);
        ZapPlayerIfPossible(tr2);
    }

    [GameEvent.Tick.Client]
    protected void UpdateParticles()
    {
        if (LaserParticles == null)
        {
            LaserParticles = Particles.Create("particles/microgame.laser.vpcf", this);
            LaserParticles.SetPosition(4, new Vector3(10f, 1f, 0f));
        }

        var (tr1, tr2) = GetTraces();
        LaserParticles.SetPosition(3, LaserColor);
        LaserParticles?.SetPosition(1, tr1.HitPosition);
        LaserParticles?.SetPosition(2, tr2.HitPosition);
    }

    private (TraceResult, TraceResult) GetTraces()
    {
        const float maxRange = 1000f;
        const float radius = 2f;
        
        var tr1 = Trace.Ray(Position, Position + Rotation.Right * maxRange)
            .UseHitboxes()
            .Ignore(this)
            .Size(radius)
            .Run();
        
        var tr2 = Trace.Ray(Position, Position + Rotation.Left * maxRange)
            .UseHitboxes()
            .Ignore(this)
            .Size(radius)
            .Run();

        return (tr1, tr2);
    }

    private void ZapPlayerIfPossible(TraceResult result)
    {
        if (result.Hit && result.Entity is GarrywarePlayer player)
        {
            Zapped?.Invoke(player);
        }
    }
    
}
