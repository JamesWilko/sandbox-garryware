using Sandbox;

namespace Garryware.Utilities;

// @todo: make an editor utility for checking these at editor time rather than run time
public static class Validation
{

	public static void Exists(GameObject go, string message = null)
	{
		if (go.IsValid())
			return;

		message ??= "A GameObject reference was null and will cause errors!";
		Log.Error($"[Validation] {message}");
	}

	public static void PrefabExists(GameObject prefab, string prefabName)
	{
		if (prefab.IsValid())
			return;
		
		Log.Error($"[Validation] Prefab reference '{prefabName}' was null, this is going to cause errors!");
	}
	
}
