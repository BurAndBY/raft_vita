using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace PlayWay.Water
{
	[AddComponentMenu("Water/Water (Base Component)", -1)]
	[ExecuteInEditMode]
	public class Water : MonoBehaviour, IShaderCollectionClient
	{
		[Serializable]
		public class WaterEvent : UnityEvent<Water>
		{
		}

		public struct WeightedProfile
		{
			public WaterProfile profile;

			public float weight;

			public WeightedProfile(WaterProfile profile, float weight)
			{
				this.profile = profile;
				this.weight = weight;
			}
		}

		public struct ParameterOverride<T>
		{
			public int hash;

			public T value;

			public ParameterOverride(int hash, T value)
			{
				this.hash = hash;
				this.value = value;
			}
		}

		public enum ColorParameter
		{
			AbsorptionColor,
			DiffuseColor,
			SpecularColor,
			DepthColor,
			EmissionColor,
			ReflectionColor
		}

		public enum FloatParameter
		{
			DisplacementScale = 6,
			Glossiness = 7,
			RefractionDistortion = 10,
			SpecularFresnelBias = 11,
			DisplacementNormalsIntensity = 13,
			EdgeBlendFactorInv = 14,
			LightSmoothnessMultiplier = 18
		}

		public enum VectorParameter
		{
			SubsurfaceScatteringPack = 8,
			WrapSubsurfaceScatteringPack = 9,
			DetailFadeFactor = 12,
			PlanarReflectionPack = 15,
			BumpScale = 16,
			FoamTiling = 17
		}

		public enum TextureParameter
		{
			BumpMap = 19,
			FoamTex,
			FoamNormalMap
		}

		public enum LaunchState
		{
			Disabled,
			Started,
			Ready
		}

		[SerializeField]
		private WaterProfile profile;

		[SerializeField]
		private Shader waterShader;

		[SerializeField]
		private Shader waterVolumeShader;

		[SerializeField]
		private bool refraction = true;

		[SerializeField]
		private bool blendEdges = true;

		[SerializeField]
		private bool volumetricLighting = true;

		[Tooltip("Affects direct light specular and diffuse components. Shadows currently work only for main directional light and you need to attach WaterShadowCastingLight script to it. Also it doesn't work at all on mobile platforms.")]
		[SerializeField]
		private bool receiveShadows;

		[SerializeField]
		private ShaderCollection shaderCollection;

		[SerializeField]
		private ShadowCastingMode shadowCastingMode;

		[SerializeField]
		private bool useCubemapReflections = true;

		[Tooltip("Set it to anything else than 0 if your game has multiplayer functionality or you want your water to behave the same way each time your game is played (good for intro etc.).")]
		[SerializeField]
		private int seed;

		[SerializeField]
		[Tooltip("May hurt performance on some systems.")]
		[Range(0f, 1f)]
		private float tesselationFactor = 1f;

		[SerializeField]
		private float refractionMaxDepth = -1f;

		[SerializeField]
		private WaterUvAnimator uvAnimator;

		[SerializeField]
		private WaterVolume volume;

		[SerializeField]
		private WaterGeometry geometry;

		[SerializeField]
		private WaterRenderer waterRenderer;

		[SerializeField]
		private WaterEvent profilesChanged;

		[SerializeField]
		private Material waterMaterialPrefab;

		[SerializeField]
		private Material waterVolumeMaterialPrefab;

		private WeightedProfile[] profiles;

		private bool profilesDirty;

		private Material waterMaterial;

		private Material waterBackMaterial;

		private Material waterVolumeMaterial;

		private Material waterVolumeBackMaterial;

		private float horizontalDisplacementScale;

		private float gravity;

		private float directionality;

		private float density;

		private float underwaterBlurSize;

		private float underwaterDistortionsIntensity;

		private float underwaterDistortionAnimationSpeed;

		private float time = -1f;

		private Color underwaterAbsorptionColor;

		private float maxHorizontalDisplacement;

		private float maxVerticalDisplacement;

		private int waterId;

		private int surfaceOffsetId;

		private int activeSamplesCount;

		private WaterProfile runtimeProfile;

		private LaunchState launchState;

		private Vector2 surfaceOffset = new Vector2(float.NaN, float.NaN);

		private IWaterRenderAware[] renderAwareComponents;

		private IWaterDisplacements[] displacingComponents;

		private static int nextWaterId = 1;

		private static string[] parameterNames = new string[24]
		{
			"_AbsorptionColor", "_Color", "_SpecColor", "_DepthColor", "_EmissionColor", "_ReflectionColor", "_DisplacementsScale", "_Glossiness", "_SubsurfaceScatteringPack", "_WrapSubsurfaceScatteringPack",
			"_RefractionDistortion", "_SpecularFresnelBias", "_DetailFadeFactor", "_DisplacementNormalsIntensity", "_EdgeBlendFactorInv", "_PlanarReflectionPack", "_BumpScale", "_FoamTiling", "_LightSmoothnessMul", "_BumpMap",
			"_FoamTex", "_FoamNormalMap", "_FoamSpecularColor", "_RefractionMaxDepth"
		};

		private static string[] disallowedVolumeKeywords = new string[12]
		{
			"_WAVES_FFT_SLOPE", "_WAVES_GERSTNER", "_WATER_FOAM_WS", "_PLANAR_REFLECTIONS", "_PLANAR_REFLECTIONS_HQ", "_INCLUDE_SLOPE_VARIANCE", "_NORMALMAP", "_PROJECTION_GRID", "_WATER_OVERLAYS", "_WAVES_ALIGN",
			"_TRIANGLES", "_BOUNDED_WATER"
		};

		private static string[] hardwareDependentKeywords = new string[2] { "_WATER_FOAM_WS", "_WATER_RECEIVE_SHADOWS" };

		private int[] parameterHashes;

		private ParameterOverride<Vector4>[] vectorOverrides;

		private ParameterOverride<Color>[] colorOverrides;

		private ParameterOverride<float>[] floatOverrides;

		private ParameterOverride<Texture>[] textureOverrides;

		private static Collider[] collidersBuffer = new Collider[30];

		private static List<Water> possibleWaters = new List<Water>();

		private static List<Water> excludedWaters = new List<Water>();

		public int WaterId
		{
			get
			{
				return waterId;
			}
		}

		public Material WaterMaterial
		{
			get
			{
				if (waterMaterial == null)
				{
					CreateMaterials();
				}
				return waterMaterial;
			}
		}

		public Material WaterBackMaterial
		{
			get
			{
				if (waterBackMaterial == null)
				{
					CreateMaterials();
				}
				return waterBackMaterial;
			}
		}

		public Material WaterVolumeMaterial
		{
			get
			{
				if (waterVolumeMaterial == null)
				{
					CreateMaterials();
				}
				return waterVolumeMaterial;
			}
		}

		public Material WaterVolumeBackMaterial
		{
			get
			{
				if (waterVolumeBackMaterial == null)
				{
					CreateMaterials();
				}
				return waterVolumeBackMaterial;
			}
		}

		public WeightedProfile[] Profiles
		{
			get
			{
				return profiles;
			}
		}

		public float HorizontalDisplacementScale
		{
			get
			{
				return horizontalDisplacementScale;
			}
		}

		public bool ReceiveShadows
		{
			get
			{
				return receiveShadows;
			}
		}

		public ShadowCastingMode ShadowCastingMode
		{
			get
			{
				return shadowCastingMode;
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

		public float UniformWaterScale
		{
			get
			{
				return base.transform.localScale.y;
			}
		}

		public Color UnderwaterAbsorptionColor
		{
			get
			{
				return underwaterAbsorptionColor;
			}
		}

		public bool VolumetricLighting
		{
			get
			{
				return volumetricLighting;
			}
		}

		public bool FinalVolumetricLighting
		{
			get
			{
				return volumetricLighting && WaterQualitySettings.Instance.AllowVolumetricLighting;
			}
		}

		public int ComputedSamplesCount
		{
			get
			{
				return activeSamplesCount;
			}
		}

		public WaterEvent ProfilesChanged
		{
			get
			{
				return profilesChanged;
			}
		}

		public WaterVolume Volume
		{
			get
			{
				return volume;
			}
		}

		public WaterGeometry Geometry
		{
			get
			{
				return geometry;
			}
		}

		public WaterRenderer Renderer
		{
			get
			{
				return waterRenderer;
			}
		}

		public int Seed
		{
			get
			{
				return seed;
			}
			set
			{
				seed = value;
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

		public ShaderCollection ShaderCollection
		{
			get
			{
				return shaderCollection;
			}
		}

		public float MaxHorizontalDisplacement
		{
			get
			{
				return maxHorizontalDisplacement;
			}
		}

		public float MaxVerticalDisplacement
		{
			get
			{
				return maxVerticalDisplacement;
			}
		}

		public float Time
		{
			get
			{
				return (time != -1f) ? time : UnityEngine.Time.time;
			}
			set
			{
				time = value;
			}
		}

		public Vector2 SurfaceOffset
		{
			get
			{
				return (!float.IsNaN(surfaceOffset.x)) ? surfaceOffset : new Vector2(0f - base.transform.position.x, 0f - base.transform.position.z);
			}
			set
			{
				surfaceOffset = value;
			}
		}

		public Color AbsorptionColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[0]);
			}
			set
			{
				SetColor(ColorParameter.AbsorptionColor, value);
			}
		}

		public Color DiffuseColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[1]);
			}
			set
			{
				SetColor(ColorParameter.DiffuseColor, value);
			}
		}

		public Color SpecularColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[2]);
			}
			set
			{
				SetColor(ColorParameter.SpecularColor, value);
			}
		}

		public Color DepthColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[3]);
			}
			set
			{
				SetColor(ColorParameter.DepthColor, value);
			}
		}

		public Color EmissionColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[4]);
			}
			set
			{
				SetColor(ColorParameter.EmissionColor, value);
			}
		}

		public Color ReflectionColor
		{
			get
			{
				return waterMaterial.GetColor(parameterHashes[5]);
			}
			set
			{
				SetColor(ColorParameter.ReflectionColor, value);
			}
		}

		public float SubsurfaceScattering
		{
			get
			{
				return waterMaterial.GetVector(parameterHashes[8]).x;
			}
			set
			{
				Vector4 vector = waterMaterial.GetVector(parameterHashes[8]);
				vector.x = value;
				SetVector(VectorParameter.SubsurfaceScatteringPack, vector);
			}
		}

		private void Awake()
		{
			waterId = nextWaterId++;
			bool flag = volume == null;
			CreateWaterManagers();
			if (flag)
			{
				base.gameObject.layer = WaterProjectSettings.Instance.WaterLayer;
			}
			CreateParameterHashes();
			renderAwareComponents = GetComponents<IWaterRenderAware>();
			displacingComponents = GetComponents<IWaterDisplacements>();
			if (!Application.isPlaying)
			{
				return;
			}
			if (profile == null)
			{
				Debug.LogError("Water profile is not set. You may assign it in the inspector.");
				base.gameObject.SetActive(false);
				return;
			}
			try
			{
				CreateMaterials();
				if (profiles == null)
				{
					profiles = new WeightedProfile[1]
					{
						new WeightedProfile(profile, 1f)
					};
					ResolveProfileData(profiles);
				}
				uvAnimator.Start(this);
				profilesChanged.AddListener(OnProfilesChanged);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
				base.gameObject.SetActive(false);
			}
		}

		private void Start()
		{
			launchState = LaunchState.Started;
			SetupMaterials();
			if (profiles != null)
			{
				ResolveProfileData(profiles);
			}
			profilesDirty = true;
		}

		private void OnEnable()
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			launchState = LaunchState.Started;
			CreateParameterHashes();
			ValidateShaders();
			if (Application.isPlaying)
			{
				shaderCollection = null;
			}
			CreateMaterials();
			if (profiles == null && profile != null)
			{
				profiles = new WeightedProfile[1]
				{
					new WeightedProfile(profile, 1f)
				};
				ResolveProfileData(profiles);
			}
			WaterQualitySettings.Instance.Changed -= new Action(OnQualitySettingsChanged);
			WaterQualitySettings.Instance.Changed += new Action(OnQualitySettingsChanged);
			WaterGlobals.Instance.AddWater(this);
			if (geometry != null)
			{
				geometry.OnEnable(this);
				waterRenderer.OnEnable(this);
				volume.OnEnable(this);
			}
		}

		private void OnDisable()
		{
			WaterGlobals.Instance.RemoveWater(this);
			geometry.OnDisable();
			waterRenderer.OnDisable();
			volume.OnDisable();
		}

		private void OnDestroy()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			WaterQualitySettings.Instance.Changed -= new Action(OnQualitySettingsChanged);
		}

		public Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector3 result = default(Vector3);
			for (int i = 0; i < displacingComponents.Length; i++)
			{
				result += displacingComponents[i].GetDisplacementAt(x, z, spectrumStart, spectrumEnd, time);
			}
			return result;
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector2 result = default(Vector2);
			for (int i = 0; i < displacingComponents.Length; i++)
			{
				result += displacingComponents[i].GetHorizontalDisplacementAt(x, z, spectrumStart, spectrumEnd, time);
			}
			return result;
		}

		public float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			float num = 0f;
			for (int i = 0; i < displacingComponents.Length; i++)
			{
				num += displacingComponents[i].GetHeightAt(x, z, spectrumStart, spectrumEnd, time);
			}
			return num;
		}

		public Vector4 GetHeightAndForcesAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector4 zero = Vector4.zero;
			for (int i = 0; i < displacingComponents.Length; i++)
			{
				zero += displacingComponents[i].GetForceAndHeightAt(x, z, spectrumStart, spectrumEnd, time);
			}
			return zero;
		}

		public void CacheProfiles(params WaterProfile[] profiles)
		{
			WindWaves component = GetComponent<WindWaves>();
			if (component != null)
			{
				foreach (WaterProfile waterProfile in profiles)
				{
					component.SpectrumResolver.CacheSpectrum(waterProfile.Spectrum);
				}
			}
		}

		public void SetProfiles(params WeightedProfile[] profiles)
		{
			ValidateProfiles(profiles);
			this.profiles = profiles;
			profilesDirty = true;
		}

		public void InvalidateMaterialKeywords()
		{
		}

		private void CreateMaterials()
		{
			if (waterMaterial == null)
			{
				if (waterMaterialPrefab == null)
				{
					waterMaterial = new Material(waterShader);
				}
				else
				{
					waterMaterial = UnityEngine.Object.Instantiate(waterMaterialPrefab);
				}
				waterMaterial.hideFlags = HideFlags.DontSave;
				waterMaterial.SetVector("_WaterId", new Vector4(1 << waterId, 1 << waterId + 1, 0f, 0f));
				waterMaterial.SetFloat("_WaterStencilId", waterId);
				waterMaterial.SetFloat("_WaterStencilIdInv", ~waterId & 0xFF);
			}
			if (waterBackMaterial == null)
			{
				if (waterMaterialPrefab == null)
				{
					waterBackMaterial = new Material(waterShader);
				}
				else
				{
					waterBackMaterial = UnityEngine.Object.Instantiate(waterMaterialPrefab);
				}
				waterBackMaterial.hideFlags = HideFlags.DontSave;
				UpdateBackMaterial();
			}
			bool flag = false;
			if (waterVolumeMaterial == null)
			{
				if (waterVolumeMaterialPrefab == null)
				{
					waterVolumeMaterial = new Material(waterVolumeShader);
				}
				else
				{
					waterVolumeMaterial = UnityEngine.Object.Instantiate(waterVolumeMaterialPrefab);
				}
				waterVolumeMaterial.hideFlags = HideFlags.DontSave;
				flag = true;
			}
			if (waterVolumeBackMaterial == null)
			{
				if (waterVolumeMaterialPrefab == null)
				{
					waterVolumeBackMaterial = new Material(waterVolumeShader);
				}
				else
				{
					waterVolumeBackMaterial = UnityEngine.Object.Instantiate(waterVolumeMaterialPrefab);
				}
				waterVolumeBackMaterial.hideFlags = HideFlags.DontSave;
				flag = true;
			}
			if (flag)
			{
				UpdateWaterVolumeMaterials();
			}
		}

		private void SetupMaterials()
		{
			if (launchState != 0)
			{
				WaterQualitySettings instance = WaterQualitySettings.Instance;
				ShaderVariant shaderVariant = new ShaderVariant();
				BuildShaderVariant(shaderVariant, instance.CurrentQualityLevel);
				ValidateShaderCollection(shaderVariant);
				if (shaderCollection != null)
				{
					waterMaterial.shader = shaderCollection.GetShaderVariant(shaderVariant.GetWaterKeywords(), shaderVariant.GetUnityKeywords(), shaderVariant.GetAdditionalCode(), shaderVariant.GetKeywordsString(), false);
				}
				else
				{
					waterMaterial.shader = ShaderCollection.GetRuntimeShaderVariant(shaderVariant.GetKeywordsString(), false);
				}
				waterMaterial.shaderKeywords = shaderVariant.GetUnityKeywords();
				UpdateMaterials();
				UpdateBackMaterial();
				string[] array = disallowedVolumeKeywords;
				foreach (string keyword in array)
				{
					shaderVariant.SetWaterKeyword(keyword, false);
				}
				if (shaderCollection != null)
				{
					waterVolumeMaterial.shader = shaderCollection.GetShaderVariant(shaderVariant.GetWaterKeywords(), shaderVariant.GetUnityKeywords(), shaderVariant.GetAdditionalCode(), shaderVariant.GetKeywordsString(), true);
				}
				else
				{
					waterVolumeMaterial.shader = ShaderCollection.GetRuntimeShaderVariant(shaderVariant.GetKeywordsString(), true);
				}
				waterVolumeBackMaterial.shader = waterVolumeMaterial.shader;
				UpdateWaterVolumeMaterials();
				Material material = waterVolumeBackMaterial;
				string[] unityKeywords = shaderVariant.GetUnityKeywords();
				waterVolumeMaterial.shaderKeywords = unityKeywords;
				material.shaderKeywords = unityKeywords;
			}
		}

		private void SetShader(ref Material material, Shader shader)
		{
		}

		private void ValidateShaderCollection(ShaderVariant variant)
		{
		}

		[ContextMenu("Rebuild Shaders")]
		private void RebuildShaders()
		{
		}

		private void UpdateBackMaterial()
		{
			if (waterBackMaterial != null)
			{
				waterBackMaterial.shader = waterMaterial.shader;
				waterBackMaterial.CopyPropertiesFromMaterial(waterMaterial);
				waterBackMaterial.SetFloat(parameterHashes[11], 0f);
				waterBackMaterial.EnableKeyword("_WATER_BACK");
				waterBackMaterial.SetFloat("_Cull", 1f);
			}
		}

		private void UpdateWaterVolumeMaterials()
		{
			if (waterVolumeMaterial != null)
			{
				waterVolumeMaterial.CopyPropertiesFromMaterial(waterMaterial);
				waterVolumeBackMaterial.CopyPropertiesFromMaterial(waterMaterial);
				Material material = waterVolumeBackMaterial;
				int renderQueue = ((!refraction && !blendEdges) ? 2000 : 2991);
				waterVolumeMaterial.renderQueue = renderQueue;
				material.renderQueue = renderQueue;
				waterVolumeBackMaterial.SetFloat("_Cull", 1f);
			}
		}

		internal void SetVectorDirect(int materialPropertyId, Vector4 vector)
		{
			waterMaterial.SetVector(materialPropertyId, vector);
			waterBackMaterial.SetVector(materialPropertyId, vector);
			waterVolumeMaterial.SetVector(materialPropertyId, vector);
		}

		internal void SetFloatDirect(int materialPropertyId, float value)
		{
			waterMaterial.SetFloat(materialPropertyId, value);
			waterBackMaterial.SetFloat(materialPropertyId, value);
			waterVolumeMaterial.SetFloat(materialPropertyId, value);
		}

		public bool SetKeyword(string keyword, bool enable)
		{
			if (waterMaterial != null)
			{
				if (enable)
				{
					if (!waterMaterial.IsKeywordEnabled(keyword))
					{
						waterMaterial.EnableKeyword(keyword);
						waterBackMaterial.EnableKeyword(keyword);
						waterVolumeMaterial.EnableKeyword(keyword);
						return true;
					}
				}
				else if (waterMaterial.IsKeywordEnabled(keyword))
				{
					waterMaterial.DisableKeyword(keyword);
					waterBackMaterial.DisableKeyword(keyword);
					waterVolumeMaterial.DisableKeyword(keyword);
					return true;
				}
			}
			return false;
		}

		public void OnValidate()
		{
			ValidateShaders();
			renderAwareComponents = GetComponents<IWaterRenderAware>();
			displacingComponents = GetComponents<IWaterDisplacements>();
			if (!(waterMaterial == null))
			{
				CreateParameterHashes();
				if (profiles != null && profiles.Length != 0 && runtimeProfile == profile)
				{
					ResolveProfileData(profiles);
				}
				else if (profile != null)
				{
					runtimeProfile = profile;
					ResolveProfileData(new WeightedProfile[1]
					{
						new WeightedProfile(profile, 1f)
					});
				}
				geometry.OnValidate(this);
				waterRenderer.OnValidate(this);
				SetupMaterials();
			}
		}

		private void ValidateShaders()
		{
			if (waterShader == null)
			{
				waterShader = Shader.Find("PlayWay Water/Standard");
			}
			if (waterVolumeShader == null)
			{
				waterVolumeShader = Shader.Find("PlayWay Water/Standard Volume");
			}
		}

		private void ResolveProfileData(WeightedProfile[] profiles)
		{
			WaterProfile waterProfile = profiles[0].profile;
			float num = 0f;
			for (int i = 0; i < profiles.Length; i++)
			{
				WeightedProfile weightedProfile = profiles[i];
				if (waterProfile == null || num < weightedProfile.weight)
				{
					waterProfile = weightedProfile.profile;
					num = weightedProfile.weight;
				}
			}
			horizontalDisplacementScale = 0f;
			gravity = 0f;
			directionality = 0f;
			density = 0f;
			underwaterBlurSize = 0f;
			underwaterDistortionsIntensity = 0f;
			underwaterDistortionAnimationSpeed = 0f;
			underwaterAbsorptionColor = new Color(0f, 0f, 0f);
			Color value = new Color(0f, 0f, 0f);
			Color value2 = new Color(0f, 0f, 0f);
			Color value3 = new Color(0f, 0f, 0f);
			Color value4 = new Color(0f, 0f, 0f);
			Color value5 = new Color(0f, 0f, 0f);
			Color value6 = new Color(0f, 0f, 0f);
			Color value7 = new Color(0f, 0f, 0f);
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			Vector3 vector = default(Vector3);
			Vector2 vector2 = default(Vector2);
			NormalMapAnimation normalMapAnimation = default(NormalMapAnimation);
			NormalMapAnimation normalMapAnimation2 = default(NormalMapAnimation);
			for (int j = 0; j < profiles.Length; j++)
			{
				WaterProfile waterProfile2 = profiles[j].profile;
				float weight = profiles[j].weight;
				horizontalDisplacementScale += waterProfile2.HorizontalDisplacementScale * weight;
				gravity -= waterProfile2.Gravity * weight;
				directionality += waterProfile2.Directionality * weight;
				density += waterProfile2.Density * weight;
				underwaterBlurSize += waterProfile2.UnderwaterBlurSize * weight;
				underwaterDistortionsIntensity += waterProfile2.UnderwaterDistortionsIntensity * weight;
				underwaterDistortionAnimationSpeed += waterProfile2.UnderwaterDistortionAnimationSpeed * weight;
				underwaterAbsorptionColor += waterProfile2.UnderwaterAbsorptionColor * weight;
				value += waterProfile2.AbsorptionColor * weight;
				value2 += waterProfile2.DiffuseColor * weight;
				value3 += waterProfile2.SpecularColor * weight;
				value4 += waterProfile2.DepthColor * weight;
				value5 += waterProfile2.EmissionColor * weight;
				value6 += waterProfile2.ReflectionColor * weight;
				value7 += waterProfile2.FoamSpecularColor * weight;
				num2 += waterProfile2.Smoothness * weight;
				num3 += waterProfile2.AmbientSmoothness * weight;
				num4 += waterProfile2.SubsurfaceScattering * weight;
				num5 += waterProfile2.RefractionDistortion * weight;
				num6 += waterProfile2.FresnelBias * weight;
				num7 += waterProfile2.DetailFadeDistance * weight;
				num8 += waterProfile2.DisplacementNormalsIntensity * weight;
				num9 += waterProfile2.EdgeBlendFactor * weight;
				num10 += waterProfile2.DirectionalWrapSSS * weight;
				num11 += waterProfile2.PointWrapSSS * weight;
				vector.x += waterProfile2.PlanarReflectionIntensity * weight;
				vector.y += waterProfile2.PlanarReflectionFlatten * weight;
				vector.z += waterProfile2.PlanarReflectionVerticalOffset * weight;
				vector2 += waterProfile2.FoamTiling * weight;
				normalMapAnimation += waterProfile2.NormalMapAnimation1 * weight;
				normalMapAnimation2 += waterProfile2.NormalMapAnimation2 * weight;
			}
			WindWaves component = GetComponent<WindWaves>();
			if (component != null && component.FinalRenderMode == WaveSpectrumRenderMode.GerstnerAndFFTSlope)
			{
				num8 *= 0.5f;
			}
			waterMaterial.SetColor(parameterHashes[0], value);
			waterMaterial.SetColor(parameterHashes[1], value2);
			waterMaterial.SetColor(parameterHashes[2], value3);
			waterMaterial.SetColor(parameterHashes[3], value4);
			waterMaterial.SetColor(parameterHashes[4], value5);
			waterMaterial.SetColor(parameterHashes[5], value6);
			waterMaterial.SetColor(parameterHashes[22], value7);
			waterMaterial.SetFloat(parameterHashes[6], horizontalDisplacementScale);
			waterMaterial.SetFloat(parameterHashes[7], num3);
			waterMaterial.SetVector(parameterHashes[8], new Vector4(num4, 0.15f, 1.65f, 0f));
			waterMaterial.SetVector(parameterHashes[9], new Vector4(num10, 1f / (1f + num10), num11, 1f / (1f + num11)));
			waterMaterial.SetFloat(parameterHashes[10], num5);
			waterMaterial.SetFloat(parameterHashes[11], num6);
			waterMaterial.SetFloat(parameterHashes[12], num7);
			waterMaterial.SetFloat(parameterHashes[13], num8);
			waterMaterial.SetFloat(parameterHashes[14], 1f / num9);
			waterMaterial.SetVector(parameterHashes[15], vector);
			waterMaterial.SetVector(parameterHashes[16], new Vector4(normalMapAnimation.Intensity, normalMapAnimation2.Intensity, (0f - (normalMapAnimation.Intensity + normalMapAnimation2.Intensity)) * 0.5f, 0f));
			waterMaterial.SetVector(parameterHashes[17], new Vector2(vector2.x / normalMapAnimation.Tiling.x, vector2.y / normalMapAnimation.Tiling.y));
			waterMaterial.SetFloat(parameterHashes[18], num2 / num3);
			waterMaterial.SetTexture(parameterHashes[19], waterProfile.NormalMap);
			waterMaterial.SetTexture(parameterHashes[20], waterProfile.FoamDiffuseMap);
			waterMaterial.SetTexture(parameterHashes[21], waterProfile.FoamNormalMap);
			uvAnimator.NormalMapAnimation1 = normalMapAnimation;
			uvAnimator.NormalMapAnimation2 = normalMapAnimation2;
			SetKeyword("_EMISSION", value5.grayscale != 0f);
			if (vectorOverrides != null)
			{
				ApplyOverridenParameters();
			}
			UpdateBackMaterial();
			UpdateWaterVolumeMaterials();
		}

		private void ApplyOverridenParameters()
		{
			for (int i = 0; i < vectorOverrides.Length; i++)
			{
				waterMaterial.SetVector(vectorOverrides[i].hash, vectorOverrides[i].value);
			}
			for (int j = 0; j < floatOverrides.Length; j++)
			{
				waterMaterial.SetFloat(floatOverrides[j].hash, floatOverrides[j].value);
			}
			for (int k = 0; k < colorOverrides.Length; k++)
			{
				waterMaterial.SetColor(colorOverrides[k].hash, colorOverrides[k].value);
			}
			for (int l = 0; l < textureOverrides.Length; l++)
			{
				waterMaterial.SetTexture(textureOverrides[l].hash, textureOverrides[l].value);
			}
		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
				UpdateStatisticalData();
				uvAnimator.Update();
				geometry.Update();
				waterRenderer.Update();
				FireEvents();
				if (launchState != LaunchState.Ready)
				{
					SetupMaterials();
					launchState = LaunchState.Ready;
				}
			}
		}

		public void OnWaterRender(Camera camera)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			Vector2 vector = SurfaceOffset;
			Vector4 value = new Vector4(vector.x, base.transform.position.y, vector.y, UniformWaterScale);
			waterMaterial.SetVector(surfaceOffsetId, value);
			waterBackMaterial.SetVector(surfaceOffsetId, value);
			waterVolumeMaterial.SetVector(surfaceOffsetId, value);
			waterVolumeBackMaterial.SetVector(surfaceOffsetId, value);
			for (int i = 0; i < renderAwareComponents.Length; i++)
			{
				IWaterRenderAware waterRenderAware = renderAwareComponents[i];
				if ((MonoBehaviour)waterRenderAware != null && ((MonoBehaviour)waterRenderAware).enabled)
				{
					waterRenderAware.OnWaterRender(camera);
				}
			}
		}

		public void OnWaterPostRender(Camera camera)
		{
			for (int i = 0; i < renderAwareComponents.Length; i++)
			{
				IWaterRenderAware waterRenderAware = renderAwareComponents[i];
				if ((MonoBehaviour)waterRenderAware != null && ((MonoBehaviour)waterRenderAware).enabled)
				{
					waterRenderAware.OnWaterPostRender(camera);
				}
			}
		}

		internal void OnSamplingStarted()
		{
			activeSamplesCount++;
		}

		internal void OnSamplingStopped()
		{
			activeSamplesCount--;
		}

		private void AddDefaultComponents()
		{
			if (GetComponent<WaterPlanarReflection>() == null)
			{
				base.gameObject.AddComponent<WaterPlanarReflection>();
			}
			if (GetComponent<WindWaves>() == null)
			{
				base.gameObject.AddComponent<WindWaves>();
			}
			if (GetComponent<WaterFoam>() == null)
			{
				base.gameObject.AddComponent<WaterFoam>();
			}
		}

		private bool IsNotCopied()
		{
			return true;
		}

		private void OnQualitySettingsChanged()
		{
			OnValidate();
			profilesDirty = true;
		}

		private void FireEvents()
		{
			if (profilesDirty)
			{
				profilesDirty = false;
				profilesChanged.Invoke(this);
			}
		}

		private void OnProfilesChanged(Water water)
		{
			ResolveProfileData(profiles);
		}

		private void ValidateProfiles(WeightedProfile[] profiles)
		{
			if (profiles.Length == 0)
			{
				throw new ArgumentException("Water has to use at least one profile.");
			}
			float tileSize = profiles[0].profile.TileSize;
			for (int i = 1; i < profiles.Length; i++)
			{
				if (profiles[i].profile.TileSize != tileSize)
				{
					Debug.LogError("TileSize varies between used water profiles. It is the only parameter that you should keep equal on all profiles used at a time.");
					break;
				}
			}
		}

		private void CreateParameterHashes()
		{
			if (parameterHashes == null || parameterHashes.Length != parameterNames.Length)
			{
				surfaceOffsetId = Shader.PropertyToID("_SurfaceOffset");
				int num = parameterNames.Length;
				parameterHashes = new int[num];
				for (int i = 0; i < num; i++)
				{
					parameterHashes[i] = Shader.PropertyToID(parameterNames[i]);
				}
			}
		}

		private void BuildShaderVariant(ShaderVariant variant, WaterQualityLevel qualityLevel)
		{
			if (renderAwareComponents != null)
			{
				bool flag = blendEdges && qualityLevel.allowAlphaBlending;
				bool flag2 = refraction && qualityLevel.allowAlphaBlending;
				bool flag3 = flag2 || flag;
				for (int i = 0; i < renderAwareComponents.Length; i++)
				{
					renderAwareComponents[i].BuildShaderVariant(variant, this, qualityLevel);
				}
				variant.SetWaterKeyword("_WATER_REFRACTION", flag2);
				variant.SetWaterKeyword("_VOLUMETRIC_LIGHTING", volumetricLighting && qualityLevel.allowVolumetricLighting);
				variant.SetWaterKeyword("_CUBEMAP_REFLECTIONS", useCubemapReflections);
				variant.SetWaterKeyword("_NORMALMAP", waterMaterial.GetTexture("_BumpMap") != null);
				variant.SetWaterKeyword("_WATER_RECEIVE_SHADOWS", receiveShadows);
				variant.SetWaterKeyword("_ALPHABLEND_ON", flag3);
				variant.SetWaterKeyword("_ALPHAPREMULTIPLY_ON", !flag3);
				variant.SetUnityKeyword("_BOUNDED_WATER", !volume.Boundless && volume.HasRenderableAdditiveVolumes);
				variant.SetUnityKeyword("_TRIANGLES", geometry.Triangular);
			}
		}

		private void UpdateMaterials()
		{
			WaterQualityLevel currentQualityLevel = WaterQualitySettings.Instance.CurrentQualityLevel;
			for (int i = 0; i < renderAwareComponents.Length; i++)
			{
				renderAwareComponents[i].UpdateMaterial(this, currentQualityLevel);
			}
			bool flag = blendEdges && currentQualityLevel.allowAlphaBlending;
			bool flag2 = (refraction && currentQualityLevel.allowAlphaBlending) || flag;
			waterMaterial.SetFloat("_Cull", 2f);
			waterMaterial.SetOverrideTag("RenderType", (!flag2) ? "Opaque" : "Transparent");
			waterMaterial.SetFloat("_Mode", flag2 ? 2 : 0);
			waterMaterial.SetInt("_SrcBlend", (!flag2) ? 1 : 5);
			waterMaterial.SetInt("_DstBlend", flag2 ? 10 : 0);
			waterMaterial.renderQueue = ((!flag2) ? 2000 : 2990);
			float b = Mathf.Sqrt(2000000f / (float)geometry.TesselatedBaseVertexCount);
			waterMaterial.SetFloat("_TesselationFactor", Mathf.Lerp(1f, b, Mathf.Min(tesselationFactor, currentQualityLevel.maxTesselationFactor)));
			waterMaterial.SetFloat(parameterHashes[23], refractionMaxDepth);
		}

		private void AddShaderVariants(ShaderCollection collection)
		{
			WaterQualityLevel[] qualityLevelsDirect = WaterQualitySettings.Instance.GetQualityLevelsDirect();
			for (int i = 0; i < qualityLevelsDirect.Length; i++)
			{
				SetProgress((float)i / (float)qualityLevelsDirect.Length);
				WaterQualityLevel qualityLevel = qualityLevelsDirect[i];
				ShaderVariant shaderVariant = new ShaderVariant();
				BuildShaderVariant(shaderVariant, qualityLevel);
				collection.GetShaderVariant(shaderVariant.GetWaterKeywords(), shaderVariant.GetUnityKeywords(), shaderVariant.GetAdditionalCode(), shaderVariant.GetKeywordsString(), false);
				AddFallbackVariants(shaderVariant, collection, false, 0);
				SetProgress(((float)i + 0.5f) / (float)qualityLevelsDirect.Length);
				string[] array = disallowedVolumeKeywords;
				foreach (string keyword in array)
				{
					shaderVariant.SetWaterKeyword(keyword, false);
				}
				collection.GetShaderVariant(shaderVariant.GetWaterKeywords(), shaderVariant.GetUnityKeywords(), shaderVariant.GetAdditionalCode(), shaderVariant.GetKeywordsString(), true);
				AddFallbackVariants(shaderVariant, collection, true, 0);
			}
			SetProgress(1f);
		}

		private void SetProgress(float progress)
		{
		}

		private void AddFallbackVariants(ShaderVariant variant, ShaderCollection collection, bool volume, int index)
		{
			if (index < hardwareDependentKeywords.Length)
			{
				string keyword = hardwareDependentKeywords[index];
				AddFallbackVariants(variant, collection, volume, index + 1);
				if (variant.IsWaterKeywordEnabled(keyword))
				{
					variant.SetWaterKeyword(keyword, false);
					AddFallbackVariants(variant, collection, volume, index + 1);
					variant.SetWaterKeyword(keyword, true);
				}
			}
			else
			{
				collection.GetShaderVariant(variant.GetWaterKeywords(), variant.GetUnityKeywords(), variant.GetAdditionalCode(), variant.GetKeywordsString(), volume);
			}
		}

		private void CreateWaterManagers()
		{
			if (uvAnimator == null)
			{
				uvAnimator = new WaterUvAnimator();
			}
			if (volume == null)
			{
				volume = new WaterVolume();
			}
			if (geometry == null)
			{
				geometry = new WaterGeometry();
			}
			if (waterRenderer == null)
			{
				waterRenderer = new WaterRenderer();
			}
			if (profilesChanged == null)
			{
				profilesChanged = new WaterEvent();
			}
		}

		public void Write(ShaderCollection collection)
		{
			if (collection == shaderCollection && waterMaterial != null)
			{
				AddShaderVariants(collection);
			}
		}

		private void UpdateStatisticalData()
		{
			maxHorizontalDisplacement = 0f;
			maxVerticalDisplacement = 0f;
			for (int i = 0; i < displacingComponents.Length; i++)
			{
				maxHorizontalDisplacement += displacingComponents[i].MaxHorizontalDisplacement;
				maxVerticalDisplacement += displacingComponents[i].MaxVerticalDisplacement;
			}
		}

		private void SetVector(VectorParameter parameter, Vector4 value)
		{
			InitializeOverrides();
			int num = parameterHashes[(int)parameter];
			waterMaterial.SetVector(num, value);
			waterBackMaterial.SetVector(num, value);
			waterVolumeMaterial.SetVector(num, value);
			waterVolumeBackMaterial.SetVector(num, value);
			for (int i = 0; i < vectorOverrides.Length; i++)
			{
				if (vectorOverrides[i].hash == num)
				{
					vectorOverrides[i].value = value;
					return;
				}
			}
			Array.Resize(ref vectorOverrides, vectorOverrides.Length + 1);
			vectorOverrides[vectorOverrides.Length - 1] = new ParameterOverride<Vector4>(num, value);
		}

		private void SetColor(ColorParameter parameter, Color value)
		{
			InitializeOverrides();
			int num = parameterHashes[(int)parameter];
			waterMaterial.SetColor(num, value);
			waterBackMaterial.SetColor(num, value);
			waterVolumeMaterial.SetColor(num, value);
			waterVolumeBackMaterial.SetColor(num, value);
			for (int i = 0; i < colorOverrides.Length; i++)
			{
				if (colorOverrides[i].hash == num)
				{
					colorOverrides[i].value = value;
					return;
				}
			}
			Array.Resize(ref colorOverrides, colorOverrides.Length + 1);
			colorOverrides[colorOverrides.Length - 1] = new ParameterOverride<Color>(num, value);
		}

		private void SetFloat(FloatParameter parameter, float value)
		{
			InitializeOverrides();
			int num = parameterHashes[(int)parameter];
			waterMaterial.SetFloat(num, value);
			waterBackMaterial.SetFloat(num, value);
			waterVolumeMaterial.SetFloat(num, value);
			waterVolumeBackMaterial.SetFloat(num, value);
			for (int i = 0; i < floatOverrides.Length; i++)
			{
				if (floatOverrides[i].hash == num)
				{
					floatOverrides[i].value = value;
					return;
				}
			}
			Array.Resize(ref floatOverrides, floatOverrides.Length + 1);
			floatOverrides[floatOverrides.Length - 1] = new ParameterOverride<float>(num, value);
		}

		private void SetTexture(TextureParameter parameter, Texture value)
		{
			InitializeOverrides();
			int num = parameterHashes[(int)parameter];
			waterMaterial.SetTexture(num, value);
			waterBackMaterial.SetTexture(num, value);
			waterVolumeMaterial.SetTexture(num, value);
			waterVolumeBackMaterial.SetTexture(num, value);
			for (int i = 0; i < textureOverrides.Length; i++)
			{
				if (textureOverrides[i].hash == num)
				{
					textureOverrides[i].value = value;
					return;
				}
			}
			Array.Resize(ref textureOverrides, textureOverrides.Length + 1);
			textureOverrides[textureOverrides.Length - 1] = new ParameterOverride<Texture>(num, value);
		}

		public static Water FindWater(Vector3 position, float radius)
		{
			bool isInsideSubtractiveVolume;
			bool isInsideAdditiveVolume;
			return FindWater(position, radius, out isInsideSubtractiveVolume, out isInsideAdditiveVolume);
		}

		public static Water FindWater(Vector3 position, float radius, out bool isInsideSubtractiveVolume, out bool isInsideAdditiveVolume)
		{
			isInsideSubtractiveVolume = false;
			isInsideAdditiveVolume = false;
			int num = Physics.OverlapSphereNonAlloc(position, radius, collidersBuffer, 1 << WaterProjectSettings.Instance.WaterCollidersLayer, QueryTriggerInteraction.Collide);
			possibleWaters.Clear();
			excludedWaters.Clear();
			for (int i = 0; i < num; i++)
			{
				WaterVolumeBase component = collidersBuffer[i].GetComponent<WaterVolumeBase>();
				if (component != null)
				{
					if (component is WaterVolumeAdd)
					{
						isInsideAdditiveVolume = true;
						possibleWaters.Add(component.Water);
					}
					else
					{
						isInsideSubtractiveVolume = true;
						excludedWaters.Add(component.Water);
					}
				}
			}
			for (int j = 0; j < possibleWaters.Count; j++)
			{
				if (!excludedWaters.Contains(possibleWaters[j]))
				{
					return possibleWaters[j];
				}
			}
			List<Water> boundlessWaters = WaterGlobals.Instance.BoundlessWaters;
			int count = boundlessWaters.Count;
			for (int k = 0; k < count; k++)
			{
				if (boundlessWaters[k].Volume.IsPointInsideMainVolume(position, radius) && !excludedWaters.Contains(boundlessWaters[k]))
				{
					return boundlessWaters[k];
				}
			}
			return null;
		}

		private void InitializeOverrides()
		{
			if (vectorOverrides == null)
			{
				vectorOverrides = new ParameterOverride<Vector4>[0];
				floatOverrides = new ParameterOverride<float>[0];
				colorOverrides = new ParameterOverride<Color>[0];
				textureOverrides = new ParameterOverride<Texture>[0];
			}
		}

		[ContextMenu("Print Used Keywords")]
		protected void PrintUsedKeywords()
		{
			Debug.Log(waterMaterial.shader.name);
		}
	}
}
