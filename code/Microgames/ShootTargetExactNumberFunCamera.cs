using Sandbox;

namespace Garryware.Microgames;

public class ShootTargetExactNumberFunCamera : ShootTargetExactNumber
{
    public override void Setup()
    {
        base.Setup();

        var cameraType = ShootTargetAtLeastNumberFunCamera.AvailableCameraModes.Next();
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.OverrideCameraType(cameraType);
            }
        }
    }

    public override void Finish()
    {
        base.Finish();
        
        foreach (var client in Client.All)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.RestoreNormalCamera();
            }
        }
    }
    
}
