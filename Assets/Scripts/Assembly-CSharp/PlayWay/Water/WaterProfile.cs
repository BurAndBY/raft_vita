using UnityEngine;

namespace PlayWay.Water
{
	public class WaterProfile : ScriptableObject
	{
		public enum WaterSpectrumType
		{
			Phillips,
			Unified
		}

		[SerializeField]
		[HideInInspector]
		private Shader spectrumShader;

		[SerializeField]
		private WaterSpectrumType spectrumType = WaterSpectrumType.Unified;

		[SerializeField]
		private float windSpeed = 22f;

		[Tooltip("Tile size in world units of all water maps including heightmap. High values lower overall quality, but low values make the water pattern noticeable.")]
		[SerializeField]
		private float tileSize = 180f;

		[SerializeField]
		private float tileScale = 1f;

		[Tooltip("Setting it to something else than 1.0 will make the spectrum less physically correct, but still may be useful at times.")]
		[SerializeField]
		private float wavesAmplitude = 1f;

		[Range(0f, 4f)]
		[SerializeField]
		private float horizontalDisplacementScale = 1f;

		[SerializeField]
		private float phillipsCutoffFactor = 2000f;

		[SerializeField]
		private float gravity = -9.81f;

		[Tooltip("It is the length of water in meters over which a wind has blown. Usually a distance to the closest land in the direction opposite to the wind.")]
		[SerializeField]
		private float fetch = 100000f;

		[Range(0f, 1f)]
		[Tooltip("Eliminates waves moving against the wind.")]
		[SerializeField]
		private float directionality;

		[SerializeField]
		[ColorUsage(false, true, 0f, 3f, 0f, 3f)]
		private Color absorptionColor = new Color(0.35f, 0.04f, 0.001f, 1f);

		[SerializeField]
		[ColorUsage(false, true, 0f, 3f, 0f, 3f)]
		[Tooltip("Used by the underwater camera image-effect.")]
		private Color underwaterAbsorptionColor = new Color(0.35f, 0.04f, 0.001f, 1f);

		[ColorUsage(false)]
		[SerializeField]
		private Color diffuseColor = new Color(0.1176f, 0.2196f, 0.2666f);

		[SerializeField]
		[ColorUsage(false)]
		private Color specularColor = new Color(0.0353f, 0.0471f, 0.0549f);

		[SerializeField]
		[ColorUsage(false)]
		private Color depthColor = new Color(0f, 0f, 0f);

		[ColorUsage(false)]
		[SerializeField]
		private Color emissionColor = new Color(0f, 0f, 0f);

		[ColorUsage(false)]
		[SerializeField]
		private Color reflectionColor = new Color(1f, 1f, 1f);

		[SerializeField]
		[Range(0f, 1f)]
		private float smoothness = 0.94f;

		[SerializeField]
		private bool customAmbientSmoothness;

		[Range(0f, 1f)]
		[SerializeField]
		private float ambientSmoothness = 0.94f;

		[SerializeField]
		[Range(0f, 1f)]
		private float subsurfaceScattering = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float refractionDistortion = 0.55f;

		[SerializeField]
		private float fresnelBias = 0.02040781f;

		[Range(0.5f, 20f)]
		[SerializeField]
		private float detailFadeDistance = 4.5f;

		[SerializeField]
		[Range(0.1f, 10f)]
		private float displacementNormalsIntensity = 2f;

		[SerializeField]
		[Tooltip("Planar reflections are very good solution for calm weather, but you should fade them out for profiles with big waves (storms etc.) as they get completely incorrect then.")]
		[Range(0f, 1f)]
		private float planarReflectionIntensity = 0.6f;

		[SerializeField]
		[Range(1f, 10f)]
		private float planarReflectionFlatten = 6f;

		[SerializeField]
		[Range(0f, 0.008f)]
		[Tooltip("Fixes some artifacts produced by planar reflections at grazing angles.")]
		private float planarReflectionVerticalOffset = 0.0015f;

