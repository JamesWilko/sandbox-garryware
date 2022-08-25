using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class SimplePopup : Panel
{
    private readonly Label label;

    private bool hasLifetime;
    private TimeUntil timeUntilAutoRemoval;
    
    public string Text
    {
        get => label.Text;
        set => label.SetText(value);
    }

    public float Lifetime
    {
        set
        {
            hasLifetime = true;
            timeUntilAutoRemoval = value;
        }
    }

    public SimplePopup()
    {
        label = Add.Label();
    }

    public override void Tick()
    {
        base.Tick();

        if (hasLifetime && timeUntilAutoRemoval <= 0)
        {
            Delete();
        }
    }
}
