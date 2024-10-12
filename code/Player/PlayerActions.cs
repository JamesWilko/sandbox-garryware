using System;

namespace Garryware;


[Flags] // @note: use flags to show inputs on the HUD so that people know how to do a specific action
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
	Punt = 1 << 9, // @note: this should probably be a name override on the weapon, but we've only got one of these atm
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
		_ => throw new ArgumentOutOfRangeException()
	};

	public static string AsInputAction(this PlayerAction action) => action switch
	{
		PlayerAction.None => "",
		PlayerAction.PrimaryAttack => "attack1",
		PlayerAction.SecondaryAttack => "attack2",
		PlayerAction.DropWeapon => "Drop",
		PlayerAction.PlayerUse => "Use",
		PlayerAction.Jump => "Jump",
		PlayerAction.Sprint => "Run",
		PlayerAction.Crouch => "Duck",
		PlayerAction.ReadyUp => "ReadyUp",
		PlayerAction.Reload => "Reload",
		PlayerAction.Punt => "attack1",
		_ => throw new ArgumentOutOfRangeException()
	};
    
}
