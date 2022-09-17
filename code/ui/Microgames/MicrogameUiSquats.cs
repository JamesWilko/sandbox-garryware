﻿using Sandbox;
using Sandbox.UI;

namespace Garryware.UI.Microgames;

public class MicrogameUiSquats : Panel
{
    public Image Up { get; set; } 
    public Image Down { get; set; }

    private bool hasSquatted;
    
    public MicrogameUiSquats()
    {
        StyleSheet.Load("/ui/Microgames/MicrogameUiSquats.scss");
        
        Up = AddChild<Image>("up");
        Up.Texture = Texture.Load(FileSystem.Mounted, "ui/microgames/squat-up.png");
        
        Down = AddChild<Image>("down");
        Down.Texture = Texture.Load(FileSystem.Mounted, "ui/microgames/squat-down.png");
    }

    public override void Tick()
    {
        base.Tick();

        if (Local.Pawn is GarrywarePlayer player)
        {
            Up.Style.Opacity = player.IsSquatting ? 0 : 1;
            Down.Style.Opacity = player.IsSquatting ? 1 : 0;

            if (player.IsSquatting && !hasSquatted)
            {
                SoundUtility.PlaySmallTargetHit();
                hasSquatted = true;
            }
            if (!player.IsSquatting)
            {
                hasSquatted = false;
            }
        }
    }
    
}