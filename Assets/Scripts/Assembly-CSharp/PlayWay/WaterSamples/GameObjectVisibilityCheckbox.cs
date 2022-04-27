using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.WaterSamples
{
	public class GameObjectVisibilityCheckbox : MonoBehaviour
	{
		[SerializeField]
		private GameObject target;

		private void Awake()
		{
			Toggle component = GetComponent<Toggle>();
			component.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnValueChanged(bool value)
		{
			target.gameObject.SetActive(value);
		}
	}
}
