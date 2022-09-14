using Sandbox;

namespace Garryware.Microgames;

public class ShootTargetAtLeastNumberFunCamera : ShootTargetAtLeastNumber
{
    public static readonly ShuffledDeck<System.Type> AvailableCameraModes;

    static ShootTargetAtLeastNumberFunCamera()
    {
        AvailableCameraModes = new();
        AvailableCameraModes.Add(typeof(WobblyFirstPersonCamera));
        AvailableCameraModes.Add(typeof(InvertedFirstPersonCamera));
        AvailableCameraModes.Add(typeof(InvertedControlsFirstPersonCamera));
        AvailableCameraModes.Shuffle();
    }
    
    public override void Setup()
    {
        base.Setup();

        var cameraType = AvailableCameraModes.Next();
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
