using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.Water.Samples
{
	public class PresetDropdown : MonoBehaviour
	{
		[SerializeField]
		private Water water;

		[SerializeField]
		private WaterProfile[] profiles;

		[SerializeField]
		private Dropdown dropdown;

		[SerializeField]
		private Slider progressSlider;

		private WaterProfile sourceProfile;

		private WaterProfile targetProfile;

		private float changeTime = float.NaN;

		private void Start()
		{
			dropdown.onValueChanged.AddListener(OnValueChanged);
			if (water.Profiles == null)
			{
				base.enabled = false;
			}
			else
			{
				targetProfile = water.Profiles[0].profile;
			}
		}

		public void SkipPresetTransition()
		{
			changeTime = -100f;
		}

		private void Update()
		{
			if (!float.IsNaN(changeTime))
			{
				float num = Mathf.Clamp01((Time.time - changeTime) / 30f);
				water.SetProfiles(new Water.WeightedProfile(sourceProfile, 1f - num), new Water.WeightedProfile(targetProfile, num));
				progressSlider.value = num;
				if (num == 1f)
				{
					num = float.NaN;
					changeTime = float.NaN;
					progressSlider.transform.parent.gameObject.SetActive(false);
					dropdown.interactable = true;
				}
			}
		}

		private void OnValueChanged(int index)
		{
			sourceProfile = targetProfile;
			targetProfile = profiles[index];
			changeTime = Time.time;
			progressSlider.transform.parent.gameObject.SetActive(true);
			dropdown.interactable = false;
		}
	}
}
