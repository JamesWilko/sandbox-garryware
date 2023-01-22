using Sandbox;

namespace Garryware.Microgames;

public class ShootTargetAtLeastNumberFunCamera : ShootTargetAtLeastNumber
{
    public static readonly ShuffledDeck<CameraMode> AvailableCameraModes;

    static ShootTargetAtLeastNumberFunCamera()
    {
        AvailableCameraModes = new();
        AvailableCameraModes.Add(CameraMode.FirstPersonWobbly);
        AvailableCameraModes.Add(CameraMode.FirstPersonInverted);
        AvailableCameraModes.Add(CameraMode.FirstPersonInvertedControls);
        AvailableCameraModes.Shuffle();
    }
    
    public override void Setup()
    {
        base.Setup();

        var cameraType = AvailableCameraModes.Next();
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.OverrideCameraMode(cameraType);
            }
        }
    }

    public override void Finish()
    {
        base.Finish();
        
        foreach (var client in Game.Clients)
        {
            if (client.Pawn is GarrywarePlayer player)
            {
                player.RestoreNormalCamera();
            }
        }
    }
    
}
