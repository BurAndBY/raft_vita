using System;
using UnityEngine;
using UnityEngine.Events;

namespace PlayWay.Water
{
	[AddComponentMenu("Water/Wind Waves", 0)]
	[ExecuteInEditMode]
	public class WindWaves : MonoBehaviour, IWaterDisplacements, IWaterRenderAware
	{
		[Serializable]
		public class WindWavesEvent : UnityEvent<WindWaves>
		{
		}

		[HideInInspector]
		[SerializeField]
		private Shader spectrumShader;

		[SerializeField]
		private Transform windDirectionPointer;

		[SerializeField]
		[Tooltip("Higher values increase quality, but also decrease performance. Directly controls quality of waves, foam and spray.")]
		private int resolution = 256;

		[SerializeField]
		[Tooltip("Determines if 32-bit precision buffers should be used for computations (Default: off). Not supported on most mobile devices. This setting has impact on performance, even on PCs.\n\nTips:\n 1024 and higher - The difference is clearly visible, use this option on higher quality settings.\n 512 or lower - Keep it disabled.")]
		private bool highPrecision = true;

		[SerializeField]
		[Tooltip("Determines how small waves should be considered by the CPU in ongoing computations. Higher values will increase the precision of all wave computations done on CPU (GetHeightAt etc.), but may decrease performance. Most waves in the ocean spectrum have negligible visual impact and may be safely ignored.")]
		private float cpuWaveThreshold = 0.008f;

		[SerializeField]
		[Tooltip("How many waves at most should be considered by the CPU.")]
		private int cpuMaxWaves = 2500;

		[Range(0f, 3f)]
		[Tooltip("Determines final CPU FFT resolution (0 - acceptable, 1 - good, 2 - perfect, 3 - insane).")]
		[SerializeField]
		private int cpuFFTPrecisionBoost = 1;

		[SerializeField]
		[Tooltip("Copying wave spectrum from other fluid will make this instance a lot faster.")]
		private WindWaves copyFrom;

		[SerializeField]
		private WaveSpectrumRenderMode renderMode;

		[SerializeField]
		private WindWavesEvent windDirectionChanged;

		[SerializeField]
		private WindWavesEvent resolutionChanged;

		[SerializeField]
		private WavesRendererFFT waterWavesFFT;

		[SerializeField]
		private WavesRendererGerstner waterWavesGerstner;

		[SerializeField]
		private DynamicSmoothness dynamicSmoothness;

		private Vector4 tileSizeScales = new Vector4(0.79241f, 0.163151f, 3.175131f, 13.731513f);

		private Water water;

		private int finalResolution;

		private bool finalHighPrecision;

		private float windSpeedMagnitude;

		private float tileSize;

		private Vector4 tileSizes;

		private Vector4 unscaledTileSizes;

		private Vector2 windDirection;

		private Vector2 windSpeed;

		private WaveSpectrumRenderMode finalRenderMode;

		private SpectrumResolver spectrumResolver;

		private WindWaves runtimeCopyFrom;

		private Vector2 lastWaterPos;

		private bool initialized;

		private bool windSpeedChanged;

		private int tileSizeId;

		private int tileSizeScalesId;

		private int maxDisplacementId;

		public WindWaves CopyFrom
		{
			get
			{
				return runtimeCopyFrom;
			}
			set
			{
				if (copyFrom != value || runtimeCopyFrom != value)
				{
					copyFrom = value;
					runtimeCopyFrom = value;
					dynamicSmoothness.OnCopyModeChanged();
					waterWavesFFT.OnCopyModeChanged();
				}
			}
		}

		public SpectrumResolver SpectrumResolver
		{
			get
			{
				return (!(copyFrom == null)) ? copyFrom.spectrumResolver : spectrumResolver;
			}
		}

		public WavesRendererFFT WaterWavesFFT
		{
			get
			{
				return waterWavesFFT;
			}
		}

		public WavesRendererGerstner WaterWavesGerstner
		{
			get
			{
				return waterWavesGerstner;
			}
		}

