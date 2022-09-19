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
    Reload = 1 << 8,
    Punt = 1 << 9, // @note: this should probably be a name override on the weapon, but we've only got one override for now
}

public static class PlayerActionsExtension
{
    
    public static string AsFriendlyName(this PlayerAction action) => action switch
    {
        PlayerAction.None => "#action.none",
        PlayerAction.PrimaryAttack => "#action.attack.primary",
        PlayerAction.SecondaryAttack => "#action.attack.secondary",
        PlayerAction.DropWeapon => "#action.drop",
        PlayerAction.PlayerUse => "#action.use",
        PlayerAction.Jump => "#action.jump",
        PlayerAction.Sprint => "#action.sprint",
        PlayerAction.Crouch => "#action.crouch",
        PlayerAction.ReadyUp => "#action.ready-up",
        PlayerAction.Reload => "#action.reload",
        PlayerAction.Punt => "#action.punt",
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
        PlayerAction.Reload => InputButton.Reload,
        PlayerAction.Punt => InputButton.PrimaryAttack,
        _ => 0
    };
    
}