		[SerializeField]
		private float edgeBlendFactor = 0.15f;

		[SerializeField]
		private float directionalWrapSSS = 0.2f;

		[SerializeField]
		private float pointWrapSSS = 0.5f;

		[Tooltip("Used by the physics.")]
		[SerializeField]
		private float density = 998.6f;

		[Range(0f, 0.03f)]
		[SerializeField]
		private float underwaterBlurSize = 0.003f;

		[SerializeField]
		[Range(0f, 0.4f)]
		private float underwaterDistortionsIntensity = 0.05f;

		[SerializeField]
		[Range(0.02f, 0.5f)]
		private float underwaterDistortionAnimationSpeed = 0.1f;

		[SerializeField]
		private NormalMapAnimation normalMapAnimation1 = new NormalMapAnimation(1f, -10f, 1f, new Vector2(1f, 1f));

		[SerializeField]
		private NormalMapAnimation normalMapAnimation2 = new NormalMapAnimation(-0.55f, 20f, 0.74f, new Vector2(1.5f, 1.5f));

		[SerializeField]
		private Texture2D normalMap;

		[SerializeField]
		private float foamIntensity = 1f;

		[SerializeField]
		private float foamThreshold = 1f;

		[SerializeField]
		[Tooltip("Determines how fast foam will fade out.")]
		private float foamFadingFactor = 0.85f;

		[SerializeField]
		private float foamNormalScale = 2.2f;

		[SerializeField]
		private Color foamSpecularColor = new Color(1f, 1f, 1f, 0f);

		[SerializeField]
		[Range(0f, 4f)]
		private float sprayThreshold = 1f;

		[SerializeField]
		[Range(0f, 0.999f)]
		private float spraySkipRatio = 0.9f;

		[Range(0.25f, 4f)]
		[SerializeField]
		private float spraySize = 1f;

		[SerializeField]
		private Texture2D foamDiffuseMap;

		[SerializeField]
		private Texture2D foamNormalMap;

		[SerializeField]
		private Vector2 foamTiling = new Vector2(5.4f, 5.4f);

		private WaterWavesSpectrum spectrum;

		public WaterSpectrumType SpectrumType
		{
			get
			{
				return spectrumType;
			}
		}

		public WaterWavesSpectrum Spectrum
		{
			get
			{
				if (spectrum == null)
				{
					CreateSpectrum();
				}
				return spectrum;
			}
		}

		public float WindSpeed
		{
			get
			{
				return windSpeed;
			}
		}

		public float TileSize
		{
			get
			{
				return tileSize;
			}
		}

		public float TileScale
		{
			get
			{
				return tileScale;
			}
		}

		public float HorizontalDisplacementScale
		{
			get
			{
				return horizontalDisplacementScale;
			}
		}

		public float Gravity
		{
			get
			{
				return gravity;
			}
		}

		public float Directionality
		{
			get
			{
				return directionality;
			}
		}

		public Color AbsorptionColor
		{
			get
			{
				return absorptionColor;
			}
		}

		public Color UnderwaterAbsorptionColor
		{
			get
			{
				return underwaterAbsorptionColor;
			}
		}

		public Color DiffuseColor
		{
			get
			{
				return diffuseColor;
			}
		}

		public Color SpecularColor
		{
			get
			{
				return specularColor;
			}
		}

		public Color DepthColor
		{
			get
			{
				return depthColor;
			}
		}

		public Color EmissionColor
		{
			get
			{
				return emissionColor;
			}
		}

		public Color ReflectionColor
		{
			get
			{
				return reflectionColor;
			}
		}

		public float Smoothness
		{
			get
			{
				return smoothness;
			}
		}

		public bool CustomAmbientSmoothness
		{
			get
			{
				return customAmbientSmoothness;
			}
		}

