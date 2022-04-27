using UnityEngine;

[ExecuteInEditMode]
public class OutputDemo : MonoBehaviour
{
	public AzureSky_Controller getOutput;

	private Light thisLight;

	private void Start()
	{
		thisLight = GetComponent<Light>();
	}

	private void Update()
	{
		if ((bool)getOutput)
		{
			thisLight.intensity = getOutput.AzureSkyGetCurveOutput(0);
			thisLight.color = getOutput.AzureSkyGetGradientOutput(0);
		}
	}
}
