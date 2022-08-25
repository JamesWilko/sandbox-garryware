using Sandbox;

namespace Garryware;

// Flags to show inputs on the HUD so that people know how to do a specific action
[System.Flags]
public enum PlayerAction
{
    None,
    UseWeapon = 1 << 0,
    DropWeapon = 1 << 1,
    PlayerUse = 1 << 2,
    Jump = 1 << 3,
    Sprint = 1 << 4,
    Crouch = 1 << 5,
}

public static class PlayerActionsExtension
{
    
    public static string GetFriendlyName(this PlayerAction action) => action switch
    {
        PlayerAction.None => "None",
        PlayerAction.UseWeapon => "Shoot",
        PlayerAction.DropWeapon => "Drop",
        PlayerAction.PlayerUse => "Use",
        PlayerAction.Jump => "Jump",
        PlayerAction.Sprint => "Run",
        PlayerAction.Crouch => "Crouch",
        _ => "Unknown Action"
    };
    
    public static Texture GetInputGlyph(this PlayerAction action) => action switch
    {
        PlayerAction.None => null,
        PlayerAction.UseWeapon => Input.GetGlyph(InputButton.PrimaryAttack),
        PlayerAction.DropWeapon => Input.GetGlyph(InputButton.Drop),
        PlayerAction.PlayerUse => Input.GetGlyph(InputButton.Use),
        PlayerAction.Jump => Input.GetGlyph(InputButton.Jump),
        PlayerAction.Sprint => Input.GetGlyph(InputButton.Run),
        PlayerAction.Crouch => Input.GetGlyph(InputButton.Duck),
        _ => null
    };

}
