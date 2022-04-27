using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterRenderer
	{
		[SerializeField]
		[HideInInspector]
		private Shader volumeFrontShader;

		[SerializeField]
		[HideInInspector]
		private Shader volumeBackShader;

		[SerializeField]
		private Transform reflectionProbeAnchor;

		[SerializeField]
		private bool useSharedMask = true;

		private Water water;

		private List<Renderer> masks = new List<Renderer>();

		private Dictionary<Camera, MaterialPropertyBlock> propertyBlocks = new Dictionary<Camera, MaterialPropertyBlock>();

		public int MaskCount
		{
			get
			{
				return masks.Count;
			}
		}

		public Transform ReflectionProbeAnchor
		{
			get
			{
				return reflectionProbeAnchor;
			}
			set
			{
				reflectionProbeAnchor = value;
			}
		}

		internal void OnEnable(Water water)
		{
			this.water = water;
			useSharedMask = true;
		}

		internal void OnDisable()
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnSomeCameraPreCull));
			ReleaseTemporaryBuffers();
		}

		internal void Update()
		{
			ReleaseTemporaryBuffers();
		}

		public void AddMask(Renderer mask)
		{
			mask.enabled = false;
			masks.Add(mask);
		}

		public void RemoveMask(Renderer mask)
		{
			masks.Remove(mask);
		}

		internal void OnValidate(Water water)
		{
			if (volumeFrontShader == null)
			{
				volumeFrontShader = Shader.Find("PlayWay Water/Volumes/Front");
			}
			if (volumeBackShader == null)
			{
				volumeBackShader = Shader.Find("PlayWay Water/Volumes/Back");
			}
		}

		private MaterialPropertyBlock GetMaterialPropertyBlock(Camera camera)
		{
			MaterialPropertyBlock value;
			if (!propertyBlocks.TryGetValue(camera, out value))
			{
				value = (propertyBlocks[camera] = new MaterialPropertyBlock());
			}
			return value;
		}

		public void Render(Camera camera, WaterGeometryType geometryType)
		{
			if (water == null || water.WaterMaterial == null || !water.isActiveAndEnabled || (camera.cullingMask & (1 << water.gameObject.layer)) == 0)
			{
				return;
			}
			WaterCamera waterCamera = WaterCamera.GetWaterCamera(camera);
			if (!water.Volume.Boundless && water.Volume.HasRenderableAdditiveVolumes && ((object)waterCamera == null || !waterCamera.RenderVolumes))
			{
				return;
			}
			MaterialPropertyBlock properties = (((object)waterCamera != null && waterCamera.IsEffectCamera && !(waterCamera.MainCamera == null)) ? GetMaterialPropertyBlock(waterCamera.MainCamera) : GetMaterialPropertyBlock(camera));
			if ((object)waterCamera != null && water.ReceiveShadows)
			{
				Vector2 min = new Vector2(0f, 0f);
				Vector2 max = new Vector2(1f, 1f);
				waterCamera.ReportShadowedWaterMinMaxRect(min, max);
			}
			water.OnWaterRender(camera);
			Matrix4x4 matrix;
			Mesh[] transformedMeshes = water.Geometry.GetTransformedMeshes(camera, out matrix, geometryType, false, ((object)waterCamera != null) ? waterCamera.ForcedVertexCount : 0);
			for (int i = 0; i < transformedMeshes.Length; i++)
			{
				Graphics.DrawMesh(transformedMeshes[i], matrix, water.WaterMaterial, water.gameObject.layer, camera, 0, properties, water.ShadowCastingMode, false, (!(reflectionProbeAnchor == null)) ? reflectionProbeAnchor : water.transform);
				if ((object)waterCamera == null || (waterCamera.ContainingWater != null && !waterCamera.IsEffectCamera))
				{
					Graphics.DrawMesh(transformedMeshes[i], matrix, water.WaterBackMaterial, water.gameObject.layer, camera, 0, properties, water.ShadowCastingMode, false, (!(reflectionProbeAnchor == null)) ? reflectionProbeAnchor : water.transform);
				}
			}
		}

		public void PostRender(Camera camera)
		{
			if (water != null)
			{
				water.OnWaterPostRender(camera);
			}
		}

		public void OnSharedSubtractiveMaskRender(ref bool hasSubtractiveVolumes, ref bool hasAdditiveVolumes, ref bool hasFlatMasks)
		{
			List<WaterVolumeAdd> volumesDirect = water.Volume.GetVolumesDirect();
			int count = volumesDirect.Count;
			for (int i = 0; i < count; i++)
			{
				volumesDirect[i].DisableRenderers();
			}
			List<WaterVolumeSubtract> subtractiveVolumesDirect = water.Volume.GetSubtractiveVolumesDirect();
			int count2 = subtractiveVolumesDirect.Count;
			if (useSharedMask)
			{
				for (int j = 0; j < count2; j++)
				{
					subtractiveVolumesDirect[j].EnableRenderers(false);
				}
				int count3 = masks.Count;
				for (int k = 0; k < count3; k++)
				{
					masks[k].enabled = true;
				}
				hasSubtractiveVolumes = hasSubtractiveVolumes || water.Volume.GetSubtractiveVolumesDirect().Count != 0;
				hasAdditiveVolumes = hasAdditiveVolumes || count != 0;
				hasFlatMasks = hasFlatMasks || count3 != 0;
			}
			else
			{
				for (int l = 0; l < count2; l++)
				{
					subtractiveVolumesDirect[l].DisableRenderers();
				}
			}
		}

		public void OnSharedMaskAdditiveRender()
		{
			if (useSharedMask)
			{
				List<WaterVolumeAdd> volumesDirect = water.Volume.GetVolumesDirect();
				int count = volumesDirect.Count;
				for (int i = 0; i < count; i++)
				{
					volumesDirect[i].EnableRenderers(false);
				}
				List<WaterVolumeSubtract> subtractiveVolumesDirect = water.Volume.GetSubtractiveVolumesDirect();
				int count2 = subtractiveVolumesDirect.Count;
				for (int j = 0; j < count2; j++)
				{
					subtractiveVolumesDirect[j].DisableRenderers();
				}
			}
		}

		public void OnSharedMaskPostRender()
		{
			List<WaterVolumeAdd> volumesDirect = water.Volume.GetVolumesDirect();
			int count = volumesDirect.Count;
			for (int i = 0; i < count; i++)
			{
				volumesDirect[i].EnableRenderers(true);
			}
			List<WaterVolumeSubtract> subtractiveVolumesDirect = water.Volume.GetSubtractiveVolumesDirect();
			int count2 = subtractiveVolumesDirect.Count;
			for (int j = 0; j < count2; j++)
			{
				subtractiveVolumesDirect[j].EnableRenderers(true);
			}
			if (useSharedMask)
			{
				int count3 = masks.Count;
				for (int k = 0; k < count3; k++)
				{
					masks[k].enabled = false;
				}
			}
		}

		private void OnSomeCameraPreCull(Camera camera)
		{
		}

		private void ReleaseTemporaryBuffers()
		{
		}
	}
}
