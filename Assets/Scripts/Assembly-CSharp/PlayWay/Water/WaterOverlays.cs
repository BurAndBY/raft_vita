using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class WaterOverlays : MonoBehaviour, IWaterRenderAware
	{
		[HideInInspector]
		[SerializeField]
		private Shader mapLocalDisplacementsShader;

		[SerializeField]
		private int antialiasing = 2;

		[SerializeField]
		private LayerMask interactionMask = int.MaxValue;

		private Water water;

		private Dictionary<Camera, WaterOverlaysData> buffers = new Dictionary<Camera, WaterOverlaysData>();

		private List<Camera> lostCameras = new List<Camera>();

		private IOverlaysRenderer[] overlayRenderers;

		private Material mapLocalDisplacementsMaterial;

		private static List<IWaterInteraction> shorelineRenderers = new List<IWaterInteraction>();

		private void OnEnable()
		{
			water = GetComponent<Water>();
			ValidateWaterComponents();
			if (mapLocalDisplacementsMaterial == null)
			{
				mapLocalDisplacementsMaterial = new Material(mapLocalDisplacementsShader);
				mapLocalDisplacementsMaterial.hideFlags = HideFlags.DontSave;
			}
		}

		private void OnDisable()
		{
			Dictionary<Camera, WaterOverlaysData>.Enumerator enumerator = buffers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Dispose();
			}
			buffers.Clear();
		}

		private void OnValidate()
		{
			if (mapLocalDisplacementsShader == null)
			{
				mapLocalDisplacementsShader = Shader.Find("PlayWay Water/Utility/Map Local Displacements");
			}
		}

		private void Update()
		{
			int num = Time.frameCount - 3;
			Dictionary<Camera, WaterOverlaysData>.Enumerator enumerator = buffers.GetEnumerator();
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value.lastFrameUsed < num)
				{
					enumerator.Current.Value.Dispose();
					lostCameras.Add(enumerator.Current.Key);
				}
			}
			for (int i = 0; i < lostCameras.Count; i++)
			{
				buffers.Remove(lostCameras[i]);
			}
			lostCameras.Clear();
		}

		public void ValidateWaterComponents()
		{
			overlayRenderers = GetComponents<IOverlaysRenderer>();
			int[] array = new int[overlayRenderers.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Type type = overlayRenderers[i].GetType();
				object[] customAttributes = type.GetCustomAttributes(typeof(OverlayRendererOrderAttribute), true);
				if (customAttributes.Length != 0)
				{
					array[i] = ((OverlayRendererOrderAttribute)customAttributes[0]).Priority;
				}
			}
			Array.Sort(array, overlayRenderers);
		}

		public void UpdateMaterial(Water water, WaterQualityLevel qualityLevel)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WaterQualityLevel qualityLevel)
		{
			variant.SetWaterKeyword("_WATER_OVERLAYS", base.enabled);
		}

		public void OnWaterRender(Camera camera)
		{
			WaterCamera component = camera.GetComponent<WaterCamera>();
			if (!(component == null) && !component.IsEffectCamera && base.enabled && Application.isPlaying && !WaterCamera.IsSceneViewCamera(camera))
			{
				WaterOverlaysData cameraOverlaysData = GetCameraOverlaysData(camera);
				cameraOverlaysData.lastFrameUsed = Time.frameCount;
				cameraOverlaysData.ClearOverlays();
				RenderInteractions(cameraOverlaysData);
				for (int i = 0; i < overlayRenderers.Length; i++)
				{
					overlayRenderers[i].RenderOverlays(cameraOverlaysData);
				}
				water.WaterMaterial.SetTexture("_LocalDisplacementMap", cameraOverlaysData.DynamicDisplacementMap);
				water.WaterMaterial.SetTexture("_LocalSlopeMap", cameraOverlaysData.SlopeMap);
				water.WaterMaterial.SetTexture("_TotalDisplacementMap", cameraOverlaysData.GetTotalDisplacementMap());
				water.WaterBackMaterial.SetTexture("_LocalDisplacementMap", cameraOverlaysData.DynamicDisplacementMap);
				water.WaterBackMaterial.SetTexture("_LocalSlopeMap", cameraOverlaysData.SlopeMap);
				water.WaterBackMaterial.SetTexture("_TotalDisplacementMap", cameraOverlaysData.GetTotalDisplacementMap());
			}
		}

		public void RenderTotalDisplacementMap(RenderTexture renderTexture)
		{
			mapLocalDisplacementsMaterial.CopyPropertiesFromMaterial(water.WaterMaterial);
			Graphics.Blit(null, renderTexture, mapLocalDisplacementsMaterial, 0);
		}

		public void OnWaterPostRender(Camera camera)
		{
		}

		public WaterOverlaysData GetCameraOverlaysData(Camera camera)
		{
			WaterOverlaysData value;
			if (!buffers.TryGetValue(camera, out value))
			{
				int resolution = Mathf.NextPowerOfTwo(camera.pixelWidth + camera.pixelHeight >> 1);
				value = (buffers[camera] = new WaterOverlaysData(this, WaterCamera.GetWaterCamera(camera), resolution, antialiasing));
				RenderInteractions(value);
				value.SwapSlopeMaps();
				for (int i = 0; i < overlayRenderers.Length; i++)
				{
					overlayRenderers[i].RenderOverlays(value);
				}
				value.Initialization = false;
			}
			return value;
		}

		private void RenderInteractions(WaterOverlaysData overlays)
		{
			int count = shorelineRenderers.Count;
			if (count == 0)
			{
				return;
			}
			Rect localMapsRect = overlays.Camera.LocalMapsRect;
			if (localMapsRect.width == 0f)
			{
				return;
			}
			Camera effectsCamera = overlays.Camera.EffectsCamera;
			effectsCamera.enabled = false;
			effectsCamera.depthTextureMode = DepthTextureMode.None;
			effectsCamera.orthographic = true;
			effectsCamera.orthographicSize = localMapsRect.width * 0.5f;
			effectsCamera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
			effectsCamera.farClipPlane = 2000f;
			effectsCamera.clearFlags = CameraClearFlags.Nothing;
			effectsCamera.transform.position = new Vector3(localMapsRect.center.x, 1000f, localMapsRect.center.y);
			effectsCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 1f));
			effectsCamera.targetTexture = overlays.SlopeMap;
			for (int i = 0; i < count; i++)
			{
				if (((1 << shorelineRenderers[i].Layer) & (int)interactionMask) != 0)
				{
					shorelineRenderers[i].InteractionRenderer.enabled = true;
				}
			}
			effectsCamera.Render();
			for (int j = 0; j < count; j++)
			{
				shorelineRenderers[j].InteractionRenderer.enabled = false;
			}
		}

		public static void RegisterInteraction(IWaterInteraction renderer)
		{
			shorelineRenderers.Add(renderer);
		}

		public static void UnregisterInteraction(IWaterInteraction renderer)
		{
			shorelineRenderers.Remove(renderer);
		}
	}
}
