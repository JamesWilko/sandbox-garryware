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
    public static Model BeachBall;
    public static Model Ball;
    public static Model Watermelon;
    public static ShuffledDeck<Model> RubbishDeck { get; private set; }

    public static Material WhiteMaterial;
    
    public static void Precache()
    {
        Crate = Model.Load("models/citizen_props/crate01.vmdl");
        Balloon = Model.Load("models/citizen_props/balloonregular01.vmdl");
        BeachBall = Model.Load("models/citizen_props/beachball.vmdl");
        Ball = Model.Load("models/ball/ball.vmdl");
        Watermelon = Model.Load("models/sbox_props/watermelon/watermelon.vmdl");
        
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

        RubbishDeck = new ShuffledDeck<Model>();
        RubbishDeck.Add(Model.Load("models/citizen_props/bathroomsink01.vmdl"), 2);
        RubbishDeck.Add(Model.Load("models/sbox_props/pizza_box/pizza_box.vmdl"), 5);
        RubbishDeck.Add(Model.Load("models/sbox_props/bin/rubbish_bag.vmdl"), 3);
        RubbishDeck.Add(Model.Load("models/sbox_props/burger_box/burger_box.vmdl"), 3);
        RubbishDeck.Add(Model.Load("models/citizen_props/trashbag02.vmdl"), 5);
        RubbishDeck.Shuffle();
    }
    
    public static void ShuffleDecks()
    {
        ColorsDeck.Shuffle();
        RubbishDeck.Shuffle();
    }
    
}
