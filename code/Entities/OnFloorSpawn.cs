using Sandbox;

namespace Garryware.Entities;

public enum FloorGridColor
{
    Light,
    Dark
}

/// <summary>
/// A spawn point for entities on the floor of the map.
/// </summary>
[Library("gw_spawn_on_floor")]
[Title("On Floor Spawn")]
public partial class OnFloorSpawn : Entity
{
    [Property]
    public FloorGridColor GridColor { get; set; } = FloorGridColor.Light;
}
