using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.WaterSamples
{
	public class FogCheckbox : MonoBehaviour
	{
		private void Awake()
		{
			Toggle component = GetComponent<Toggle>();
			component.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnValueChanged(bool value)
		{
			RenderSettings.fog = value;
		}
	}
}
