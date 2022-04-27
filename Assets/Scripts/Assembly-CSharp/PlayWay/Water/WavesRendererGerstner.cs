using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WavesRendererGerstner
	{
		[SerializeField]
		[Range(0f, 20f)]
		private int numGerstners = 20;

		private Water water;

		private WindWaves windWaves;

		private Gerstner4[] gerstnerFours;

		private int lastUpdateFrame;

		private bool enabled;

		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		internal void Enable(WindWaves windWaves)
		{
			if (!enabled)
			{
				enabled = true;
				water = windWaves.GetComponent<Water>();
				this.windWaves = windWaves;
				if (Application.isPlaying)
				{
					water.ProfilesChanged.AddListener(OnProfilesChanged);
					FindBestWaves();
				}
			}
		}

		internal void Disable()
		{
			if (enabled)
			{
				enabled = false;
				if (water != null)
				{
					water.InvalidateMaterialKeywords();
				}
			}
		}

		internal void OnValidate(WindWaves windWaves)
		{
			if (enabled)
			{
				FindBestWaves();
			}
		}

		private void FindBestWaves()
		{
			gerstnerFours = windWaves.SpectrumResolver.FindGerstners(numGerstners, false);
			UpdateMaterial();
		}

		private void UpdateMaterial()
		{
			Material waterMaterial = water.WaterMaterial;
			Vector4 value = default(Vector4);
			Vector4 value2 = default(Vector4);
			Vector4 value3 = default(Vector4);
			Vector4 value4 = default(Vector4);
			for (int i = 0; i < gerstnerFours.Length; i++)
			{
				Gerstner4 gerstner = gerstnerFours[i];
				value.x = gerstner.wave0.amplitude;
				value2.x = gerstner.wave0.frequency;
				value3.x = gerstner.wave0.direction.x;
				value3.y = gerstner.wave0.direction.y;
				value.y = gerstner.wave1.amplitude;
				value2.y = gerstner.wave1.frequency;
				value3.z = gerstner.wave1.direction.x;
				value3.w = gerstner.wave1.direction.y;
				value.z = gerstner.wave2.amplitude;
				value2.z = gerstner.wave2.frequency;
				value4.x = gerstner.wave2.direction.x;
				value4.y = gerstner.wave2.direction.y;
				value.w = gerstner.wave3.amplitude;
				value2.w = gerstner.wave3.frequency;
				value4.z = gerstner.wave3.direction.x;
				value4.w = gerstner.wave3.direction.y;
				waterMaterial.SetVector("_GrAB" + i, value3);
				waterMaterial.SetVector("_GrCD" + i, value4);
				waterMaterial.SetVector("_GrAmp" + i, value);
				waterMaterial.SetVector("_GrFrq" + i, value2);
			}
			for (int j = gerstnerFours.Length; j < 5; j++)
			{
				waterMaterial.SetVector("_GrAmp" + j, Vector4.zero);
			}
		}

		public void OnWaterRender(Camera camera)
		{
			if (Application.isPlaying && enabled)
			{
				UpdateWaves();
			}
		}

		public void OnWaterPostRender(Camera camera)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WindWaves windWaves, WaterQualityLevel qualityLevel)
		{
			variant.SetUnityKeyword("_WAVES_GERSTNER", enabled);
		}

		private void UpdateWaves()
		{
			int frameCount = Time.frameCount;
			if (lastUpdateFrame != frameCount)
			{
				lastUpdateFrame = frameCount;
				Material waterMaterial = water.WaterMaterial;
				float time = Time.time;
				Vector4 value = default(Vector4);
				for (int i = 0; i < gerstnerFours.Length; i++)
				{
					Gerstner4 gerstner = gerstnerFours[i];
					value.x = gerstner.wave0.offset + gerstner.wave0.speed * time;
					value.y = gerstner.wave1.offset + gerstner.wave1.speed * time;
					value.z = gerstner.wave2.offset + gerstner.wave2.speed * time;
					value.w = gerstner.wave3.offset + gerstner.wave3.speed * time;
					waterMaterial.SetVector("_GrOff" + i, value);
				}
			}
		}

		private void OnProfilesChanged(Water water)
		{
			FindBestWaves();
		}
	}
}
