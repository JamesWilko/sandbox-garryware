using Sandbox;
using SandboxEditor;

namespace Garryware.Entities;

public enum RoomSize
{
    Small, // 0 - 6 players
    Medium, // 6 - 12 players
    Large // 13+ players
}

public enum RoomType
{
    Empty,
    Boxes,
    Platform
}

[Library("gw_trigger_room"), HammerEntity]
[Title("Garryware Room Extents")]
public class GarrywareRoom : BaseTrigger
{

    public RoomSize Size;
    public RoomType Type;

}
