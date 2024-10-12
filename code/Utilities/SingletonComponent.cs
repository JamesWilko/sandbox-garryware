using System.Collections.Generic;
using Sandbox;

namespace Garryware.Utilities;

public abstract class SingletonComponent<T> : Component, IHotloadManaged where T : SingletonComponent<T>
{
	public static T Instance { get; private set; }

	protected override void OnAwake()
	{
		if (Active)
		{
			Instance = (T)this;
			Log.Info($"Setup {typeof(T).Name} singleton");
		}
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Log.Info($"Destroyed {typeof(T).Name} singleton");
			Instance = null;
		}
	}
	
	void IHotloadManaged.Created(IReadOnlyDictionary<string, object> state)
	{
		if (state.GetValueOrDefault($"{typeof(T).Name}_IsActive") is true)
		{
			Instance = (T)this;
		}
	}
	
	void IHotloadManaged.Destroyed(Dictionary<string, object> state)
	{
		state[$"{typeof(T).Name}_IsActive"] = Instance == this;
	}
}
