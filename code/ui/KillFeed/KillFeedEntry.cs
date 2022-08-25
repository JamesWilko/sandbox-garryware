using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Garryware.UI;

public partial class KillFeedEntry : Panel
{
    public Label Left { get; internal set; }
    public Label Method { get; internal set; }

    public RealTimeSince TimeSinceBorn = 0;

    public KillFeedEntry()
    {
        Left = Add.Label("", "left");
        Method = Add.Label("", "method");
    }

    public override void Tick()
    {
        base.Tick();

        if (TimeSinceBorn > 4)
        {
            Delete();
        }
    }
}