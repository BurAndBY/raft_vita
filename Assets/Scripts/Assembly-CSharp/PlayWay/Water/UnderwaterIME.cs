using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlayWay.Water
{
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(WaterCamera))]
	public class UnderwaterIME : MonoBehaviour, IWaterImageEffect
	{
		[SerializeField]
		[HideInInspector]
		private Shader underwaterMaskShader;

		[SerializeField]
		[HideInInspector]
		private Shader imeShader;

		[SerializeField]
		[HideInInspector]
		private Shader noiseShader;

		[SerializeField]
		[HideInInspector]
		private Shader composeUnderwaterMaskShader;

		[SerializeField]
		private Blur blur;

		[SerializeField]
		private bool underwaterAudio = true;

		[SerializeField]
		[Range(0f, 4f)]
		[Tooltip("Individual camera blur scale. It's recommended to modify blur scale through water profiles. Use this one, only if some of your cameras need a clear view and some don't.")]
		private float cameraBlurScale = 1f;

		[SerializeField]
		[ColorUsage(false, true, 0f, 3f, 0f, 3f)]
		[Tooltip("Individual camera absorption color. It's recommended to modify absorption color through water profiles. Use this one, only if you fine tune some specific cameras to your needs.")]
		private Color cameraAbsorptionColor = new Color(0f, 0f, 0f, 0f);

		private Material maskMaterial;

		private Material imeMaterial;

		private Material noiseMaterial;

		private Material composeUnderwaterMaskMaterial;

		private Camera localCamera;

		private WaterCamera localWaterCamera;

		private AudioReverbFilter reverbFilter;

		private CommandBuffer maskCommandBuffer;

		private float intensity = float.NaN;

		private bool renderUnderwaterMask;

		private bool effectEnabled = true;

		private int maskRT;

		private int maskRT2;

		public Blur Blur
		{
			get
			{
				return blur;
			}
		}

		public float Intensity
		{
			get
			{
				return intensity;
			}
		}

		public bool EffectEnabled
		{
			get
			{
				return effectEnabled;
			}
			set
			{
				effectEnabled = value;
			}
		}

		private void Awake()
		{
			localCamera = GetComponent<Camera>();
			localWaterCamera = GetComponent<WaterCamera>();
			maskRT = Shader.PropertyToID("_UnderwaterMask");
			maskRT2 = Shader.PropertyToID("_UnderwaterMask2");
			OnValidate();
			maskMaterial = new Material(underwaterMaskShader);
			maskMaterial.hideFlags = HideFlags.DontSave;
			imeMaterial = new Material(imeShader);
			imeMaterial.hideFlags = HideFlags.DontSave;
			noiseMaterial = new Material(noiseShader);
			noiseMaterial.hideFlags = HideFlags.DontSave;
			composeUnderwaterMaskMaterial = new Material(composeUnderwaterMaskShader);
			composeUnderwaterMaskMaterial.hideFlags = HideFlags.DontSave;
			reverbFilter = GetComponent<AudioReverbFilter>();
			if (reverbFilter == null && underwaterAudio)
			{
				reverbFilter = base.gameObject.AddComponent<AudioReverbFilter>();
			}
		}

		public void OnWaterCameraEnabled()
		{
			WaterCamera component = GetComponent<WaterCamera>();
			component.SubmersionStateChanged.AddListener(OnSubmersionStateChanged);
		}

		public void OnWaterCameraPreCull()
		{
			if (!effectEnabled)
			{
				base.enabled = false;
				return;
			}
			switch (localWaterCamera.SubmersionState)
			{
			case SubmersionState.None:
				base.enabled = false;
				break;
			case SubmersionState.Partial:
				base.enabled = true;
				renderUnderwaterMask = true;
				break;
			case SubmersionState.Full:
				base.enabled = true;
				renderUnderwaterMask = false;
				break;
			}
			float num = localCamera.nearClipPlane * Mathf.Tan(localCamera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			float num2 = base.transform.position.y - localWaterCamera.WaterLevel;
			float effectsIntensity = (0f - num2 + num) * 0.25f;
			SetEffectsIntensity(effectsIntensity);
		}

		private void OnDisable()
		{
			if (maskCommandBuffer != null)
			{
				maskCommandBuffer.Clear();
			}
		}

		private void OnDestroy()
		{
			if (maskCommandBuffer != null)
			{
				maskCommandBuffer.Dispose();
				maskCommandBuffer = null;
			}
			if (blur != null)
			{
				blur.Dispose();
			}
			UnityEngine.Object.Destroy(maskMaterial);
			UnityEngine.Object.Destroy(imeMaterial);
		}

		private void OnValidate()
		{
			if (underwaterMaskShader == null)
			{
				underwaterMaskShader = Shader.Find("PlayWay Water/Underwater/Screen-Space Mask");
			}
			if (imeShader == null)
			{
				imeShader = Shader.Find("PlayWay Water/Underwater/Base IME");
			}
			if (noiseShader == null)
			{
				noiseShader = Shader.Find("PlayWay Water/Utilities/Noise");
			}
			if (composeUnderwaterMaskShader == null)
			{
				composeUnderwaterMaskShader = Shader.Find("PlayWay Water/Underwater/Compose Underwater Mask");
			}
			if (blur != null)
			{
				blur.Validate("PlayWay Water/Utilities/Blur (Underwater)");
			}
		}

		private void OnPreCull()
		{
			RenderUnderwaterMask();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!localWaterCamera.enabled || localWaterCamera.ContainingWater == null)
			{
				Graphics.Blit(source, destination);
				return;
			}
			source.filterMode = FilterMode.Bilinear;
			TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(source.width, source.height, 0, (!(destination != null)) ? source.format : destination.format, true, false);
			temporary.Texture.filterMode = FilterMode.Bilinear;
			temporary.Texture.wrapMode = TextureWrapMode.Clamp;
			RenderDepthScatter(source, temporary);
			blur.TotalSize = localWaterCamera.ContainingWater.UnderwaterBlurSize * cameraBlurScale;
			blur.Apply(temporary);
			RenderDistortions(temporary, destination);
			temporary.Dispose();
		}

		private void RenderUnderwaterMask()
		{
			if (maskCommandBuffer == null)
			{
				return;
			}
			maskCommandBuffer.Clear();
			Water containingWater = localWaterCamera.ContainingWater;
			if (renderUnderwaterMask || (containingWater != null && containingWater.Renderer.MaskCount > 0))
			{
				int width = Camera.current.pixelWidth >> 2;
				int height = Camera.current.pixelHeight >> 2;
				maskCommandBuffer.GetTemporaryRT(maskRT, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
				maskCommandBuffer.GetTemporaryRT(maskRT2, width, height, 0, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
			}
			else
			{
				maskCommandBuffer.GetTemporaryRT(maskRT, 4, 4, 0, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
			}
			if (renderUnderwaterMask && containingWater != null)
			{
				maskMaterial.CopyPropertiesFromMaterial(containingWater.WaterMaterial);
				maskCommandBuffer.SetRenderTarget(maskRT2);
				maskCommandBuffer.ClearRenderTarget(false, true, Color.black);
				Matrix4x4 matrix;
				Mesh[] transformedMeshes = containingWater.Geometry.GetTransformedMeshes(localCamera, out matrix, (containingWater.Geometry.GeometryType == WaterGeometry.Type.ProjectionGrid) ? WaterGeometryType.RadialGrid : WaterGeometryType.Auto, true);
				Mesh[] array = transformedMeshes;
				foreach (Mesh mesh in array)
				{
					maskCommandBuffer.DrawMesh(mesh, matrix, maskMaterial);
				}
				maskCommandBuffer.Blit(maskRT2, maskRT, imeMaterial, 4);
				maskCommandBuffer.ReleaseTemporaryRT(maskRT2);
			}
			else
			{
				maskCommandBuffer.SetRenderTarget(maskRT);
				maskCommandBuffer.ClearRenderTarget(false, true, Color.white);
			}
			if (containingWater != null && containingWater.Renderer.MaskCount != 0)
			{
				RenderTexture subtractiveMask = localWaterCamera.SubtractiveMask;
				if (subtractiveMask != null)
				{
					maskCommandBuffer.Blit(subtractiveMask, maskRT, composeUnderwaterMaskMaterial, 0);
				}
			}
			CameraEvent evt = ((localCamera.actualRenderingPath == RenderingPath.Forward) ? CameraEvent.AfterDepthTexture : CameraEvent.AfterLighting);
			localCamera.RemoveCommandBuffer(evt, maskCommandBuffer);
			localCamera.AddCommandBuffer(evt, maskCommandBuffer);
		}

		private void RenderDepthScatter(RenderTexture source, RenderTexture target)
		{
			imeMaterial.CopyPropertiesFromMaterial(localWaterCamera.ContainingWater.WaterMaterial);
			Vector2 surfaceOffset = localWaterCamera.ContainingWater.SurfaceOffset;
			imeMaterial.SetVector("_SurfaceOffset", new Vector3(surfaceOffset.x, localWaterCamera.ContainingWater.transform.position.y, surfaceOffset.y));
			imeMaterial.SetColor("_AbsorptionColor", (cameraAbsorptionColor.maxColorComponent != 0f) ? cameraAbsorptionColor : localWaterCamera.ContainingWater.UnderwaterAbsorptionColor);
			imeMaterial.SetMatrix("UNITY_MATRIX_VP_INVERSE", Matrix4x4.Inverse(localCamera.projectionMatrix * localCamera.worldToCameraMatrix));
			Vector4 vector = imeMaterial.GetVector("_SubsurfaceScatteringPack");
			vector.y = 1f;
			vector.z = 2f;
			imeMaterial.SetVector("_SubsurfaceScatteringPack", vector);
			Graphics.Blit(source, target, imeMaterial, 2);
		}

		private void RenderDistortions(RenderTexture source, RenderTexture target)
		{
			float underwaterDistortionsIntensity = localWaterCamera.ContainingWater.UnderwaterDistortionsIntensity;
			if (underwaterDistortionsIntensity > 0f)
			{
				int width = Camera.current.pixelWidth >> 2;
				int height = Camera.current.pixelHeight >> 2;
				TemporaryRenderTexture temporary = RenderTexturesCache.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, true, false);
				RenderDistortionMap(temporary);
				imeMaterial.SetTexture("_DistortionTex", (RenderTexture)temporary);
				imeMaterial.SetFloat("_DistortionIntensity", underwaterDistortionsIntensity);
				Graphics.Blit(source, target, imeMaterial, 3);
				temporary.Dispose();
			}
			else
			{
				Graphics.Blit(source, target);
			}
		}

		private void RenderDistortionMap(RenderTexture target)
		{
			noiseMaterial.SetVector("_Offset", new Vector4(0f, 0f, Time.time * localWaterCamera.ContainingWater.UnderwaterDistortionAnimationSpeed, 0f));
			noiseMaterial.SetVector("_Period", new Vector4(4f, 4f, 4f, 4f));
			Graphics.Blit(null, target, noiseMaterial, 1);
		}

		private void OnSubmersionStateChanged(WaterCamera waterCamera)
		{
			if (waterCamera.SubmersionState != 0)
			{
				if (maskCommandBuffer == null)
				{
					maskCommandBuffer = new CommandBuffer();
					maskCommandBuffer.name = "Render Underwater Mask";
				}
			}
			else if (maskCommandBuffer != null)
			{
				Camera component = GetComponent<Camera>();
				component.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, maskCommandBuffer);
				component.RemoveCommandBuffer(CameraEvent.AfterLighting, maskCommandBuffer);
			}
		}

		private void SetEffectsIntensity(float intensity)
		{
			if (localCamera == null)
			{
				return;
			}
			intensity = Mathf.Clamp01(intensity);
			if (this.intensity != intensity)
			{
				this.intensity = intensity;
				if (reverbFilter != null && underwaterAudio)
				{
					float num = ((!(intensity > 0.05f)) ? intensity : Mathf.Clamp01(intensity + 0.7f));
					reverbFilter.dryLevel = -2000f * num;
					reverbFilter.room = -10000f * (1f - num);
					reverbFilter.roomHF = Mathf.Lerp(-10000f, -4000f, num);
					reverbFilter.decayTime = 1.6f * num;
					reverbFilter.decayHFRatio = 0.1f * num;
					reverbFilter.reflectionsLevel = -449f * num;
					reverbFilter.reverbLevel = 1500f * num;
					reverbFilter.reverbDelay = 0.0259f * num;
				}
			}
		}
	}
}
