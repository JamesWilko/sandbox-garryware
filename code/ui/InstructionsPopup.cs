using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public class InstructionsPopup : Panel
{
    private readonly Label instructionText;

    public string Text
    {
        get => instructionText.Text;
        set => instructionText.SetText(value);
    }

    public InstructionsPopup()
    {
        instructionText = Add.Label();
    }
    
}
