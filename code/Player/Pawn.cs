using Sandbox;

namespace Garryware;

public class Pawn : Component, Component.INetworkSpawn
{
	private static Pawn Current { get; set; }
	
	/// <summary>
	/// Is this pawn currently being possessed by the local player.
	/// Clientside only.
	/// </summary>
	public bool IsPossessed => Current == this;
	
	/// <summary>
	/// Is this pawn currently being controlled by the local player.
	/// This means we're possessing it and it's not a remote proxy so we're allowed to make changes to it.
	/// </summary>
	public bool IsLocallyControlled => IsPossessed && !IsProxy;
	
	public void OnNetworkSpawn(Connection owner)
	{
		if (IsProxy)
			return;
		
		Possess();
	}
	
	public void Possess()
	{
		// Don't possess the same pawn twice
		if (IsPossessed)
			return;

		if (Current.IsValid())
		{
			Current.Unpossess();
		}
		Current = this;
		OnPossessed();
	}

	public void Unpossess()
	{
		// Can't unpossess a pawn that we're not possessing
		if (!IsPossessed)
			return;
		
		OnUnpossessed();
		Current = null;
	}
	
	protected virtual void OnPossessed()
	{
		// pure virtual
	}

	protected virtual void OnUnpossessed()
	{
		// pure virtual
	}
}