		public DynamicSmoothness DynamicSmoothness
		{
			get
			{
				return dynamicSmoothness;
			}
		}

		public WaveSpectrumRenderMode RenderMode
		{
			get
			{
				return renderMode;
			}
			set
			{
				renderMode = value;
				ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
			}
		}

		public WaveSpectrumRenderMode FinalRenderMode
		{
			get
			{
				return finalRenderMode;
			}
		}

		public Vector4 TileSizes
		{
			get
			{
				return tileSizes;
			}
		}

		public Vector4 UnscaledTileSizes
		{
			get
			{
				return unscaledTileSizes;
			}
		}

		public Vector2 WindSpeed
		{
			get
			{
				return windSpeed;
			}
		}

		public bool WindSpeedChanged
		{
			get
			{
				return windSpeedChanged;
			}
		}

		public Vector2 WindDirection
		{
			get
			{
				return windDirection;
			}
		}

		public Transform WindDirectionPointer
		{
			get
			{
				return windDirectionPointer;
			}
		}

		public WindWavesEvent WindDirectionChanged
		{
			get
			{
				return windDirectionChanged;
			}
		}

		public WindWavesEvent ResolutionChanged
		{
			get
			{
				return resolutionChanged ?? (resolutionChanged = new WindWavesEvent());
			}
		}

		public int Resolution
		{
			get
			{
				return resolution;
			}
			set
			{
				if (resolution != value)
				{
					resolution = value;
					ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
				}
			}
		}

		public int FinalResolution
		{
			get
			{
				return finalResolution;
			}
		}

		public bool FinalHighPrecision
		{
			get
			{
				return finalHighPrecision;
			}
		}

		public bool HighPrecision
		{
			get
			{
				return highPrecision;
			}
		}

		public int CpuMaxWaves
		{
			get
			{
				return cpuMaxWaves;
			}
		}

		public float CpuWaveThreshold
		{
			get
			{
				return cpuWaveThreshold;
			}
		}

		public int CpuFFTPrecisionBoost
		{
			get
			{
				return cpuFFTPrecisionBoost;
			}
		}

		public Vector4 TileSizeScales
		{
			get
			{
				return tileSizeScales;
			}
		}

		public float MaxVerticalDisplacement
		{
			get
			{
				return spectrumResolver.MaxVerticalDisplacement;
			}
		}

		public float MaxHorizontalDisplacement
		{
			get
			{
				return spectrumResolver.MaxHorizontalDisplacement;
			}
		}

		private void Awake()
		{
			runtimeCopyFrom = copyFrom;
			tileSizeId = Shader.PropertyToID("_WaterTileSize");
			tileSizeScalesId = Shader.PropertyToID("_WaterTileSizeScales");
			maxDisplacementId = Shader.PropertyToID("_MaxDisplacement");
			CheckSupport();
			if (!initialized)
			{
				Initialize();
			}
			ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
		}

		private void OnEnable()
		{
			if (!initialized)
			{
				Initialize();
			}
			UpdateWind();
			CreateObjects();
		}

		private void OnDisable()
		{
			if (waterWavesFFT != null)
			{
				waterWavesFFT.Disable();
			}
			if (waterWavesGerstner != null)
			{
				waterWavesGerstner.Disable();
			}
			if (dynamicSmoothness != null)
			{
				dynamicSmoothness.FreeResources();
			}
		}

		private void Start()
		{
			dynamicSmoothness.Start(this);
			if (Application.isPlaying)
			{
				water.ProfilesChanged.AddListener(OnProfilesChanged);
				OnProfilesChanged(water);
			}
		}

		public void OnValidate()
		{
			if (spectrumShader == null)
			{
				spectrumShader = Shader.Find("PlayWay Water/Spectrum/Water Spectrum");
			}
			if (dynamicSmoothness != null)
			{
				dynamicSmoothness.OnValidate(this);
			}
			if (base.isActiveAndEnabled && Application.isPlaying)
			{
				CopyFrom = copyFrom;
			}
			if (spectrumResolver != null)
			{
				ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
				waterWavesFFT.OnValidate(this);
				waterWavesGerstner.OnValidate(this);
				water.OnValidate();
			}
			if (water != null)
			{
				UpdateShaderParams();
			}
		}

