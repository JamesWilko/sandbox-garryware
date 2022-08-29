using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.UI;

namespace Garryware.UI;

public class OnScreenControls : Panel
{
    private Panel canvas;
    private List<PlayerAction> activeActions = new();
    private Dictionary<PlayerAction, OnScreenControlEntry> onScreenControls = new();
    
    public override void Tick()
    {
        base.Tick();
        
        // Update which actions are currently active
        activeActions.Clear();
        foreach (var action in Enum.GetValues<PlayerAction>())
        {
            if (action != PlayerAction.None && GarrywareGame.Current.AvailableActions.HasFlag(action))
            {
                activeActions.Add(action);
            }
        }

        if (activeActions.Count > 0)
        {
            canvas ??= Add.Panel("canvas");
        }
        else if (canvas != null)
        {
            onScreenControls.Clear();
            canvas.Delete();
            canvas = null;
        }
        
        // Ensure canvas exists before adding actions to it
        if(canvas == null)
            return;
        
        // Add rows for new actions
        foreach (var action in activeActions.Except(onScreenControls.Keys))
        {
            if (action != PlayerAction.None)
            {
                var osc = canvas.AddChild<OnScreenControlEntry>();
                osc.Action = action;
                onScreenControls.Add(action, osc);
            }
        }

        // Remove rows for clients that left
        foreach (var action in onScreenControls.Keys.Except(activeActions))
        {
            if (onScreenControls.TryGetValue(action, out var control))
            {
                control?.Delete();
                onScreenControls.Remove(action);
            }
        }
    }
    
}
