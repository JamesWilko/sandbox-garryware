using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Garryware.Entities;

public enum RoomSize
{
    Small, // 0 - 6 players
    Medium, // 6 - 12 players
    Large // 13+ players
}

[Library("gw_trigger_room")]
[Title("Garryware Room Extents")]
public class GarrywareRoom : Trigger
{
    [Property]
    public RoomSize Size { get; set; } = RoomSize.Small;

    [Property]
    public MicrogameRoom Contents { get; set; } = MicrogameRoom.Empty;
    
    public List<SpawnPoint> SpawnPoints { get; private set; }
    public List<OnBoxTrigger> OnBoxTriggers { get; private set; } 
    public List<OnBoxSpawn> OnBoxSpawns { get; private set; } 
    public List<AboveBoxSpawn> AboveBoxSpawns { get; private set; } 
    public List<InAirSpawn> InAirSpawns { get; private set; } 
    public List<OnFloorSpawn> OnFloorSpawns { get; private set; } 
    
    public ShuffledDeck<SpawnPoint> SpawnPointsDeck { get; private set; }
    public ShuffledDeck<OnBoxSpawn> OnBoxSpawnsDeck { get; private set; }
    public ShuffledDeck<AboveBoxSpawn> AboveBoxSpawnsDeck { get; private set; }
    public ShuffledDeck<InAirSpawn> InAirSpawnsDeck { get; private set; }
    public ShuffledDeck<OnFloorSpawn> OnFloorSpawnsDeck { get; private set; }

    public override void Spawn()
    {
        base.Spawn();
        
        // Build the lists of all the spawn points and garryware entities that are in this specific room
        SpawnPoints = FindEntitiesInThisRoom<SpawnPoint>();
        OnBoxTriggers = FindEntitiesInThisRoom<OnBoxTrigger>();
        OnBoxSpawns = FindEntitiesInThisRoom<OnBoxSpawn>();
        AboveBoxSpawns = FindEntitiesInThisRoom<AboveBoxSpawn>();
        InAirSpawns = FindEntitiesInThisRoom<InAirSpawn>();
        OnFloorSpawns = FindEntitiesInThisRoom<OnFloorSpawn>();
        
        // Build decks for the entities
        SpawnPointsDeck = new ShuffledDeck<SpawnPoint>(SpawnPoints);
        OnBoxSpawnsDeck = new ShuffledDeck<OnBoxSpawn>(OnBoxSpawns);
        AboveBoxSpawnsDeck = new ShuffledDeck<AboveBoxSpawn>(AboveBoxSpawns);
        InAirSpawnsDeck = new ShuffledDeck<InAirSpawn>(InAirSpawns);
        OnFloorSpawnsDeck = new ShuffledDeck<OnFloorSpawn>(OnFloorSpawns);
    }

    private List<T> FindEntitiesInThisRoom<T>() where T : Entity
    {
        return Entity.All.OfType<T>().Where(ContainsEntity).ToList();
    }

    public void ShuffleDecks()
    {
        SpawnPointsDeck.Shuffle();
        OnBoxSpawnsDeck.Shuffle();
        AboveBoxSpawnsDeck.Shuffle();
        InAirSpawnsDeck.Shuffle();
        OnFloorSpawnsDeck.Shuffle();
    }
    
}
