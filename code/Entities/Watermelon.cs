using Sandbox;

namespace Garryware.Entities;

public class Watermelon : BreakableProp
{
    public override void Spawn()
    {
        base.Spawn();
        Model = CommonEntities.Watermelon;
        UpdateIndestructibility();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        UpdateIndestructibility();
    }

    public override void OnNewModel(Model model)
    {
        base.OnNewModel(model);
        UpdateIndestructibility();
    }

    private void UpdateIndestructibility()
    {
        if(!IsClient)
            return;
        
        RenderColor = Indestructible ? new Color(0.75f, 0.75f, 0.75f) : Color.White;
        
        if (PhysicsBody != null)
        {
            foreach (var shape in PhysicsBody.Shapes)
            {
                shape.SurfaceMaterial = Indestructible ? "metal" : "watermelon";
            }
        }
    }
    
}
