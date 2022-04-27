using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace PlayWay.Water
{
	[AddComponentMenu("Water/Water Camera", -1)]
	[ExecuteInEditMode]
	public class WaterCamera : MonoBehaviour
	{
		[Serializable]
		public class WaterCameraEvent : UnityEvent<WaterCamera>
		{
		}

		[HideInInspector]
		[SerializeField]
		private Shader depthBlitCopyShader;

		[SerializeField]
		[HideInInspector]
		private Shader waterDepthShader;

		[SerializeField]
		[HideInInspector]
		private Shader volumeFrontShader;

		[SerializeField]
		[HideInInspector]
		private Shader volumeBackShader;

		[HideInInspector]
		[SerializeField]
		private Shader volumeFrontFastShader;

		[HideInInspector]
		[SerializeField]
		private Shader shadowEnforcerShader;

		[SerializeField]
		private WaterGeometryType geometryType;

		[SerializeField]
		private bool renderWaterDepth = true;

		[Range(0.2f, 1f)]
		[Tooltip("Water has a pretty smooth shape so it's often safe to render it's depth in a lower resolution than the rest of the scene. Although the default value is 1.0, you may probably safely use 0.5 and gain some minor performance boost. If you will encounter any artifacts in masking or image effects, set it back to 1.0.")]
		[SerializeField]
		private float baseEffectsQuality = 1f;

		[SerializeField]
		private bool renderVolumes = true;

		[SerializeField]
		private bool renderFlatMasks = true;

		[SerializeField]
		private bool sharedCommandBuffers;

		[SerializeField]
		[HideInInspector]
		private int forcedVertexCount;

		[SerializeField]
		private WaterCameraEvent submersionStateChanged;

		private RenderTexture waterDepthTexture;

		private RenderTexture subtractiveMaskTexture;

		private RenderTexture additiveMaskTexture;

		private CommandBuffer depthRenderCommands;

		private CommandBuffer cleanUpCommands;

		private WaterCamera baseCamera;

		private Camera effectCamera;

		private Camera mainCamera;

		private Camera thisCamera;

		private Material depthMixerMaterial;

		private RenderTextureFormat waterDepthTextureFormat;

		private RenderTextureFormat blendedDepthTexturesFormat;

		private int waterDepthTextureId;

		private int underwaterMaskId;

		private int additiveMaskId;

		private int subtractiveMaskId;

		private bool isEffectCamera;

		private bool effectsEnabled;

		private IWaterImageEffect[] imageEffects;

		private Rect localMapsRect;

		private Rect localMapsRectPrevious;

		private Rect shadowedWaterRect;

		private int pixelWidth;

		private int pixelHeight;

		private Mesh shadowsEnforcerMesh;

		private Material shadowsEnforcerMaterial;

		private Water containingWater;

		private WaterSample waterSample;

		private float waterLevel;

		private SubmersionState submersionState;

		private bool isInsideSubtractiveVolume;

		private bool isInsideAdditiveVolume;

		private static Dictionary<Camera, WaterCamera> waterCamerasCache = new Dictionary<Camera, WaterCamera>();

		private static List<WaterCamera> enabledWaterCameras = new List<WaterCamera>();

		private static Texture2D underwaterWhiteMask;

		public bool RenderWaterDepth
		{
			get
			{
				return renderWaterDepth;
			}
			set
			{
				renderWaterDepth = value;
			}
		}

		public bool RenderVolumes
		{
			get
			{
				return renderVolumes;
			}
			set
			{
				renderVolumes = value;
			}
		}

		public bool IsEffectCamera
		{
			get
			{
				return isEffectCamera;
			}
			set
			{
				isEffectCamera = value;
			}
		}

		public WaterGeometryType GeometryType
		{
			get
			{
				return geometryType;
			}
			set
			{
				geometryType = value;
			}
		}

		public Rect LocalMapsRect
		{
			get
			{
				return localMapsRect;
			}
		}

		public Rect LocalMapsRectPrevious
		{
			get
			{
				return localMapsRectPrevious;
			}
		}

		public Vector4 LocalMapsShaderCoords
		{
			get
			{
				return new Vector4(0f - localMapsRect.xMin, 0f - localMapsRect.yMin, 1f / localMapsRect.width, localMapsRect.width);
			}
		}

		public int ForcedVertexCount
		{
			get
			{
				return forcedVertexCount;
			}
		}

		public Water ContainingWater
		{
			get
			{
				return (!(baseCamera == null)) ? baseCamera.ContainingWater : ((submersionState == SubmersionState.None) ? null : containingWater);
			}
		}

		public float WaterLevel
		{
			get
			{
				return waterLevel;
			}
		}

		public SubmersionState SubmersionState
		{
			get
			{
				return submersionState;
			}
		}

		public Camera MainCamera
		{
			get
			{
				return mainCamera;
			}
		}

		public static List<WaterCamera> EnabledWaterCameras
		{
			get
			{
				return enabledWaterCameras;
			}
		}

		public Camera EffectsCamera
		{
			get
			{
				if (!isEffectCamera && effectCamera == null)
				{
					CreateEffectsCamera();
				}
				return effectCamera;
			}
		}

		public RenderTexture SubtractiveMask
		{
			get
			{
				return subtractiveMaskTexture;
			}
		}

		public WaterCameraEvent SubmersionStateChanged
		{
			get
			{
				return submersionStateChanged ?? (submersionStateChanged = new WaterCameraEvent());
			}
		}

		public static event Action<WaterCamera> OnPreRender;

		private void Awake()
		{
			waterDepthTextureId = Shader.PropertyToID("_WaterDepthTexture");
			underwaterMaskId = Shader.PropertyToID("_UnderwaterMask");
			additiveMaskId = Shader.PropertyToID("_AdditiveMask");
			subtractiveMaskId = Shader.PropertyToID("_SubtractiveMask");
			if (SystemInfo.graphicsShaderLevel >= 40 && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
			{
				waterDepthTextureFormat = RenderTextureFormat.Depth;
				blendedDepthTexturesFormat = RenderTextureFormat.Depth;
			}
			else
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat) && baseEffectsQuality > 0.2f)
				{
					blendedDepthTexturesFormat = RenderTextureFormat.RFloat;
				}
				else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf))
				{
					blendedDepthTexturesFormat = RenderTextureFormat.RHalf;
				}
				else
				{
					blendedDepthTexturesFormat = RenderTextureFormat.R8;
				}
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
				{
					waterDepthTextureFormat = RenderTextureFormat.Depth;
				}
				else
				{
					waterDepthTextureFormat = blendedDepthTexturesFormat;
				}
			}
			OnValidate();
		}

		private void OnEnable()
		{
			thisCamera = GetComponent<Camera>();
			waterCamerasCache[thisCamera] = this;
			if (!isEffectCamera)
			{
				enabledWaterCameras.Add(this);
				imageEffects = GetComponents<IWaterImageEffect>();
				IWaterImageEffect[] array = imageEffects;
				foreach (IWaterImageEffect waterImageEffect in array)
				{
					waterImageEffect.OnWaterCameraEnabled();
				}
			}
		}

		private void OnDisable()
		{
			if (!isEffectCamera)
			{
				enabledWaterCameras.Remove(this);
			}
			DisableEffects();
			if (effectCamera != null)
			{
				effectCamera.gameObject.Destroy();
				effectCamera = null;
			}
			if (depthMixerMaterial != null)
			{
				depthMixerMaterial.Destroy();
				depthMixerMaterial = null;
			}
			if (waterSample != null)
			{
				waterSample.Stop();
				waterSample = null;
			}
			containingWater = null;
		}

		private void OnDestroy()
		{
			waterCamerasCache.Clear();
		}

		private void OnValidate()
		{
			if (depthBlitCopyShader == null)
			{
				depthBlitCopyShader = Shader.Find("PlayWay Water/Depth/CopyMix");
			}
			if (waterDepthShader == null)
			{
				waterDepthShader = Shader.Find("PlayWay Water/Depth/Water Depth");
			}
			if (volumeFrontShader == null)
			{
				volumeFrontShader = Shader.Find("PlayWay Water/Volumes/Front");
			}
			if (volumeBackShader == null)
			{
				volumeBackShader = Shader.Find("PlayWay Water/Volumes/Back");
			}
			if (volumeFrontFastShader == null)
			{
				volumeFrontFastShader = Shader.Find("PlayWay Water/Volumes/Front Simple");
			}
			if (shadowEnforcerShader == null)
			{
				shadowEnforcerShader = Shader.Find("PlayWay Water/Utility/ShadowEnforcer");
			}
		}

		private void OnPreCull()
		{
			if (!base.enabled)
			{
				return;
			}
			if (WaterCamera.OnPreRender != null)
			{
				WaterCamera.OnPreRender(this);
			}
			if (!isEffectCamera)
			{
				ToggleEffects();
			}
			int baseEffectsWidth = Mathf.RoundToInt((float)thisCamera.pixelWidth * baseEffectsQuality);
			int baseEffectsHeight = Mathf.RoundToInt((float)thisCamera.pixelHeight * baseEffectsQuality);
			if (!isEffectCamera)
			{
				PrepareToRender();
				SetFallbackUnderwaterMask();
			}
			if (effectsEnabled)
			{
				SetLocalMapCoordinates();
			}
			RenderWater();
			if (!effectsEnabled)
			{
				return;
			}
			if (renderVolumes)
			{
				RenderWaterMasks(baseEffectsWidth, baseEffectsHeight);
			}
			else
			{
				SetBlankWaterMasks();
			}
			if (renderWaterDepth)
			{
				RenderWaterDepthBuffer(baseEffectsWidth, baseEffectsHeight);
			}
			if (imageEffects != null && Application.isPlaying)
			{
				IWaterImageEffect[] array = imageEffects;
				foreach (IWaterImageEffect waterImageEffect in array)
				{
					waterImageEffect.OnWaterCameraPreCull();
				}
			}
			if (shadowedWaterRect.xMin < shadowedWaterRect.xMax)
			{
				RenderShadowEnforcers();
			}
		}

		private void OnPostRender()
		{
			if (waterDepthTexture != null)
			{
				RenderTexture.ReleaseTemporary(waterDepthTexture);
				waterDepthTexture = null;
			}
			if (subtractiveMaskTexture != null)
			{
				RenderTexture.ReleaseTemporary(subtractiveMaskTexture);
				subtractiveMaskTexture = null;
			}
			if (additiveMaskTexture != null)
			{
				RenderTexture.ReleaseTemporary(additiveMaskTexture);
				additiveMaskTexture = null;
			}
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				waters[i].Renderer.PostRender(thisCamera);
			}
		}

		internal void ReportShadowedWaterMinMaxRect(Vector2 min, Vector2 max)
		{
			if (shadowedWaterRect.xMin > min.x)
			{
				shadowedWaterRect.xMin = min.x;
			}
			if (shadowedWaterRect.yMin > min.y)
			{
				shadowedWaterRect.yMin = min.y;
			}
			if (shadowedWaterRect.xMax < max.x)
			{
				shadowedWaterRect.xMax = max.x;
			}
			if (shadowedWaterRect.yMax < max.y)
			{
				shadowedWaterRect.yMax = max.y;
			}
		}

		public static WaterCamera GetWaterCamera(Camera camera, bool forceAdd = false)
		{
			WaterCamera value;
			if (!waterCamerasCache.TryGetValue(camera, out value))
			{
				value = camera.GetComponent<WaterCamera>();
				if (value != null)
				{
					waterCamerasCache[camera] = value;
				}
				else if (!forceAdd)
				{
					value = (waterCamerasCache[camera] = null);
				}
				else
				{
					waterCamerasCache[camera] = camera.gameObject.AddComponent<WaterCamera>();
				}
			}
			return value;
		}

		private void RenderWater()
		{
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				waters[i].Renderer.Render(thisCamera, geometryType);
			}
		}

		private void RenderWaterDepthBuffer(int baseEffectsWidth, int baseEffectsHeight)
		{
			if (waterDepthTexture == null)
			{
				waterDepthTexture = RenderTexture.GetTemporary(baseEffectsWidth, baseEffectsHeight, (waterDepthTextureFormat != RenderTextureFormat.Depth) ? 16 : 32, waterDepthTextureFormat, RenderTextureReadWrite.Linear);
				waterDepthTexture.filterMode = ((!(baseEffectsQuality > 0.98f)) ? FilterMode.Bilinear : FilterMode.Point);
				waterDepthTexture.wrapMode = TextureWrapMode.Clamp;
			}
			Camera effectsCamera = EffectsCamera;
			effectsCamera.CopyFrom(thisCamera);
			effectsCamera.GetComponent<WaterCamera>().enabled = true;
			effectsCamera.renderingPath = RenderingPath.Forward;
			effectsCamera.clearFlags = CameraClearFlags.Color;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			effectsCamera.backgroundColor = Color.white;
			effectsCamera.targetTexture = waterDepthTexture;
			effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
			effectsCamera.RenderWithShader(waterDepthShader, "CustomType");
			effectsCamera.targetTexture = null;
			Shader.SetGlobalTexture(waterDepthTextureId, waterDepthTexture);
		}

		private void RenderWaterMasks(int baseEffectsWidth, int baseEffectsHeight)
		{
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			bool hasSubtractiveVolumes = false;
			bool hasAdditiveVolumes = false;
			bool hasFlatMasks = false;
			for (int i = 0; i < count; i++)
			{
				waters[i].Renderer.OnSharedSubtractiveMaskRender(ref hasSubtractiveVolumes, ref hasAdditiveVolumes, ref hasFlatMasks);
			}
			Camera effectsCamera = EffectsCamera;
			effectsCamera.CopyFrom(thisCamera);
			effectsCamera.GetComponent<WaterCamera>().enabled = false;
			effectsCamera.renderingPath = RenderingPath.Forward;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			if (hasSubtractiveVolumes || hasFlatMasks)
			{
				if (subtractiveMaskTexture == null)
				{
					subtractiveMaskTexture = RenderTexture.GetTemporary(baseEffectsWidth, baseEffectsHeight, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
					subtractiveMaskTexture.filterMode = ((!(baseEffectsQuality > 0.98f)) ? FilterMode.Bilinear : FilterMode.Point);
					subtractiveMaskTexture.wrapMode = TextureWrapMode.Clamp;
				}
				Graphics.SetRenderTarget(subtractiveMaskTexture);
				GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
				effectsCamera.targetTexture = subtractiveMaskTexture;
				if (hasSubtractiveVolumes)
				{
					effectsCamera.clearFlags = CameraClearFlags.Nothing;
					effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
					effectsCamera.RenderWithShader((!isInsideSubtractiveVolume) ? volumeFrontFastShader : volumeFrontShader, string.Empty);
					effectsCamera.RenderWithShader(volumeBackShader, string.Empty);
				}
				if (hasFlatMasks && renderFlatMasks)
				{
					effectsCamera.clearFlags = CameraClearFlags.Nothing;
					effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
					effectsCamera.Render();
				}
				for (int j = 0; j < count; j++)
				{
					waters[j].WaterMaterial.SetTexture(subtractiveMaskId, subtractiveMaskTexture);
					waters[j].WaterBackMaterial.SetTexture(subtractiveMaskId, subtractiveMaskTexture);
				}
			}
			if (hasAdditiveVolumes)
			{
				for (int k = 0; k < count; k++)
				{
					waters[k].Renderer.OnSharedMaskAdditiveRender();
				}
				if (additiveMaskTexture == null)
				{
					additiveMaskTexture = RenderTexture.GetTemporary(baseEffectsWidth, baseEffectsHeight, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
					additiveMaskTexture.filterMode = ((!(baseEffectsQuality > 0.98f)) ? FilterMode.Bilinear : FilterMode.Point);
					additiveMaskTexture.wrapMode = TextureWrapMode.Clamp;
				}
				Graphics.SetRenderTarget(additiveMaskTexture);
				GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
				effectsCamera.clearFlags = CameraClearFlags.Nothing;
				effectsCamera.targetTexture = additiveMaskTexture;
				effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterLayer;
				effectsCamera.RenderWithShader((!isInsideAdditiveVolume) ? volumeFrontFastShader : volumeFrontShader, string.Empty);
				effectsCamera.RenderWithShader(volumeBackShader, string.Empty);
				for (int l = 0; l < count; l++)
				{
					waters[l].WaterMaterial.SetTexture(additiveMaskId, additiveMaskTexture);
					waters[l].WaterBackMaterial.SetTexture(additiveMaskId, additiveMaskTexture);
				}
			}
			effectsCamera.targetTexture = null;
			for (int m = 0; m < count; m++)
			{
				waters[m].Renderer.OnSharedMaskPostRender();
			}
		}

		private void SetBlankWaterMasks()
		{
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				Water water = waters[i];
				water.WaterMaterial.SetTexture(subtractiveMaskId, underwaterWhiteMask);
				water.WaterMaterial.SetTexture(additiveMaskId, underwaterWhiteMask);
				water.WaterBackMaterial.SetTexture(subtractiveMaskId, underwaterWhiteMask);
				water.WaterBackMaterial.SetTexture(additiveMaskId, underwaterWhiteMask);
			}
		}

		private void AddDepthRenderingCommands()
		{
			pixelWidth = thisCamera.pixelWidth;
			pixelHeight = thisCamera.pixelHeight;
			if (depthMixerMaterial == null)
			{
				depthMixerMaterial = new Material(depthBlitCopyShader);
				depthMixerMaterial.hideFlags = HideFlags.DontSave;
			}
			Camera component = GetComponent<Camera>();
			if (((component.depthTextureMode | DepthTextureMode.Depth) != 0 && renderWaterDepth) || renderVolumes)
			{
				int num = Shader.PropertyToID("_CameraDepthTexture2");
				int num2 = Shader.PropertyToID("_WaterlessDepthTexture");
				depthRenderCommands = new CommandBuffer();
				depthRenderCommands.name = "Apply Water Depth";
				depthRenderCommands.GetTemporaryRT(num2, pixelWidth, pixelHeight, (blendedDepthTexturesFormat == RenderTextureFormat.Depth) ? 32 : 0, FilterMode.Point, blendedDepthTexturesFormat, RenderTextureReadWrite.Linear);
				if (!IsSceneViewCamera(component))
				{
					depthRenderCommands.Blit(BuiltinRenderTextureType.None, num2, depthMixerMaterial, 0);
				}
				else
				{
					depthRenderCommands.SetRenderTarget(num2);
					depthRenderCommands.ClearRenderTarget(true, true, new Color(10000f, 10000f, 10000f, 10000f));
				}
				depthRenderCommands.GetTemporaryRT(num, pixelWidth, pixelHeight, (blendedDepthTexturesFormat == RenderTextureFormat.Depth) ? 32 : 0, FilterMode.Point, blendedDepthTexturesFormat, RenderTextureReadWrite.Linear);
				depthRenderCommands.SetRenderTarget(num);
				depthRenderCommands.ClearRenderTarget(true, true, Color.white);
				depthRenderCommands.Blit(BuiltinRenderTextureType.None, num, depthMixerMaterial, 1);
				depthRenderCommands.SetGlobalTexture("_CameraDepthTexture", num);
				cleanUpCommands = new CommandBuffer();
				cleanUpCommands.name = "Clean Water Buffers";
				cleanUpCommands.ReleaseTemporaryRT(num);
				cleanUpCommands.ReleaseTemporaryRT(num2);
				component.depthTextureMode |= DepthTextureMode.Depth;
				component.AddCommandBuffer((component.actualRenderingPath == RenderingPath.Forward) ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, depthRenderCommands);
				component.AddCommandBuffer(CameraEvent.AfterEverything, cleanUpCommands);
			}
		}

		private void RemoveDepthRenderingCommands()
		{
			if (depthRenderCommands != null)
			{
				thisCamera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, depthRenderCommands);
				thisCamera.RemoveCommandBuffer(CameraEvent.BeforeLighting, depthRenderCommands);
				depthRenderCommands.Dispose();
				depthRenderCommands = null;
			}
			if (cleanUpCommands != null)
			{
				thisCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cleanUpCommands);
				cleanUpCommands.Dispose();
				cleanUpCommands = null;
			}
			if (!sharedCommandBuffers)
			{
				thisCamera.RemoveAllCommandBuffers();
			}
		}

		private void EnableEffects()
		{
			if (!isEffectCamera)
			{
				effectsEnabled = true;
				AddDepthRenderingCommands();
			}
		}

		private void DisableEffects()
		{
			effectsEnabled = false;
			RemoveDepthRenderingCommands();
		}

		private bool IsWaterPossiblyVisible()
		{
			List<Water> waters = WaterGlobals.Instance.Waters;
			return waters.Count != 0;
		}

		private void CreateEffectsCamera()
		{
			GameObject gameObject = new GameObject(base.name + " Water Effects Camera");
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			effectCamera = gameObject.AddComponent<Camera>();
			effectCamera.enabled = false;
			WaterCamera waterCamera = gameObject.AddComponent<WaterCamera>();
			waterCamera.isEffectCamera = true;
			waterCamera.mainCamera = thisCamera;
			waterCamera.baseCamera = this;
			waterCamera.waterDepthShader = waterDepthShader;
			enabledWaterCameras.Remove(waterCamera);
		}

		private void RenderShadowEnforcers()
		{
			if (shadowsEnforcerMesh == null)
			{
				shadowsEnforcerMesh = new Mesh();
				shadowsEnforcerMesh.hideFlags = HideFlags.DontSave;
				shadowsEnforcerMesh.name = "Water Shadow Enforcer";
				shadowsEnforcerMesh.vertices = new Vector3[4];
				shadowsEnforcerMesh.SetIndices(new int[4] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
				shadowsEnforcerMesh.UploadMeshData(true);
				shadowsEnforcerMaterial = new Material(shadowEnforcerShader);
				shadowsEnforcerMaterial.hideFlags = HideFlags.DontSave;
			}
			Bounds bounds = default(Bounds);
			float distance = QualitySettings.shadowDistance * 0.5f;
			Vector3 point = thisCamera.ViewportPointToRay(new Vector3(shadowedWaterRect.xMin, shadowedWaterRect.yMin, 1f)).GetPoint(distance);
			Vector3 point2 = thisCamera.ViewportPointToRay(new Vector3(shadowedWaterRect.xMax, shadowedWaterRect.yMax, 1f)).GetPoint(distance);
			SetBoundsMinMaxComponentWise(ref bounds, point, point2);
			shadowsEnforcerMesh.bounds = bounds;
			Graphics.DrawMesh(shadowsEnforcerMesh, Matrix4x4.identity, shadowsEnforcerMaterial, 0);
		}

		private void SetBoundsMinMaxComponentWise(ref Bounds bounds, Vector3 a, Vector3 b)
		{
			if (a.x > b.x)
			{
				float x = b.x;
				b.x = a.x;
				a.x = x;
			}
			if (a.y > b.y)
			{
				float y = b.y;
				b.y = a.y;
				a.y = y;
			}
			if (a.z > b.z)
			{
				float z = b.z;
				b.z = a.z;
				a.z = z;
			}
			bounds.SetMinMax(a, b);
		}

		private void PrepareToRender()
		{
			shadowedWaterRect = new Rect(1f, 1f, -1f, -1f);
			float num = thisCamera.nearClipPlane * Mathf.Tan(thisCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f)) * 3f;
			Water water = Water.FindWater(base.transform.position, num, out isInsideSubtractiveVolume, out isInsideAdditiveVolume);
			if (water != containingWater)
			{
				if (containingWater != null && this.submersionState != 0)
				{
					this.submersionState = SubmersionState.None;
					SubmersionStateChanged.Invoke(this);
				}
				containingWater = water;
				this.submersionState = SubmersionState.None;
				if (waterSample != null)
				{
					waterSample.Stop();
					waterSample = null;
				}
				if (water != null && water.Volume.Boundless)
				{
					waterSample = new WaterSample(containingWater, WaterSample.DisplacementMode.Height, 0.4f);
					waterSample.Start(base.transform.position);
				}
			}
			SubmersionState submersionState;
			if (waterSample != null)
			{
				waterLevel = waterSample.GetAndReset(base.transform.position).y;
				submersionState = ((base.transform.position.y - num < waterLevel) ? ((!(base.transform.position.y + num < waterLevel)) ? SubmersionState.Partial : SubmersionState.Full) : SubmersionState.None);
			}
			else
			{
				submersionState = ((containingWater != null) ? SubmersionState.Partial : SubmersionState.None);
			}
			if (submersionState != this.submersionState)
			{
				this.submersionState = submersionState;
				SubmersionStateChanged.Invoke(this);
			}
		}

		private void SetFallbackUnderwaterMask()
		{
			if (underwaterWhiteMask == null)
			{
				Color color = new Color(0f, 0f, 0f, 0f);
				underwaterWhiteMask = new Texture2D(2, 2, TextureFormat.ARGB32, false);
				underwaterWhiteMask.hideFlags = HideFlags.DontSave;
				underwaterWhiteMask.SetPixel(0, 0, color);
				underwaterWhiteMask.SetPixel(1, 0, color);
				underwaterWhiteMask.SetPixel(0, 1, color);
				underwaterWhiteMask.SetPixel(1, 1, color);
				underwaterWhiteMask.Apply(false, true);
			}
			Shader.SetGlobalTexture(underwaterMaskId, underwaterWhiteMask);
		}

		private void ToggleEffects()
		{
			if (!effectsEnabled)
			{
				if (IsWaterPossiblyVisible())
				{
					EnableEffects();
				}
			}
			else if (!IsWaterPossiblyVisible())
			{
				DisableEffects();
			}
			if (effectsEnabled && (thisCamera.pixelWidth != pixelWidth || thisCamera.pixelHeight != pixelHeight))
			{
				DisableEffects();
				EnableEffects();
			}
		}

		private void SetLocalMapCoordinates()
		{
			int num = Mathf.NextPowerOfTwo(thisCamera.pixelWidth + thisCamera.pixelHeight >> 1);
			float num2 = 0f;
			float num3 = 0f;
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				Water water = waters[i];
				num2 += water.MaxVerticalDisplacement;
				float y = water.transform.position.y;
				if (num3 < y)
				{
					num3 = y;
				}
			}
			Vector3 position = thisCamera.transform.position;
			Vector3 pos = WaterUtilities.ViewportWaterPerpendicular(thisCamera);
			Vector3 vector = thisCamera.transform.localToWorldMatrix * WaterUtilities.RaycastPlane(thisCamera, num3, pos);
			Vector3 vector2 = thisCamera.transform.localToWorldMatrix * WaterUtilities.RaycastPlane(thisCamera, num3, new Vector3(0.5f, 0.5f, 0.5f));
			Vector3 vector3 = ((!(vector.sqrMagnitude > vector2.sqrMagnitude)) ? new Vector3(position.x + vector2.x * 3f, 0f, position.z + vector2.z * 3f) : new Vector3(position.x + vector.x * 3f, 0f, position.z + vector.z * 3f));
			Vector3 vector4 = vector3 - position;
			if (vector4.magnitude > thisCamera.farClipPlane * 0.5f)
			{
				vector3 = position + vector4.normalized * thisCamera.farClipPlane * 0.5f;
				vector3.y = 0f;
			}
			Vector3 forward = thisCamera.transform.forward;
			float f = Mathf.Min(1f, forward.y + 1f);
			float num4 = position.y * (1f + 7f * Mathf.Sqrt(f));
			float num5 = num2 * 2.5f;
			float num6 = ((!(num4 > num5)) ? num5 : num4);
			vector3 = new Vector3(position.x + forward.x * num6 * 0.4f, 0f, position.z + forward.z * num6 * 0.4f);
			localMapsRectPrevious = localMapsRect;
			float num7 = num6 / (float)num;
			localMapsRect = new Rect(vector3.x - num6 + num7, vector3.z - num6 + num7, 2f * num6, 2f * num6);
			Shader.SetGlobalVector("_LocalMapsCoordsPrevious", new Vector4(0f - localMapsRectPrevious.xMin, 0f - localMapsRectPrevious.yMin, 1f / localMapsRectPrevious.width, localMapsRectPrevious.width));
			Shader.SetGlobalVector("_LocalMapsCoords", new Vector4(0f - localMapsRect.xMin, 0f - localMapsRect.yMin, 1f / localMapsRect.width, localMapsRect.width));
		}

		public static bool IsSceneViewCamera(Camera camera)
		{
			return false;
		}
	}
}
