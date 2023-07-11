using Sandbox;

namespace Garryware.Entities;

public class FloatingEye : FloatingTarget
{
    public delegate void PlayerDelegate(GarrywarePlayer player);
    public event PlayerDelegate PlayerLookedAt;
    
    public override Material SpriteMaterial { get; set; } = Material.Load("materials/ware_eye.vmat");

    private RealTimeSince timeSinceLastLookedAt;
    
    public override void UpdateSceneObject(SceneObject obj)
    {
        so.Transform = new Transform(WorldSpaceBounds.Center, Rotation.Identity, Scale);
        so.Bounds = WorldSpaceBounds;
    }
    
    [GameEvent.Tick.Client]
    private void TickCheckIfClientLookedAt()
    {
        if(!IsValid)
            return;
        
        var positionOnScreen = Position.ToScreen(Screen.Size);
        if(!positionOnScreen.HasValue)
            return;

        var actualScreenPos = positionOnScreen.Value;
        bool isOnScreen = actualScreenPos.x >= 0 && actualScreenPos.x <= Screen.Size.x && actualScreenPos.y >= 0 && actualScreenPos.y <= Screen.Size.y;
        if (isOnScreen && timeSinceLastLookedAt > 0.5f) // Small delay so we don't spam the server
        {
            SendLookedAtToServer(NetworkIdent);
            timeSinceLastLookedAt = 0.0f;
        }
    }
    
    [ConCmd.Server]
    private static void SendLookedAtToServer(int entityId)
    {
        var player = ConsoleSystem.Caller.Pawn as GarrywarePlayer;
        var eye = Entity.FindByIndex(entityId) as FloatingEye;
        if (player == null || eye == null || !eye.IsValid)
            return;
        
        eye.PlayerLookedAt?.Invoke(player);
    }
    
}
