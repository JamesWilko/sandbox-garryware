using Sandbox;

namespace Garryware;

public struct GravityGunInfo
{
    public Entity Target;
    public Entity Weapon;
    public Entity Pawn;
    public IClient Instigator;
}

public interface IGravityGunCallback
{ 
    public void OnGravityGunPickedUp(GravityGunInfo info);
    public void OnGravityGunDropped(GravityGunInfo info);
    public void OnGravityGunPunted(GravityGunInfo info);
}
