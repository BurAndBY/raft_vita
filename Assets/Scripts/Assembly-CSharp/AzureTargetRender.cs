using UnityEngine;

public class AzureTargetRender : MonoBehaviour
{
	public RenderTexture targetRender;

	private void Awake()
	{
		SetTargetTexture();
	}

	private void Update()
	{
	}

	public void SetTargetTexture()
	{
		GetComponent<Camera>().targetTexture = targetRender;
	}
}
