using System.Collections.Generic;
using Sandbox;

namespace Garryware.Entities;

public partial class ColorRouletteProp : BreakableProp
{
    public List<GameColor> ColorRotation { get; set; }
    public float RotationTime { get; set; }
    
    private TimeSince TimeSinceColorChanged { get; set; }
    
    private bool canRotateColors;
    private int currentColorIndex;

    public delegate void RouletteResultDelegate(GarrywarePlayer player, ColorRouletteProp prop, GameColor color);
    public event RouletteResultDelegate PlayerSentRouletteResult;
    
    public void GenerateNewRotation()
    {
        ColorRotation = new List<GameColor>();
        for (int i = 0; i < CommonEntities.ColorsDeck.Count; ++i)
        {
            ColorRotation.Add(CommonEntities.ColorsDeck.Next());
        }
    }
    
    public void GenerateNewRotation(int maxColors)
    {
        ColorRotation = new List<GameColor>();
        for (int i = 0; i < maxColors; ++i)
        {
            ColorRotation.Add(CommonEntities.ColorsDeck.Next());
        }
    }

    public GameColor GetRandomColorInRotation()
    {
        return Rand.FromList(ColorRotation);
    }
    
    public void StartRoulette()
    {
        Assert.NotNull(ColorRotation);
        canRotateColors = true;
        
        GameColor = ColorRotation[0];
        TimeSinceColorChanged = 0;
    }

    public void StopRoulette()
    {
        canRotateColors = false;
    }
    
    [Event.Tick.Server]
    private void Update()
    {
        if(!IsValid || !canRotateColors) return;
        
        if (TimeSinceColorChanged >= RotationTime)
        {
            currentColorIndex = (currentColorIndex + 1) % ColorRotation.Count;
            GameColor = ColorRotation[currentColorIndex];
            TimeSinceColorChanged = 0;
        }
    }

    // @note: can't send Entity's or GameColor's directly, so convert them into integers for sending,
    // then convert them back on the server to an actual Entity and GameColor
    protected override void TakeClientDamage(DamageInfo info)
    {
        base.TakeClientDamage(info);
        SendClientHitColorToServer(NetworkIdent, (int)GameColor);
    }

    [ConCmd.Server]
    private static void SendClientHitColorToServer(int entityId, int colorValue)
    {
        var player = ConsoleSystem.Caller.Pawn as GarrywarePlayer;
        var rouletteProp = Entity.FindByIndex(entityId) as ColorRouletteProp;
        if (!rouletteProp?.IsValid ?? false) return;
        
        rouletteProp.PlayerSentRouletteResult?.Invoke(player, rouletteProp, (GameColor) colorValue);
    }
}
