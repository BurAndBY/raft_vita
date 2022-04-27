using UnityEngine;

public class NightLightController : MonoBehaviour
{
	public Light[] lights;

	public float nightIntensity;

	public float dayIntensity;

	private const float nightTimeStart = 0.8f;

	private const float nightTimeEnd = 0.1f;

	private void Update()
	{
		if (!(GameManager.singleton == null))
		{
			float num = GameManager.singleton.skyController.TIME_of_DAY / 24f;
			if (num >= 0.8f || num <= 0.1f)
			{
				SetLightIntensity(nightIntensity);
			}
			else
			{
				SetLightIntensity(dayIntensity);
			}
		}
	}

	private void SetLightIntensity(float targetValue)
	{
		Light[] array = lights;
		foreach (Light light in array)
		{
			light.intensity = Mathf.MoveTowards(light.intensity, targetValue, Time.deltaTime * 0.1f);
		}
	}
}
