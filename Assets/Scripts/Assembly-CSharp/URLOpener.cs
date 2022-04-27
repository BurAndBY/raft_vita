using UnityEngine;

public class URLOpener : MonoBehaviour
{
	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}
}