		private void Update()
		{
			UpdateWind();
			if (Application.isPlaying && !(runtimeCopyFrom != null))
			{
				spectrumResolver.Update();
				dynamicSmoothness.Update();
				UpdateShaderParams();
			}
		}

		internal void ResolveFinalSettings(WaterQualityLevel quality)
		{
			CreateObjects();
			WaterWavesMode wavesMode = quality.wavesMode;
			if (wavesMode == WaterWavesMode.DisallowAll)
			{
				base.enabled = false;
				return;
			}
			bool flag = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) || SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
			int num = Mathf.Min(resolution, quality.maxSpectrumResolution, SystemInfo.maxTextureSize);
			bool flag2 = highPrecision && quality.allowHighPrecisionTextures && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
			if (renderMode == WaveSpectrumRenderMode.FullFFT && wavesMode == WaterWavesMode.AllowAll && flag)
			{
				finalRenderMode = WaveSpectrumRenderMode.FullFFT;
			}
			else if (renderMode <= WaveSpectrumRenderMode.GerstnerAndFFTSlope && wavesMode <= WaterWavesMode.AllowSlopeFFT && flag)
			{
				finalRenderMode = WaveSpectrumRenderMode.GerstnerAndFFTSlope;
			}
			else
			{
				finalRenderMode = WaveSpectrumRenderMode.Gerstner;
			}
			if (finalResolution != num)
			{
				lock (this)
				{
					finalResolution = num;
					finalHighPrecision = flag2;
					if (spectrumResolver != null)
					{
						spectrumResolver.OnMapsFormatChanged(true);
					}
					if (ResolutionChanged != null)
					{
						ResolutionChanged.Invoke(this);
					}
				}
			}
			else if (finalHighPrecision != flag2)
			{
				lock (this)
				{
					finalHighPrecision = flag2;
					if (spectrumResolver != null)
					{
						spectrumResolver.OnMapsFormatChanged(false);
					}
				}
			}
			switch (finalRenderMode)
			{
			case WaveSpectrumRenderMode.FullFFT:
				waterWavesFFT.RenderedMaps = WavesRendererFFT.MapType.Displacement | WavesRendererFFT.MapType.Slope;
				waterWavesFFT.Enable(this);
				waterWavesGerstner.Disable();
				break;
			case WaveSpectrumRenderMode.GerstnerAndFFTSlope:
				waterWavesFFT.RenderedMaps = WavesRendererFFT.MapType.Slope;
				waterWavesFFT.Enable(this);
				waterWavesGerstner.Enable(this);
				break;
			case WaveSpectrumRenderMode.Gerstner:
				waterWavesFFT.Disable();
				waterWavesGerstner.Enable(this);
				break;
			}
		}

		private void UpdateShaderParams()
		{
			float num = tileSize * water.UniformWaterScale;
			tileSizes.x = num * tileSizeScales.x;
			tileSizes.y = num * tileSizeScales.y;
			tileSizes.z = num * tileSizeScales.z;
			tileSizes.w = num * tileSizeScales.w;
			water.SetVectorDirect(tileSizeId, tileSizes);
		}

		private void OnProfilesChanged(Water water)
		{
			tileSize = 0f;
			windSpeedMagnitude = 0f;
			Water.WeightedProfile[] profiles = water.Profiles;
			for (int i = 0; i < profiles.Length; i++)
			{
				Water.WeightedProfile weightedProfile = profiles[i];
				WaterProfile profile = weightedProfile.profile;
				float weight = weightedProfile.weight;
				tileSize += profile.TileSize * profile.TileScale * weight;
				windSpeedMagnitude += profile.WindSpeed * weight;
			}
			WaterQualitySettings instance = WaterQualitySettings.Instance;
			tileSize *= instance.TileSizeScale;
			unscaledTileSizes = tileSize * tileSizeScales;
			float num = tileSize * water.UniformWaterScale;
			tileSizes.x = num * tileSizeScales.x;
			tileSizes.y = num * tileSizeScales.y;
			tileSizes.z = num * tileSizeScales.z;
			tileSizes.w = num * tileSizeScales.w;
			water.SetVectorDirect(tileSizeId, tileSizes);
			water.SetVectorDirect(tileSizeScalesId, new Vector4(tileSizeScales.x / tileSizeScales.y, tileSizeScales.x / tileSizeScales.z, tileSizeScales.x / tileSizeScales.w, 0f));
			spectrumResolver.OnProfilesChanged();
			water.SetFloatDirect(maxDisplacementId, spectrumResolver.MaxHorizontalDisplacement);
		}

