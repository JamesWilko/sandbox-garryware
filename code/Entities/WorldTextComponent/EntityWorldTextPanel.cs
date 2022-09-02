using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.Entities;

public class EntityWorldTextPanel : WorldPanel
{
    private const float w = 1000;
    private const float h = 1000;

    public Entity Owner { get; set; }
    public string Text { get; set; }
    
    private Label text;

    public EntityWorldTextPanel()
    {
        PanelBounds = new Rect(-(w / 2), -(h / 2), w, h);
        StyleSheet.Load("/code/Entities/WorldTextComponent/EntityWorldTextPanel.scss");
        
        text = Add.Label(string.Empty);
    }
    
    public override void Tick()
    {
        base.Tick();

        text.Text = Text;
        
        if (Local.Client == null)
            return;

        if (Owner != null && Owner.IsValid && Local.Client.Pawn.IsValid)
        {
            // Orbit around the owner entity so its always visible, and always point at the local client
            float orbitRadius = Owner.WorldSpaceBounds.Size.Length * 0.5f;
            Vector3 centerPoint = Owner.WorldSpaceBounds.Center;
            Vector3 closestPoint = Local.Client.Pawn.Position;
            
            float vX = closestPoint.x - centerPoint.x;
            float vY = closestPoint.y - centerPoint.y;
            float magV = (float) Math.Sqrt(vX * vX + vY * vY);
            float x = centerPoint.x + vX / magV * orbitRadius;
            float y = centerPoint.y + vY / magV * orbitRadius;
            
            PanelBounds = new Rect(-(w / 2), -(h / 2), w, h);
            Position = new Vector3(x, y, Owner.WorldSpaceBounds.Center.z);
            Rotation = Rotation.LookAt(Local.Client.Pawn.EyePosition - Transform.Position);
        }
    }
    
}
