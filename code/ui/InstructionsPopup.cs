using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class InstructionsPopup : Panel
{
    private readonly Label instructionText;

    private bool hasLifetime;
    private TimeUntil timeUntilAutoRemoval;
    
    public string Text
    {
        get => instructionText.Text;
        set => instructionText.SetText(value);
    }

    public float Lifetime
    {
        set
        {
            hasLifetime = true;
            timeUntilAutoRemoval = value;
        }
    }

    public InstructionsPopup()
    {
        instructionText = Add.Label();
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
