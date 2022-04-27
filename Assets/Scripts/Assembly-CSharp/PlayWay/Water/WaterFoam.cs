using UnityEngine;

namespace PlayWay.Water
{
	[RequireComponent(typeof(WindWaves))]
	[RequireComponent(typeof(Water))]
	[OverlayRendererOrder(1000)]
	[AddComponentMenu("Water/Foam", 1)]
	public class WaterFoam : MonoBehaviour, IWaterRenderAware, IOverlaysRenderer
	{
		[HideInInspector]
		[SerializeField]
		private Shader globalFoamSimulationShader;

		[SerializeField]
		[HideInInspector]
		private Shader localFoamSimulationShader;

		[SerializeField]
		[Tooltip("Foam map supersampling in relation to the waves simulator resolution. Has to be a power of two (0.25, 0.5, 1, 2, etc.)")]
		private float supersampling = 1f;

		private float foamIntensity = 1f;

		private float foamThreshold = 1f;

		private float foamFadingFactor = 0.85f;

		private RenderTexture foamMapA;

		private RenderTexture foamMapB;

		private Material globalFoamSimulationMaterial;

		private Material localFoamSimulationMaterial;

		private Vector2 lastCameraPos;

		private Vector2 deltaPosition;

		private Water water;

		private WindWaves windWaves;

		private WaterOverlays overlays;

		private int resolution;

		private bool firstFrame;

		private int foamParametersId;

		private int foamIntensityId;

		public Texture FoamMap
		{
			get
			{
				return foamMapA;
			}
		}

		private bool FloatingPointMipMapsSupport
		{
			get
			{
				string text = SystemInfo.graphicsDeviceVendor.ToLower();
				return !text.Contains("amd") && !text.Contains("ati") && !SystemInfo.graphicsDeviceName.ToLower().Contains("radeon") && WaterProjectSettings.Instance.AllowFloatingPointMipMaps;
			}
		}

		private void Start()
		{
			water = GetComponent<Water>();
			windWaves = GetComponent<WindWaves>();
			overlays = GetComponent<WaterOverlays>();
			foamParametersId = Shader.PropertyToID("_FoamParameters");
			foamIntensityId = Shader.PropertyToID("_FoamIntensity");
			windWaves.ResolutionChanged.AddListener(OnResolutionChanged);
			resolution = Mathf.RoundToInt((float)windWaves.FinalResolution * supersampling);
			globalFoamSimulationMaterial = new Material(globalFoamSimulationShader);
			globalFoamSimulationMaterial.hideFlags = HideFlags.DontSave;
			localFoamSimulationMaterial = new Material(localFoamSimulationShader);
			localFoamSimulationMaterial.hideFlags = HideFlags.DontSave;
			firstFrame = true;
		}

		private void OnEnable()
		{
			water = GetComponent<Water>();
			water.ProfilesChanged.AddListener(OnProfilesChanged);
			OnProfilesChanged(water);
		}

		private void OnDisable()
		{
			water.InvalidateMaterialKeywords();
			water.ProfilesChanged.RemoveListener(OnProfilesChanged);
		}

		public void OnWaterRender(Camera camera)
		{
		}

		public void OnWaterPostRender(Camera camera)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WaterQualityLevel qualityLevel)
		{
			OnValidate();
			variant.SetWaterKeyword("_WATER_FOAM_WS", base.enabled && overlays == null && CheckPreresquisites());
		}

		public void UpdateMaterial(Water water, WaterQualityLevel qualityLevel)
		{
		}

		private void SetupFoamMaterials()
		{
			if (globalFoamSimulationMaterial != null)
			{
				float num = foamThreshold * (float)resolution / 2048f * 220f * 0.7f;
				globalFoamSimulationMaterial.SetVector(foamParametersId, new Vector4(foamIntensity * 0.6f, 0f, 0f, foamFadingFactor));
				globalFoamSimulationMaterial.SetVector(foamIntensityId, new Vector4(num / windWaves.TileSizes.x, num / windWaves.TileSizes.y, num / windWaves.TileSizes.z, num / windWaves.TileSizes.w));
			}
		}

		private void SetKeyword(Material material, string name, bool val)
		{
			if (val)
			{
				material.EnableKeyword(name);
			}
			else
			{
				material.DisableKeyword(name);
			}
		}

		private void SetKeyword(Material material, int index, params string[] names)
		{
			foreach (string keyword in names)
			{
				material.DisableKeyword(keyword);
			}
			material.EnableKeyword(names[index]);
		}

		private void OnValidate()
		{
			if (globalFoamSimulationShader == null)
			{
				globalFoamSimulationShader = Shader.Find("PlayWay Water/Foam/Global");
			}
			if (localFoamSimulationShader == null)
			{
				localFoamSimulationShader = Shader.Find("PlayWay Water/Foam/Local");
			}
			supersampling = (float)Mathf.ClosestPowerOfTwo(Mathf.RoundToInt(supersampling * 4096f)) / 4096f;
			water = GetComponent<Water>();
			windWaves = GetComponent<WindWaves>();
			overlays = GetComponent<WaterOverlays>();
		}

