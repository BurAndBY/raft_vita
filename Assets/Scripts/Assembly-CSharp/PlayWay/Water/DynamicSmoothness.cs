using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class DynamicSmoothness
	{
		[SerializeField]
		private ComputeShader varianceShader;

		[Tooltip("Incorporates tiny waves on the screen into Unity's shader micro-facet model. Makes water look realistic at all view distances. Recommended.\nUsed only on DX11.")]
		[SerializeField]
		private bool enabled = true;

		private RenderTexture varianceTexture;

		private int lastResetIndex;

		private int currentIndex;

		private bool finished;

		private bool initialized;

		private bool supported = true;

		private WindWaves windWaves;

		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		public Texture VarianceTexture
		{
			get
			{
				return varianceTexture;
			}
		}

		public ComputeShader ComputeShader
		{
			get
			{
				return varianceShader;
			}
			set
			{
				varianceShader = value;
			}
		}

		public void Start(WindWaves windWaves)
		{
			this.windWaves = windWaves;
			supported = CheckSupport();
			OnCopyModeChanged();
			OnValidate(windWaves);
		}

		public void FreeResources()
		{
			if (varianceTexture != null)
			{
				varianceTexture.Destroy();
				varianceTexture = null;
			}
		}

		public void OnCopyModeChanged()
		{
			if (windWaves != null && windWaves.CopyFrom != null)
			{
				FreeResources();
				windWaves.CopyFrom.DynamicSmoothness.ValidateVarianceTextures(windWaves.CopyFrom);
				windWaves.GetComponent<Water>().WaterMaterial.SetTexture("_SlopeVariance", windWaves.CopyFrom.DynamicSmoothness.varianceTexture);
			}
		}

		public bool CheckSupport()
		{
			return SystemInfo.supportsComputeShaders && SystemInfo.supports3DTextures;
		}

		public void Update()
		{
			if (enabled && supported)
			{
				if (!initialized)
				{
					InitializeVariance();
				}
				ValidateVarianceTextures(windWaves);
				if (!finished)
				{
					RenderNextPixel();
				}
			}
		}

		private void InitializeVariance()
		{
			initialized = true;
			Water component = windWaves.GetComponent<Water>();
			component.ProfilesChanged.AddListener(OnProfilesChanged);
			windWaves.WindDirectionChanged.AddListener(OnWindDirectionChanged);
		}

		private void ValidateVarianceTextures(WindWaves windWaves)
		{
			if (varianceTexture == null)
			{
				varianceTexture = CreateVarianceTexture(RenderTextureFormat.RGHalf);
			}
			if (!varianceTexture.IsCreated())
			{
				varianceTexture.Create();
				Water component = windWaves.GetComponent<Water>();
				component.WaterMaterial.SetTexture("_SlopeVariance", varianceTexture);
				varianceShader.SetTexture(0, "_Variance", varianceTexture);
				lastResetIndex = 0;
				currentIndex = 0;
			}
		}

		private void RenderNextPixel()
		{
			varianceShader.SetInt("_FFTSize", windWaves.FinalResolution);
			varianceShader.SetInt("_FFTSizeHalf", windWaves.FinalResolution >> 1);
			varianceShader.SetFloat("_VariancesSize", varianceTexture.width);
			varianceShader.SetVector("_TileSizes", windWaves.TileSizes);
			varianceShader.SetVector("_Coordinates", new Vector4(currentIndex % 4, (currentIndex >> 2) % 4, currentIndex >> 4, 0f));
			varianceShader.SetTexture(0, "_Spectrum", windWaves.SpectrumResolver.GetRawDirectionalSpectrum());
			varianceShader.Dispatch(0, 1, 1, 1);
			currentIndex++;
			if (currentIndex >= 64)
			{
				currentIndex = 0;
			}
			if (currentIndex == lastResetIndex)
			{
				finished = true;
			}
		}

		private void ResetComputations()
		{
			lastResetIndex = currentIndex;
			finished = false;
		}

		internal void OnValidate(WindWaves windWaves)
		{
		}

		private RenderTexture CreateVarianceTexture(RenderTextureFormat format)
		{
			RenderTexture renderTexture = new RenderTexture(4, 4, 0, format, RenderTextureReadWrite.Linear);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.volumeDepth = 4;
			renderTexture.isVolume = true;
			renderTexture.enableRandomWrite = true;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.filterMode = FilterMode.Bilinear;
			return renderTexture;
		}

		private void OnProfilesChanged(Water water)
		{
			ResetComputations();
		}

		private void OnWindDirectionChanged(WindWaves windWaves)
		{
			ResetComputations();
		}
	}
}
