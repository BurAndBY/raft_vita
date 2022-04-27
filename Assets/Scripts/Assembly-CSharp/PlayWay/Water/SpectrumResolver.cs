using System;
using UnityEngine;

namespace PlayWay.Water
{
	public class SpectrumResolver : SpectrumResolverCPU
	{
		public enum SpectrumType
		{
			Height,
			Slope,
			Displacement,
			RawDirectional,
			RawOmnidirectional
		}

		private Texture2D tileSizeLookup;

		private Texture omnidirectionalSpectrum;

		private RenderTexture totalOmnidirectionalSpectrum;

		private RenderTexture directionalSpectrum;

		private RenderTexture heightSpectrum;

		private RenderTexture slopeSpectrum;

		private RenderTexture displacementSpectrum;

		private RenderBuffer[] renderTargetsx2;

		private RenderBuffer[] renderTargetsx3;

		private float renderTime;

		private int renderTimeId;

		private int resolution;

		private bool tileSizesLookupDirty = true;

		private bool directionalSpectrumDirty = true;

		private Vector4 tileSizes;

		private Material animationMaterial;

		private Water water;

		private WindWaves windWaves;

		public Texture TileSizeLookup
		{
			get
			{
				ValidateTileSizeLookup();
				return tileSizeLookup;
			}
		}

		public float RenderTime
		{
			get
			{
				return renderTime;
			}
		}

		public SpectrumResolver(WindWaves windWaves, Shader spectrumShader)
			: base(windWaves, 4)
		{
			water = windWaves.GetComponent<Water>();
			this.windWaves = windWaves;
			renderTimeId = Shader.PropertyToID("_RenderTime");
			animationMaterial = new Material(spectrumShader);
			animationMaterial.hideFlags = HideFlags.DontSave;
			animationMaterial.SetFloat(renderTimeId, Time.time);
		}

		public Texture RenderHeightSpectrumAt(float time)
		{
			CheckResources();
			RenderTexture rawDirectionalSpectrum = GetRawDirectionalSpectrum();
			renderTime = time;
			animationMaterial.SetFloat(renderTimeId, time);
			Graphics.Blit(rawDirectionalSpectrum, heightSpectrum, animationMaterial, 0);
			return heightSpectrum;
		}

		public Texture RenderSlopeSpectrumAt(float time)
		{
			CheckResources();
			RenderTexture rawDirectionalSpectrum = GetRawDirectionalSpectrum();
			renderTime = time;
			animationMaterial.SetFloat(renderTimeId, time);
			Graphics.Blit(rawDirectionalSpectrum, slopeSpectrum, animationMaterial, 1);
			return slopeSpectrum;
		}

		public void RenderDisplacementsSpectraAt(float time, out Texture height, out Texture displacement)
		{
			CheckResources();
			height = heightSpectrum;
			displacement = displacementSpectrum;
			renderTargetsx2[0] = heightSpectrum.colorBuffer;
			renderTargetsx2[1] = displacementSpectrum.colorBuffer;
			RenderTexture rawDirectionalSpectrum = GetRawDirectionalSpectrum();
			renderTime = time;
			animationMaterial.SetFloat(renderTimeId, time);
			Graphics.SetRenderTarget(renderTargetsx2, heightSpectrum.depthBuffer);
			Graphics.Blit(rawDirectionalSpectrum, animationMaterial, 5);
			Graphics.SetRenderTarget(null);
		}

		public void RenderCompleteSpectraAt(float time, out Texture height, out Texture slope, out Texture displacement)
		{
			CheckResources();
			height = heightSpectrum;
			slope = slopeSpectrum;
			displacement = displacementSpectrum;
			renderTargetsx3[0] = heightSpectrum.colorBuffer;
			renderTargetsx3[1] = slopeSpectrum.colorBuffer;
			renderTargetsx3[2] = displacementSpectrum.colorBuffer;
			RenderTexture rawDirectionalSpectrum = GetRawDirectionalSpectrum();
			renderTime = time;
			animationMaterial.SetFloat(renderTimeId, time);
			Graphics.SetRenderTarget(renderTargetsx3, heightSpectrum.depthBuffer);
			Graphics.Blit(rawDirectionalSpectrum, animationMaterial, 2);
			Graphics.SetRenderTarget(null);
		}

