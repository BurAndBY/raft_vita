using UnityEditor;

public class SetPlatformOptionsPSP2
{
	[MenuItem("PSVita Tools/Set Player Settings For PS Vita")]
	// Use this for initialization
	static void SetOptions()
	{
		// Param file settings.
		PlayerSettings.PSVita.paramSfxPath = "Assets/Editor/SonyVitaAdditionalContentPublishData/param.sfx";

		// Package settings.
		PlayerSettings.PSVita.packagePassword = "7Y8mR4qlSL6qQ7IlF9z2wurxxZvZjhpt";
		PlayerSettings.PSVita.keystoneFile = "Assets/Editor/SonyVitaAdditionalContentPublishData/keystone";
	}
}
