namespace Garryware.Entities;

public class BiggerPlatform : Platform
{
    public override void Spawn()
    {
        base.Spawn();
        Scale = 16.0f;
    }
}
