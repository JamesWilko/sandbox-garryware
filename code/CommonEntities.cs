using System.Collections.Generic;
using System.Linq;
using Garryware.Entities;
using Sandbox;

namespace Garryware;

public static class CommonEntities
{

    public static ShuffledDeck<GameColor> ColorsDeck { get; private set; }

    public static Model Crate;
    public static Model Balloon;
    public static Model Ball;
    public static Model Target;

    public static Material WhiteMaterial;
    
    public static void Precache()
    {
        Crate = Model.Load("models/citizen_props/crate01.vmdl");
        Balloon = Model.Load("models/citizen_props/balloonregular01.vmdl");
        Ball = Model.Load("models/citizen_props/beachball.vmdl");
        
        WhiteMaterial = Material.Load("materials/white.vmat");
        
        ColorsDeck = new ShuffledDeck<GameColor>();
        ColorsDeck.Add(GameColor.Red);
        ColorsDeck.Add(GameColor.Blue);
        ColorsDeck.Add(GameColor.Green);
        ColorsDeck.Add(GameColor.Black);
        ColorsDeck.Add(GameColor.Magenta);
        ColorsDeck.Add(GameColor.Yellow);
        ColorsDeck.Add(GameColor.Cyan);
        ColorsDeck.Shuffle();
    }
    
    public static void ShuffleDecks()
    {
        ColorsDeck.Shuffle();
    }
    
}