		public Texture GetSpectrum(SpectrumType type)
		{
			switch (type)
			{
			case SpectrumType.Height:
				return heightSpectrum;
			case SpectrumType.Slope:
				return slopeSpectrum;
			case SpectrumType.Displacement:
				return displacementSpectrum;
			case SpectrumType.RawDirectional:
				return directionalSpectrum;
			case SpectrumType.RawOmnidirectional:
				return omnidirectionalSpectrum;
			default:
				throw new InvalidOperationException();
			}
		}

		internal override void OnProfilesChanged()
		{
			base.OnProfilesChanged();
			if (tileSizes != windWaves.TileSizes)
			{
				tileSizesLookupDirty = true;
				tileSizes = windWaves.TileSizes;
			}
			RenderTotalOmnidirectionalSpectrum();
		}

		private void RenderTotalOmnidirectionalSpectrum()
		{
			animationMaterial.SetFloat("_Gravity", water.Gravity);
			animationMaterial.SetVector("_TargetResolution", new Vector4(windWaves.FinalResolution, windWaves.FinalResolution, 0f, 0f));
			Water.WeightedProfile[] profiles = water.Profiles;
			if (profiles.Length > 1)
			{
				RenderTexture renderTexture = GetTotalOmnidirectionalSpectrum();
				Graphics.SetRenderTarget(renderTexture);
				GL.Clear(false, true, Color.black);
				Graphics.SetRenderTarget(null);
				Water.WeightedProfile[] array = profiles;
				for (int i = 0; i < array.Length; i++)
				{
					Water.WeightedProfile weightedProfile = array[i];
					if (!(weightedProfile.weight <= 0.0001f))
					{
						WaterWavesSpectrum spectrum = weightedProfile.profile.Spectrum;
						WaterWavesSpectrumData value;
						if (!spectraDataCache.TryGetValue(spectrum, out value))
						{
							value = GetSpectrumData(spectrum);
						}
						animationMaterial.SetFloat("_Weight", value.Weight);
						Graphics.Blit(value.Texture, renderTexture, animationMaterial, 4);
					}
				}
				omnidirectionalSpectrum = renderTexture;
			}
			else
			{
				WaterWavesSpectrum spectrum2 = profiles[0].profile.Spectrum;
				WaterWavesSpectrumData value2;
				if (!spectraDataCache.TryGetValue(spectrum2, out value2))
				{
					value2 = GetSpectrumData(spectrum2);
				}
				value2.Weight = 1f;
				omnidirectionalSpectrum = value2.Texture;
			}
			water.WaterMaterial.SetFloat("_MaxDisplacement", base.MaxHorizontalDisplacement);
		}

		protected override void InvalidateDirectionalSpectrum()
		{
			base.InvalidateDirectionalSpectrum();
			directionalSpectrumDirty = true;
		}

		private void RenderDirectionalSpectrum()
		{
			if (omnidirectionalSpectrum == null)
			{
				RenderTotalOmnidirectionalSpectrum();
			}
			ValidateTileSizeLookup();
			animationMaterial.SetFloat("_Directionality", 1f - water.Directionality);
			animationMaterial.SetVector("_WindDirection", base.WindDirection);
			animationMaterial.SetTexture("_TileSizeLookup", tileSizeLookup);
			Graphics.Blit(omnidirectionalSpectrum, directionalSpectrum, animationMaterial, 3);
			directionalSpectrumDirty = false;
		}

		internal RenderTexture GetRawDirectionalSpectrum()
		{
			if ((directionalSpectrumDirty || !directionalSpectrum.IsCreated()) && Application.isPlaying)
			{
				CheckResources();
				RenderDirectionalSpectrum();
			}
			return directionalSpectrum;
		}

