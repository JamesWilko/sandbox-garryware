﻿using System;
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

    [GameEvent.Client.Frame]
    protected virtual void Think()
    {
        if (so != null)
        {
            UpdateSceneObject(so);
        }
    }

    public virtual void UpdateSceneObject(SceneObject obj)
    {
        so.Transform = new Transform(WorldSpaceBounds.Center, Rotation, Scale);
        so.Bounds = WorldSpaceBounds;
    }
    
    public virtual void DoRender(SceneObject obj)
    {
        Graphics.SetupLighting(obj);
        
        // Create the vertex buffer for the sprite
        var vb = new VertexBuffer();

        // Vertex buffers are in local space, so we need the camera position in local space too
        var normal = obj.Transform.PointToLocal(Camera.Position).Normal;
        var w = normal.Cross(Vector3.Down).Normal;
        var h = normal.Cross(w).Normal;
        float halfSpriteSize = SpriteScale / 2;

        // Add a single quad to our vertex buffer
        vb.AddQuad(new Ray(default, normal), w * halfSpriteSize, h * halfSpriteSize);

        // Draw the sprite
        vb.Draw(SpriteMaterial);
    }
}
