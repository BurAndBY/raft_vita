using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace PlayWay.Water
{
	[AddComponentMenu("Water/Planar Reflections", 1)]
	[RequireComponent(typeof(Water))]
	[ExecuteInEditMode]
	public class WaterPlanarReflection : MonoBehaviour, IWaterRenderAware
	{
		[HideInInspector]
		[SerializeField]
		private Shader utilitiesShader;

		[SerializeField]
		private Camera reflectionCamera;

		[SerializeField]
		private bool reflectSkybox = true;

		[SerializeField]
		[Range(1f, 8f)]
		private int downsample = 2;

		[Tooltip("Allows you to use more rational resolution of planar reflections on screens with very high dpi. Planar reflections should be blurred anyway.")]
		[Range(1f, 8f)]
		[SerializeField]
		private int retinaDownsample = 3;

		[SerializeField]
		private LayerMask reflectionMask = int.MaxValue;

		[SerializeField]
		private bool highQuality = true;

		[SerializeField]
		private float clipPlaneOffset = 0.07f;

		private Water water;

		private TemporaryRenderTexture currentTarget;

		private bool systemSupportsHDR;

		private int finalDivider;

		private int reflectionTexProperty;

		private bool renderPlanarReflections;

		private Material utilitiesMaterial;

		private Dictionary<Camera, TemporaryRenderTexture> temporaryTargets = new Dictionary<Camera, TemporaryRenderTexture>();

		public bool ReflectSkybox
		{
			get
			{
				return reflectSkybox;
			}
			set
			{
				reflectSkybox = value;
			}
		}

		public LayerMask ReflectionMask
		{
			get
			{
				return reflectionMask;
			}
			set
			{
				if ((int)reflectionMask != (int)value)
				{
					reflectionMask = value;
					if (reflectionCamera != null)
					{
						reflectionCamera.cullingMask = reflectionMask;
					}
				}
			}
		}

		private void Start()
		{
			reflectionTexProperty = Shader.PropertyToID("_PlanarReflectionTex");
			systemSupportsHDR = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
			OnValidate();
		}

		private void OnEnable()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			water = GetComponent<Water>();
			water.ProfilesChanged.AddListener(OnProfilesChanged);
			OnProfilesChanged(water);
			UpdateMaterial(water, WaterQualitySettings.Instance.CurrentQualityLevel);
			WaterQualitySettings.Instance.Changed -= new Action(OnQualityChange);
			WaterQualitySettings.Instance.Changed += new Action(OnQualityChange);
		}

		private void OnDisable()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			water.InvalidateMaterialKeywords();
			WaterQualitySettings.Instance.Changed -= new Action(OnQualityChange);
		}

		private void OnValidate()
		{
			if (utilitiesShader == null)
			{
				utilitiesShader = Shader.Find("PlayWay Water/Utilities/PlanarReflection - Utilities");
			}
			int num = ((!(Screen.dpi <= 220f)) ? retinaDownsample : downsample);
			if (finalDivider != num)
			{
				finalDivider = num;
				ClearRenderTextures();
			}
			if (reflectionCamera != null)
			{
				ValidateReflectionCamera();
			}
			UpdateMaterial(GetComponent<Water>(), WaterQualitySettings.Instance.CurrentQualityLevel);
		}

		private void OnDestroy()
		{
			ClearRenderTextures();
		}

		private void Update()
		{
			ClearRenderTextures();
		}

		public void OnWaterRender(Camera camera)
		{
			if (!(camera == reflectionCamera) && base.enabled && camera.enabled && renderPlanarReflections && !temporaryTargets.TryGetValue(camera, out currentTarget))
			{
				RenderReflection(camera);
				Material waterMaterial = water.WaterMaterial;
				if (waterMaterial != null)
				{
					waterMaterial.SetTexture(reflectionTexProperty, (RenderTexture)currentTarget);
					waterMaterial.SetMatrix("_PlanarReflectionProj", Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(0.5f, 0.5f, 1f)) * reflectionCamera.projectionMatrix * reflectionCamera.worldToCameraMatrix);
					waterMaterial.SetFloat("_PlanarReflectionMipBias", 0f - Mathf.Log(finalDivider, 2f));
				}
			}
		}

		public void OnWaterPostRender(Camera camera)
		{
			TemporaryRenderTexture value;
			if (temporaryTargets.TryGetValue(camera, out value))
			{
				temporaryTargets.Remove(camera);
				value.Dispose();
			}
		}

		private void RenderReflection(Camera camera)
		{
			if (base.enabled)
			{
				if (reflectionCamera == null)
				{
					GameObject gameObject = new GameObject(base.name + " Reflection Camera");
					gameObject.transform.parent = base.transform;
					reflectionCamera = gameObject.AddComponent<Camera>();
					ValidateReflectionCamera();
				}
				reflectionCamera.hdr = systemSupportsHDR && camera.hdr;
				reflectionCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				currentTarget = GetRenderTexture(camera.pixelWidth, camera.pixelHeight);
				temporaryTargets[camera] = currentTarget;
				TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(currentTarget.Texture.width, currentTarget.Texture.height, 16, currentTarget.Texture.format, true, false);
				reflectionCamera.targetTexture = temporary;
				reflectionCamera.aspect = camera.aspect;
				Vector3 eulerAngles = camera.transform.eulerAngles;
				reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
				reflectionCamera.transform.position = camera.transform.position;
				Vector3 position = camera.transform.position;
				position.y = base.transform.position.y - position.y;
				reflectionCamera.transform.position = position;
				float w = 0f - base.transform.position.y - clipPlaneOffset;
				Vector4 plane = new Vector4(0f, 1f, 0f, w);
				Matrix4x4 zero = Matrix4x4.zero;
				zero = CalculateReflectionMatrix(zero, plane);
				Vector3 position2 = zero.MultiplyPoint(camera.transform.position);
				reflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix * zero;
				Vector4 clipPlane = CameraSpacePlane(reflectionCamera, base.transform.position, new Vector3(0f, 1f, 0f), 1f);
				Matrix4x4 projectionMatrix = camera.projectionMatrix;
				projectionMatrix = CalculateObliqueMatrix(projectionMatrix, clipPlane);
				reflectionCamera.projectionMatrix = projectionMatrix;
				reflectionCamera.transform.position = position2;
				Vector3 eulerAngles2 = camera.transform.eulerAngles;
				reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
				reflectionCamera.clearFlags = (reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.Color);
				GL.invertCulling = true;
				reflectionCamera.Render();
				GL.invertCulling = false;
				reflectionCamera.targetTexture = null;
				if (utilitiesMaterial == null)
				{
					utilitiesMaterial = new Material(utilitiesShader);
					utilitiesMaterial.hideFlags = HideFlags.DontSave;
				}
				Graphics.Blit((RenderTexture)temporary, currentTarget, utilitiesMaterial, 0);
				temporary.Dispose();
			}
		}

		private void ValidateReflectionCamera()
		{
			reflectionCamera.enabled = false;
			reflectionCamera.cullingMask = reflectionMask;
			reflectionCamera.renderingPath = RenderingPath.Forward;
			reflectionCamera.depthTextureMode = DepthTextureMode.None;
		}

		private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
		{
			reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMat.m01 = -2f * plane[0] * plane[1];
			reflectionMat.m02 = -2f * plane[0] * plane[2];
			reflectionMat.m03 = -2f * plane[3] * plane[0];
			reflectionMat.m10 = -2f * plane[1] * plane[0];
			reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMat.m12 = -2f * plane[1] * plane[2];
			reflectionMat.m13 = -2f * plane[3] * plane[1];
			reflectionMat.m20 = -2f * plane[2] * plane[0];
			reflectionMat.m21 = -2f * plane[2] * plane[1];
			reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMat.m23 = -2f * plane[3] * plane[2];
			reflectionMat.m30 = 0f;
			reflectionMat.m31 = 0f;
			reflectionMat.m32 = 0f;
			reflectionMat.m33 = 1f;
			return reflectionMat;
		}

		private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 point = pos + normal * clipPlaneOffset;
			Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
			Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
			Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
			return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
		}

		private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
		{
			Vector4 b = projection.inverse * new Vector4(Mathf.Sign(clipPlane.x), Mathf.Sign(clipPlane.y), 1f, 1f);
			Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
			projection[2] = vector.x - projection[3];
			projection[6] = vector.y - projection[7];
			projection[10] = vector.z - projection[11];
			projection[14] = vector.w - projection[15];
			//if (UnityEngine.XR.XRSettings.enabled)
			//{
			//	projection[2] = 0f;
			//	projection[6] = 0f;
			//}
			return projection;
		}

		private TemporaryRenderTexture GetRenderTexture(int width, int height)
		{
			int width2 = Mathf.ClosestPowerOfTwo(width / finalDivider);
			int height2 = Mathf.ClosestPowerOfTwo(height / finalDivider);
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(width2, height2, 0, (reflectionCamera.hdr && systemSupportsHDR) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32, true, false, true);
			temporary.Texture.filterMode = FilterMode.Trilinear;
			temporary.Texture.wrapMode = TextureWrapMode.Clamp;
			return temporary;
		}

		private void ClearRenderTextures()
		{
			Dictionary<Camera, TemporaryRenderTexture>.Enumerator enumerator = temporaryTargets.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Dispose();
			}
			temporaryTargets.Clear();
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.Profiles;
			if (profiles != null)
			{
				float num = 0f;
				Water.WeightedProfile[] array = profiles;
				for (int i = 0; i < array.Length; i++)
				{
					Water.WeightedProfile weightedProfile = array[i];
					WaterProfile profile = weightedProfile.profile;
					float weight = weightedProfile.weight;
					num += profile.PlanarReflectionIntensity * weight;
				}
				renderPlanarReflections = num > 0f;
			}
		}

		private void OnQualityChange()
		{
			UpdateMaterial(water, WaterQualitySettings.Instance.CurrentQualityLevel);
		}

		public void UpdateMaterial(Water water, WaterQualityLevel qualityLevel)
		{
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WaterQualityLevel qualityLevel)
		{
			variant.SetWaterKeyword("_PLANAR_REFLECTIONS", base.enabled && (!highQuality || !qualityLevel.allowHighQualityReflections));
			variant.SetWaterKeyword("_PLANAR_REFLECTIONS_HQ", base.enabled && highQuality && qualityLevel.allowHighQualityReflections);
		}
	}
}
