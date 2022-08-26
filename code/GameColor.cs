using System;

namespace Garryware;

public enum GameColor
{
    White,
    Red,
    Blue,
    Green,
    Magenta,
    Yellow,
    Cyan,
    Black
}

public static class ColorsExtension
{

    public static string AsName(this GameColor gameColor) => gameColor switch
    {
        GameColor.White => "Uncolored",
        GameColor.Red => "Red",
        GameColor.Blue => "Blue",
        GameColor.Green => "Green",
        GameColor.Magenta => "Pink",
        GameColor.Yellow => "Yellow",
        GameColor.Cyan => "Cyan",
        GameColor.Black => "Black",
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public static Color AsColor(this GameColor gameColor) => gameColor switch
    {
        GameColor.White => Color.White,
        GameColor.Red => Color.Red,
        GameColor.Blue => Color.Blue,
        GameColor.Green => Color.Green,
        GameColor.Magenta => Color.Magenta,
        GameColor.Yellow => Color.Yellow,
        GameColor.Cyan => new Color(0.1f, 0.8f, 1.0f),
        GameColor.Black => Color.Black,
        _ => throw new ArgumentOutOfRangeException()
    };
    
}