		public float AmbientSmoothness
		{
			get
			{
				return (!customAmbientSmoothness) ? smoothness : ambientSmoothness;
			}
		}

		public float SubsurfaceScattering
		{
			get
			{
				return subsurfaceScattering;
			}
		}

		public float RefractionDistortion
		{
			get
			{
				return refractionDistortion;
			}
		}

		public float FresnelBias
		{
			get
			{
				return fresnelBias;
			}
		}

		public float DetailFadeDistance
		{
			get
			{
				return detailFadeDistance * detailFadeDistance;
			}
		}

		public float DisplacementNormalsIntensity
		{
			get
			{
				return displacementNormalsIntensity;
			}
		}

		public float PlanarReflectionIntensity
		{
			get
			{
				return planarReflectionIntensity;
			}
		}

		public float PlanarReflectionFlatten
		{
			get
			{
				return planarReflectionFlatten;
			}
		}

		public float PlanarReflectionVerticalOffset
		{
			get
			{
				return planarReflectionVerticalOffset;
			}
		}

		public float EdgeBlendFactor
		{
			get
			{
				return edgeBlendFactor;
			}
		}

		public float DirectionalWrapSSS
		{
			get
			{
				return directionalWrapSSS;
			}
		}

		public float PointWrapSSS
		{
			get
			{
				return pointWrapSSS;
			}
		}

		public float Density
		{
			get
			{
				return density;
			}
		}

		public float UnderwaterBlurSize
		{
			get
			{
				return underwaterBlurSize;
			}
		}

		public float UnderwaterDistortionsIntensity
		{
			get
			{
				return underwaterDistortionsIntensity;
			}
		}

		public float UnderwaterDistortionAnimationSpeed
		{
			get
			{
				return underwaterDistortionAnimationSpeed;
			}
		}

		public NormalMapAnimation NormalMapAnimation1
		{
			get
			{
				return normalMapAnimation1;
			}
		}

		public NormalMapAnimation NormalMapAnimation2
		{
			get
			{
				return normalMapAnimation2;
			}
		}

		public float FoamIntensity
		{
			get
			{
				return foamIntensity;
			}
		}

		public float FoamThreshold
		{
			get
			{
				return foamThreshold;
			}
		}

		public float FoamFadingFactor
		{
			get
			{
				return foamFadingFactor;
			}
		}

		public float FoamNormalScale
		{
			get
			{
				return foamNormalScale;
			}
		}

		public Color FoamSpecularColor
		{
			get
			{
				return foamSpecularColor;
			}
		}

		public float SprayThreshold
		{
			get
			{
				return sprayThreshold;
			}
		}

		public float SpraySkipRatio
		{
			get
			{
				return spraySkipRatio;
			}
		}

		public float SpraySize
		{
			get
			{
				return spraySize;
			}
		}

		public Texture2D NormalMap
		{
			get
			{
				return normalMap;
			}
		}

		public Texture2D FoamDiffuseMap
		{
			get
			{
				return foamDiffuseMap;
			}
		}

		public Texture2D FoamNormalMap
		{
			get
			{
				return foamNormalMap;
			}
		}

		public Vector2 FoamTiling
		{
			get
			{
				return foamTiling;
			}
		}

		public void CacheSpectrum()
		{
			if (spectrum == null)
			{
				CreateSpectrum();
			}
		}

		private void OnEnable()
		{
			if (spectrum == null)
			{
				CreateSpectrum();
			}
		}

		private void CreateSpectrum()
		{
			switch (spectrumType)
			{
			case WaterSpectrumType.Unified:
				spectrum = new UnifiedSpectrum(tileSize, 0f - gravity, windSpeed, wavesAmplitude, fetch);
				break;
			case WaterSpectrumType.Phillips:
				spectrum = new PhillipsSpectrum(tileSize, 0f - gravity, windSpeed, wavesAmplitude, phillipsCutoffFactor);
				break;
			}
		}
	}
}
