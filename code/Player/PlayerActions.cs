using Sandbox;

namespace Garryware;

// Flags to show inputs on the HUD so that people know how to do a specific action
[System.Flags]
public enum PlayerAction
{
    None,
    PrimaryAttack = 1 << 0,
    SecondaryAttack = 1 << 1,
    DropWeapon = 1 << 2,
    PlayerUse = 1 << 3,
    Jump = 1 << 4,
    Sprint = 1 << 5,
    Crouch = 1 << 6,
    ReadyUp = 1 << 7,
}

public static class PlayerActionsExtension
{
    
    public static string AsFriendlyName(this PlayerAction action) => action switch
    {
        PlayerAction.None => "None",
        PlayerAction.PrimaryAttack => "Fire",
        PlayerAction.SecondaryAttack => "Secondary Fire",
        PlayerAction.DropWeapon => "Drop",
        PlayerAction.PlayerUse => "Use",
        PlayerAction.Jump => "Jump",
        PlayerAction.Sprint => "Run",
        PlayerAction.Crouch => "Crouch",
        PlayerAction.ReadyUp => "Ready Up",
        _ => "Unknown Action"
    };

    public static InputButton AsInputButton(this PlayerAction action) => action switch
    {
        PlayerAction.None => 0,
        PlayerAction.PrimaryAttack => InputButton.PrimaryAttack,
        PlayerAction.SecondaryAttack => InputButton.SecondaryAttack,
        PlayerAction.DropWeapon => InputButton.Drop,
        PlayerAction.PlayerUse => InputButton.Use,
        PlayerAction.Jump => InputButton.Jump,
        PlayerAction.Sprint => InputButton.Run,
        PlayerAction.Crouch => InputButton.Duck,
        PlayerAction.ReadyUp => InputButton.Flashlight,
        _ => 0
    };
    
}
