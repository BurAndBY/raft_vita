using System;
using PlayWay.Water;
using UnityEngine;

namespace PlayWay.WaterSamples
{
	public class SampleScene : MonoBehaviour
	{
		[Serializable]
		public class AmbientGradient
		{
			public Color groundColor;

			public Color equatorColor;

			public Color skyColor;

			public AmbientGradient(Color groundColor, Color equatorColor, Color skyColor)
			{
				this.groundColor = groundColor;
				this.equatorColor = equatorColor;
				this.skyColor = skyColor;
			}
		}

		[SerializeField]
		private PlayWay.Water.Water water;

		[SerializeField]
		private WaterProfile calmShoreWater;

		[SerializeField]
		private WaterProfile calmSeaWater;

		[SerializeField]
		private WaterProfile choppySeaWater;

		[SerializeField]
		private WaterProfile breezeSeaWater;

		[SerializeField]
		private WaterProfile stormSeaWater;

		[SerializeField]
		private ReflectionProbe reflectionProbe;

		[SerializeField]
		private Material galleonMaterial;

		[SerializeField]
		private Light sun;

		[SerializeField]
		private Light galleonLantern;

		[SerializeField]
		private GameObject[] seagulls;

		[SerializeField]
		private AmbientGradient ambient1;

		[SerializeField]
		private AmbientGradient ambient2;

		private WaterProfile source;

		private WaterProfile target;

		private float sourceSunIntensity;

		private float targetSunIntensity;

		private float sourceExposure;

		private float targetExposure;

		private float profileChangeTime = float.PositiveInfinity;

		private float transitionDuration;

		private AmbientGradient sourceAmbient;

		private AmbientGradient targetAmbient;

		private bool environmentDirty;

		private void Start()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			WaterQualitySettings.Instance.Changed -= new Action(OnQualitySettingsChanged);
			WaterQualitySettings.Instance.Changed += new Action(OnQualitySettingsChanged);
			water.CacheProfiles(calmShoreWater, calmSeaWater, choppySeaWater, stormSeaWater, breezeSeaWater);
		}

		public void ChangeProfile0A()
		{
			water.SetProfiles(new PlayWay.Water.Water.WeightedProfile(breezeSeaWater, 1f));
			RenderSettings.skybox = UnityEngine.Object.Instantiate(RenderSettings.skybox);
			sun.transform.RotateAround(Vector3.zero, Vector3.up, 10f);
			RenderSettings.skybox.SetFloat("_Rotation", 290f);
			GameObject[] array = seagulls;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(false);
			}
		}

		public void ChangeProfile0()
		{
			TweenProfiles(choppySeaWater, calmSeaWater, sun.intensity, RenderSettings.skybox.GetFloat("_Exposure"), null, 0.01f);
			RenderSettings.fog = false;
		}

		public void ChangeProfile1()
		{
			TweenProfiles(calmSeaWater, choppySeaWater, 0.75f, 0.78f, ambient1, 2f);
		}

		public void ChangeProfile2()
		{
			TweenProfiles(choppySeaWater, stormSeaWater, 0.55f, 0.54f, ambient2, 2f);
		}

		private void TweenProfiles(WaterProfile source, WaterProfile target, float sunIntensity, float exposure, AmbientGradient ambientGradient, float transitionDuration)
		{
			sourceAmbient = new AmbientGradient(RenderSettings.ambientGroundColor, RenderSettings.ambientEquatorColor, RenderSettings.ambientSkyColor);
			targetAmbient = ambientGradient;
			this.transitionDuration = transitionDuration;
			sourceSunIntensity = sun.intensity;
			targetSunIntensity = sunIntensity;
			sourceExposure = RenderSettings.skybox.GetFloat("_Exposure");
			targetExposure = exposure;
			this.source = source;
			this.target = target;
			water.SetProfiles(new PlayWay.Water.Water.WeightedProfile(source, 1f), new PlayWay.Water.Water.WeightedProfile(target, 0f));
			profileChangeTime = Time.time;
		}

		private void Update()
		{
			if (Time.time >= profileChangeTime)
			{
				float num = (Time.time - profileChangeTime) / transitionDuration;
				if (num > 1f)
				{
					num = 1f;
				}
				water.SetProfiles(new PlayWay.Water.Water.WeightedProfile(source, 1f - num), new PlayWay.Water.Water.WeightedProfile(target, num));
				sun.intensity = Mathf.Lerp(sourceSunIntensity, targetSunIntensity, num);
				RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(sourceExposure, targetExposure, num));
				if (targetAmbient != null)
				{
					RenderSettings.ambientGroundColor = Color.Lerp(sourceAmbient.groundColor, targetAmbient.groundColor, num);
					RenderSettings.ambientEquatorColor = Color.Lerp(sourceAmbient.equatorColor, targetAmbient.equatorColor, num);
					RenderSettings.ambientSkyColor = Color.Lerp(sourceAmbient.skyColor, targetAmbient.skyColor, num);
				}
				if (num != 1f)
				{
					environmentDirty = true;
				}
				else
				{
					profileChangeTime = float.PositiveInfinity;
				}
			}
			if (galleonLantern.isActiveAndEnabled)
			{
				galleonMaterial.SetColor("_EmissionColor", Color.white * galleonLantern.intensity * 3.4f);
			}
			else
			{
				galleonMaterial.SetColor("_EmissionColor", Color.white * 0.01f);
			}
			if (environmentDirty && Time.frameCount % 4 == 0)
			{
				RefreshEnvironment();
			}
		}

		private void OnDestroy()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			WaterQualitySettings.Instance.Changed -= new Action(OnQualitySettingsChanged);
		}

		private void OnQualitySettingsChanged()
		{
			water.CacheProfiles(calmShoreWater, calmSeaWater, choppySeaWater, stormSeaWater, breezeSeaWater);
		}

		private void RefreshEnvironment()
		{
			reflectionProbe.RenderProbe();
			environmentDirty = false;
		}

		private void DisableIMEs()
		{
		}
	}
}
