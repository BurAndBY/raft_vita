using PlayWay.Water;
using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.WaterSamples
{
	public class QualityLevelDropdown : MonoBehaviour
	{
		private void Awake()
		{
			Dropdown component = GetComponent<Dropdown>();
			component.value = WaterQualitySettings.Instance.GetQualityLevel();
			component.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnValueChanged(int index)
		{
			WaterQualitySettings.Instance.SetQualityLevel(index);
		}
	}
}
