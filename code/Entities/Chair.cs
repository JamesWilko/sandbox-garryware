using Sandbox;

namespace Garryware.Entities;

public partial class Chair : BreakableProp, IUse
{
    public GarrywarePlayer Sitter { get; private set; }

    public override void Spawn()
    {
        Model = Cloud.Model("rust.chair_b");
        Indestructible = true;
        Static = true;
        PhysicsEnabled = false;
        base.Spawn();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        RemoveSitter();
    }

    public void RemoveSitter()
    {
        if (Sitter == null) return;
        if (Sitter.IsValid)
        {
            Sitter.SetAnimParameter("sit", 0);
            Sitter.Parent = null;
            Sitter.Position += Vector3.Up * 80;
            Sitter.Controller.Position = Sitter.Position;

            if (Sitter.PhysicsBody.IsValid())
            {
                Sitter.PhysicsBody.Enabled = true;
                Sitter.PhysicsBody.Position = Sitter.Position;
            }

            Sitter.Controller = new GarrywareWalkController();
            Sitter.ThirdPersonCamera = false;
        }

        Sitter = null;
    }

    public bool IsUsable(Entity user)
    {
        if (user is GarrywarePlayer player)
        {
            return Sitter == null && !player.HasLockedInResult;
        }
        return false;
    }

    public bool OnUse(Entity user)
    {
        if (user is GarrywarePlayer player)
        {
            player.Parent = this;
            player.LocalPosition = new Vector3(7f, 0f, 7.5f);
            player.LocalRotation = Rotation.Identity;
            player.LocalScale = 1;
            player.Velocity = Vector3.Zero;
            player.PhysicsBody.Enabled = false;
            player.Controller = new ChairController();
            player.ThirdPersonCamera = true;

            Sitter = player;
        }

        return false;
    }
}