		private RenderTexture GetTotalOmnidirectionalSpectrum()
		{
			if (totalOmnidirectionalSpectrum == null)
			{
				int num = windWaves.FinalResolution << 1;
				totalOmnidirectionalSpectrum = new RenderTexture(num, num, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				totalOmnidirectionalSpectrum.hideFlags = HideFlags.DontSave;
				totalOmnidirectionalSpectrum.filterMode = FilterMode.Point;
				totalOmnidirectionalSpectrum.wrapMode = TextureWrapMode.Repeat;
			}
			return totalOmnidirectionalSpectrum;
		}

		private void CheckResources()
		{
			if (heightSpectrum == null)
			{
				int num = windWaves.FinalResolution << 1;
				bool finalHighPrecision = windWaves.FinalHighPrecision;
				heightSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
				heightSpectrum.hideFlags = HideFlags.DontSave;
				heightSpectrum.filterMode = FilterMode.Point;
				slopeSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				slopeSpectrum.hideFlags = HideFlags.DontSave;
				slopeSpectrum.filterMode = FilterMode.Point;
				displacementSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				displacementSpectrum.hideFlags = HideFlags.DontSave;
				displacementSpectrum.filterMode = FilterMode.Point;
				directionalSpectrum = new RenderTexture(num, num, 0, (!finalHighPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
				directionalSpectrum.hideFlags = HideFlags.DontSave;
				directionalSpectrum.filterMode = FilterMode.Point;
				directionalSpectrum.wrapMode = TextureWrapMode.Clamp;
				renderTargetsx2 = new RenderBuffer[2] { heightSpectrum.colorBuffer, displacementSpectrum.colorBuffer };
				renderTargetsx3 = new RenderBuffer[3] { heightSpectrum.colorBuffer, slopeSpectrum.colorBuffer, displacementSpectrum.colorBuffer };
			}
		}

		internal override void OnMapsFormatChanged(bool resolution)
		{
			base.OnMapsFormatChanged(resolution);
			if (totalOmnidirectionalSpectrum != null)
			{
				UnityEngine.Object.Destroy(totalOmnidirectionalSpectrum);
				totalOmnidirectionalSpectrum = null;
			}
			if (heightSpectrum != null)
			{
				UnityEngine.Object.Destroy(heightSpectrum);
				heightSpectrum = null;
			}
			if (slopeSpectrum != null)
			{
				UnityEngine.Object.Destroy(slopeSpectrum);
				slopeSpectrum = null;
			}
			if (displacementSpectrum != null)
			{
				UnityEngine.Object.Destroy(displacementSpectrum);
				displacementSpectrum = null;
			}
			if (directionalSpectrum != null)
			{
				UnityEngine.Object.Destroy(directionalSpectrum);
				directionalSpectrum = null;
			}
			omnidirectionalSpectrum = null;
			renderTargetsx2 = null;
			renderTargetsx3 = null;
		}

		private void ValidateTileSizeLookup()
		{
			if (tileSizeLookup == null)
			{
				tileSizeLookup = new Texture2D(2, 2, (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat)) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat, false, true);
				tileSizeLookup.hideFlags = HideFlags.DontSave;
				tileSizeLookup.wrapMode = TextureWrapMode.Clamp;
				tileSizeLookup.filterMode = FilterMode.Point;
			}
			if (tileSizesLookupDirty)
			{
				tileSizeLookup.SetPixel(0, 0, new Color(0.5f, 0.5f, 1f / tileSizes.x, 0f));
				tileSizeLookup.SetPixel(1, 0, new Color(1.5f, 0.5f, 1f / tileSizes.y, 0f));
				tileSizeLookup.SetPixel(0, 1, new Color(0.5f, 1.5f, 1f / tileSizes.z, 0f));
				tileSizeLookup.SetPixel(1, 1, new Color(1.5f, 1.5f, 1f / tileSizes.w, 0f));
				tileSizeLookup.Apply(false, false);
				tileSizesLookupDirty = false;
			}
		}
	}
}
