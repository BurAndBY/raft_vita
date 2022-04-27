using UnityEngine;

[ExecuteInEditMode]
public class MoonShaderExempleScene : MonoBehaviour
{
	public Transform camPivot;

	public Transform lightCaster;

	public Material moonMaterial;

	public float RotationSpeed = 5f;

	private void Update()
	{
		moonMaterial.SetVector("_SunDir", -lightCaster.transform.forward);
		moonMaterial.SetFloat("_LightIntensity", 1.5f);
		moonMaterial.SetColor("_MoonColor", Color.white);
		if (Application.isPlaying)
		{
			if (Input.GetMouseButton(0))
			{
				lightCaster.Rotate(0f, 0f - Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime, 0f - Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime, Space.World);
			}
			camPivot.Rotate(0f, Input.GetAxis("Horizontal") * (RotationSpeed * 0.5f) * Time.deltaTime, 0f);
		}
	}
}
