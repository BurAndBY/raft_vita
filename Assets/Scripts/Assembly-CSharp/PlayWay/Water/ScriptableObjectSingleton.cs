using UnityEngine;

namespace PlayWay.Water
{
	public class ScriptableObjectSingleton : ScriptableObject
	{
		protected static T LoadSingleton<T>() where T : ScriptableObject
		{
			return Resources.Load<T>(typeof(T).Name);
		}
	}
}
