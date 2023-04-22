using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class OnScreenControlEntry : Panel
{
    public PlayerAction Action { get; set; }
    
    private Image image;
    private Label text;
        
    public OnScreenControlEntry()
    {
        image = Add.Image();
        text = Add.Label();
    }

    public override void Tick()
    {
        base.Tick();

        image.Texture = Input.GetGlyph(Action.AsInputAction(), InputGlyphSize.Small, GlyphStyle.Light);
        text.Text = Action.AsFriendlyName();
    }
}
