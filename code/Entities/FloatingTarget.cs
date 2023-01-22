using System;
using System.Collections.Generic;
using Sandbox;

namespace Garryware.Entities;

public class FloatingTarget : BreakableProp
{
    public virtual Material SpriteMaterial { get; set; } = Material.Load("materials/ware_bullseye.vmat");

    // This needs to change if you change the model
    private float SpriteScale { get; set; } = 20f;

    protected SceneCustomObject so;

    public override void Spawn()
    {
        Model = CommonEntities.Ball;
        Indestructible = true;
        EnableShadowOnly = true;
        base.Spawn();
    }

    protected override void SetupPhysics()
    {
        base.SetupPhysics();
        
        // Set up physics properties so it floats around the arena
        PhysicsBody.LinearDrag = 0.5f;
        PhysicsBody.DragEnabled = true;
        PhysicsBody.GravityEnabled = false;
        PhysicsBody.Mass = 1000.0f;

        // Start it moving about a little bit by default
        PhysicsBody.Velocity = Vector3.Random * 400.0f;
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        if (Game.IsClient)
        {
            CreateSceneObject();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        so?.Delete();
        so = null;
    }

    private void CreateSceneObject()
    {
        so = new SceneCustomObject(Scene)
        {
            RenderOverride = DoRender
        };
    }

    [Event.Client.Frame]
    protected virtual void Think()
    {
        if (so != null)
        {
            UpdateSceneObject(so);
        }
    }

    public virtual void UpdateSceneObject(SceneObject obj)
    {
        var rot = Rotation.LookAt((Position - Camera.Position).Normal) * Rotation.FromPitch(-90f);
        so.Transform = new Transform(WorldSpaceBounds.Center, rot, Scale);
        so.Bounds = WorldSpaceBounds;
    }
    
    public virtual void DoRender(SceneObject obj)
    {
        Graphics.SetupLighting(obj);
        
        float halfSpriteSize = SpriteScale * 0.5f;
        var positionOnScreen = Position.ToScreen();
        var rect = new Rect(positionOnScreen.x - halfSpriteSize, positionOnScreen.y - halfSpriteSize, SpriteScale, SpriteScale);
        Graphics.DrawQuad(rect, SpriteMaterial, Color.White);
    }
}