		private void Dispose(bool completely)
		{
			if (foamMapA != null)
			{
				Object.Destroy(foamMapA);
				Object.Destroy(foamMapB);
				foamMapA = null;
				foamMapB = null;
			}
		}

		private void OnDestroy()
		{
			Dispose(true);
		}

		private void LateUpdate()
		{
			if (!firstFrame)
			{
				UpdateFoamMap();
			}
			else
			{
				firstFrame = false;
			}
		}

		private void CheckResources()
		{
			if (foamMapA == null)
			{
				foamMapA = CreateRT(0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, TextureWrapMode.Repeat);
				foamMapB = CreateRT(0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear, TextureWrapMode.Repeat);
				RenderTexture.active = null;
			}
		}

		private RenderTexture CreateRT(int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite, TextureWrapMode wrapMode)
		{
			RenderTexture renderTexture = new RenderTexture(resolution, resolution, depth, format, readWrite);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.wrapMode = wrapMode;
			if (FloatingPointMipMapsSupport)
			{
				renderTexture.filterMode = FilterMode.Trilinear;
				renderTexture.useMipMap = true;
				renderTexture.autoGenerateMips = true;
			}
			else
			{
				renderTexture.filterMode = FilterMode.Bilinear;
			}
			RenderTexture.active = renderTexture;
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			return renderTexture;
		}

		private void UpdateFoamMap()
		{
			if (CheckPreresquisites() && overlays == null)
			{
				CheckResources();
				SetupFoamMaterials();
				globalFoamSimulationMaterial.SetTexture("_DisplacementMap0", windWaves.WaterWavesFFT.GetDisplacementMap(0));
				globalFoamSimulationMaterial.SetTexture("_DisplacementMap1", windWaves.WaterWavesFFT.GetDisplacementMap(1));
				globalFoamSimulationMaterial.SetTexture("_DisplacementMap2", windWaves.WaterWavesFFT.GetDisplacementMap(2));
				globalFoamSimulationMaterial.SetTexture("_DisplacementMap3", windWaves.WaterWavesFFT.GetDisplacementMap(3));
				Graphics.Blit(foamMapA, foamMapB, globalFoamSimulationMaterial, 0);
				water.WaterMaterial.SetTexture("_FoamMapWS", foamMapB);
				SwapRenderTargets();
			}
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			resolution = Mathf.RoundToInt((float)windWaves.FinalResolution * supersampling);
			Dispose(false);
		}

		private bool CheckPreresquisites()
		{
			return windWaves != null && windWaves.enabled && windWaves.FinalRenderMode == WaveSpectrumRenderMode.FullFFT;
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.Profiles;
			foamIntensity = 0f;
			foamThreshold = 0f;
			foamFadingFactor = 0f;
			float num = 0f;
			if (profiles != null)
			{
				Water.WeightedProfile[] array = profiles;
				for (int i = 0; i < array.Length; i++)
				{
					Water.WeightedProfile weightedProfile = array[i];
					WaterProfile profile = weightedProfile.profile;
					float weight = weightedProfile.weight;
					foamIntensity += profile.FoamIntensity * weight;
					foamThreshold += profile.FoamThreshold * weight;
					foamFadingFactor += profile.FoamFadingFactor * weight;
					num += profile.FoamNormalScale * weight;
				}
			}
			water.WaterMaterial.SetFloat("_FoamNormalScale", num);
		}

		private Vector2 RotateVector(Vector2 vec, float angle)
		{
			float num = Mathf.Sin(angle);
			float num2 = Mathf.Cos(angle);
			return new Vector2(num2 * vec.x + num * vec.y, num2 * vec.y + num * vec.x);
		}

		private void SwapRenderTargets()
		{
			RenderTexture renderTexture = foamMapA;
			foamMapA = foamMapB;
			foamMapB = renderTexture;
		}

		public void RenderOverlays(WaterOverlaysData overlays)
		{
			if (base.enabled)
			{
				localFoamSimulationMaterial.CopyPropertiesFromMaterial(water.WaterMaterial);
				float y = foamThreshold * (float)overlays.DynamicDisplacementMap.width / 2048f * 0.7f;
				localFoamSimulationMaterial.SetVector(foamParametersId, new Vector4(foamIntensity * 0.6f, y, 0f, foamFadingFactor));
				localFoamSimulationMaterial.SetTexture("_DisplacementMap", overlays.DynamicDisplacementMap);
				localFoamSimulationMaterial.SetTexture("_SlopeMapPrevious", overlays.SlopeMapPrevious);
				localFoamSimulationMaterial.SetTexture("_TotalDisplacementMap", overlays.GetTotalDisplacementMap());
				localFoamSimulationMaterial.SetVector("_Offset", new Vector4((Random.value - 0.5f) * 0.001f, (Random.value - 0.5f) * 0.001f, 0f, 0f));
				Graphics.Blit(overlays.SlopeMapPrevious, overlays.SlopeMap, localFoamSimulationMaterial, overlays.Initialization ? 1 : 0);
			}
		}
	}
}
