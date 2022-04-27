using UnityEngine;

namespace PlayWay.Water
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(UnderwaterIME))]
	public class WaterDropsIME : MonoBehaviour, IWaterImageEffect
	{
		[SerializeField]
		[HideInInspector]
		private Shader waterDropsShader;

		[SerializeField]
		[Header("Drops")]
		private Texture2D normalMap;

		[SerializeField]
		private float intensity = 1f;

		[SerializeField]
		[Tooltip("Replace water drops effect with a temporary blur, if you prefer to simulate human vision reaction.")]
		[Header("Blur")]
		private bool useBlur;

		[SerializeField]
		private float blurFadeSpeed = 1f;

		[SerializeField]
		private Blur blur;

		private Material overlayMaterial;

		private RenderTexture maskA;

		private RenderTexture maskB;

		private WaterCamera waterCamera;

		private UnderwaterIME underwaterIME;

		private float disableTime;

		private float blurIntensity;

		private bool effectEnabled = true;

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

		public float Intensity
		{
			get
			{
				return intensity;
			}
			set
			{
				intensity = value;
			}
		}

		public Texture2D NormalMap
		{
			get
			{
				return normalMap;
			}
			set
			{
				normalMap = value;
				if (overlayMaterial != null)
				{
					overlayMaterial.SetTexture("_NormalMap", normalMap);
				}
			}
		}

		private void Awake()
		{
			waterCamera = GetComponent<WaterCamera>();
			underwaterIME = GetComponent<UnderwaterIME>();
			OnValidate();
		}

		private void OnValidate()
		{
			if (waterDropsShader == null)
			{
				waterDropsShader = Shader.Find("PlayWay Water/IME/Water Drops");
			}
			blur.Validate("PlayWay Water/Utilities/Blur (VisionBlur)");
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			CheckResources();
			if (!useBlur)
			{
				Graphics.Blit(maskA, maskB, overlayMaterial, 0);
				overlayMaterial.SetFloat("_Intensity", intensity);
				overlayMaterial.SetTexture("_Mask", maskB);
				overlayMaterial.SetTexture("_SubtractiveMask", waterCamera.SubtractiveMask);
				Graphics.Blit(source, destination, overlayMaterial, 1);
			}
			else
			{
				float size = blur.Size;
				blur.Size *= blurIntensity;
				blur.Apply(source);
				blur.Size = size;
				Graphics.Blit(source, destination);
			}
			SwapMasks();
		}

		private void CheckResources()
		{
			if (overlayMaterial == null)
			{
				overlayMaterial = new Material(waterDropsShader);
				overlayMaterial.hideFlags = HideFlags.DontSave;
				overlayMaterial.SetTexture("_NormalMap", normalMap);
			}
			if (maskA == null || maskA.width != Screen.width >> 1 || maskA.height != Screen.height >> 1)
			{
				maskA = CreateMaskRT();
				maskB = CreateMaskRT();
			}
		}

		private RenderTexture CreateMaskRT()
		{
			RenderTexture renderTexture = new RenderTexture(Screen.width >> 1, Screen.height >> 1, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.filterMode = FilterMode.Bilinear;
			Graphics.SetRenderTarget(renderTexture);
			GL.Clear(false, true, Color.black);
			return renderTexture;
		}

		private void SwapMasks()
		{
			RenderTexture renderTexture = maskA;
			maskA = maskB;
			maskB = renderTexture;
		}

		public void OnWaterCameraEnabled()
		{
		}

		public void OnWaterCameraPreCull()
		{
			if (underwaterIME.enabled)
			{
				disableTime = Time.time + 6f;
			}
			if (useBlur)
			{
				blurIntensity += Mathf.Max(0f, waterCamera.WaterLevel - base.transform.position.y);
				blurIntensity *= 1f - Time.deltaTime * blurFadeSpeed;
				if (blurIntensity > 1f)
				{
					blurIntensity = 1f;
				}
				else if (blurIntensity < 0f)
				{
					blurIntensity = 0f;
				}
				base.enabled = blurIntensity > 0.004f && effectEnabled;
			}
			else
			{
				base.enabled = intensity > 0f && Time.time <= disableTime && effectEnabled;
			}
		}
	}
}
