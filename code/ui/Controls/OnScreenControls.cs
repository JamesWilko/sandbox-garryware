using System;
using System.Collections.Generic;
using Sandbox.UI;

namespace Garryware.UI;

public class OnScreenControls : Panel
{
    private Panel canvas;
    private readonly List<Panel> controls = new();

    public OnScreenControls()
    {
        GameEvents.AvailableActionsUpdated += OnAvailableActionsUpdated;
    }

    public override void OnDeleted()
    {
        GameEvents.AvailableActionsUpdated -= OnAvailableActionsUpdated;
        
        base.OnDeleted();
    }

    private void OnAvailableActionsUpdated(PlayerAction actions)
    {
        if (actions == PlayerAction.None)
        {
            if (canvas == null)
                return;
            
            canvas.Delete();
            canvas = null;
        }
        else
        {
            // Create canvas if it doesn't exist
            canvas ??= Add.Panel("canvas");
            
            // Refresh the controls list
            controls.ForEach(c => c.Delete());
            controls.Clear();
            foreach (var control in Enum.GetValues<PlayerAction>())
            {
                if (control != PlayerAction.None && GarrywareGame.Current.AvailableActions.HasFlag(control))
                {
                    var osc = canvas.AddChild<OnScreenControlEntry>();
                    osc.Action = control;
                    controls.Add(osc);
                }
            }
        }
    }

}
