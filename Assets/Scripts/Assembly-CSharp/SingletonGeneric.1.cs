using UnityEngine;

public class SingletonGeneric<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T singleton;

	public static T Singleton
	{
		get
		{
			if ((Object)singleton == (Object)null)
			{
				singleton = Object.FindObjectOfType<T>();
			}
			else if ((Object)singleton != (Object)Object.FindObjectOfType<T>())
			{
				Object.Destroy(Object.FindObjectOfType<T>());
			}
			return singleton;
		}
	}
}