		private void Initialize()
		{
			OnValidate();
			water = GetComponent<Water>();
			if (spectrumResolver == null)
			{
				spectrumResolver = new SpectrumResolver(this, spectrumShader);
			}
			if (windDirectionChanged == null)
			{
				windDirectionChanged = new WindWavesEvent();
			}
			CreateObjects();
			initialized = true;
		}

		private void OnDestroy()
		{
			if (spectrumResolver != null)
			{
				spectrumResolver.OnDestroy();
				spectrumResolver = null;
			}
		}

		private void UpdateWind()
		{
			Vector2 vector;
			if (windDirectionPointer != null)
			{
				Vector3 forward = windDirectionPointer.forward;
				vector = new Vector2(forward.x, forward.z).normalized;
			}
			else
			{
				vector = new Vector2(1f, 0f);
			}
			Vector2 vector2 = windDirection * windSpeedMagnitude;
			if (windDirection != vector || windSpeed != vector2)
			{
				windDirection = vector;
				windSpeed = vector2;
				windSpeedChanged = true;
				spectrumResolver.SetWindDirection(windDirection);
			}
			else
			{
				windSpeedChanged = false;
			}
		}

		private void CreateObjects()
		{
			if (waterWavesFFT == null)
			{
				waterWavesFFT = new WavesRendererFFT();
			}
			if (waterWavesGerstner == null)
			{
				waterWavesGerstner = new WavesRendererGerstner();
			}
			if (dynamicSmoothness == null)
			{
				dynamicSmoothness = new DynamicSmoothness();
			}
		}

		private void CheckSupport()
		{
			if (highPrecision && (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat)))
			{
				finalHighPrecision = false;
			}
			if (!highPrecision && (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)))
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
				{
					finalHighPrecision = true;
				}
				else if (renderMode == WaveSpectrumRenderMode.FullFFT)
				{
					finalRenderMode = WaveSpectrumRenderMode.Gerstner;
				}
			}
		}

		public void OnWaterRender(Camera camera)
		{
			if (Application.isPlaying && base.enabled)
			{
				if (waterWavesFFT.Enabled)
				{
					waterWavesFFT.OnWaterRender(camera);
				}
				if (waterWavesGerstner.Enabled)
				{
					waterWavesGerstner.OnWaterRender(camera);
				}
			}
		}

		public void OnWaterPostRender(Camera camera)
		{
		}

		public void UpdateMaterial(Water water, WaterQualityLevel qualityLevel)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WaterQualityLevel qualityLevel)
		{
			CreateObjects();
			ResolveFinalSettings(qualityLevel);
			waterWavesFFT.BuildShaderVariant(variant, water, this, qualityLevel);
			waterWavesGerstner.BuildShaderVariant(variant, water, this, qualityLevel);
			variant.SetWaterKeyword("_INCLUDE_SLOPE_VARIANCE", dynamicSmoothness.Enabled);
		}

		public Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			return spectrumResolver.GetDisplacementAt(x, z, spectrumStart, spectrumEnd, time);
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			return spectrumResolver.GetHorizontalDisplacementAt(x, z, spectrumStart, spectrumEnd, time);
		}

		public float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			return spectrumResolver.GetHeightAt(x, z, spectrumStart, spectrumEnd, time);
		}

		public Vector4 GetForceAndHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			return spectrumResolver.GetForceAndHeightAt(x, z, spectrumStart, spectrumEnd, time);
		}
	}
}
