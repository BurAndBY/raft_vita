using UnityEngine;

namespace PlayWay.Water
{
	[RequireComponent(typeof(Water))]
	public class WaterNormalMapAnimation : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		private Shader normalMapShader;

		[SerializeField]
		private int resolution = 512;

		[SerializeField]
		private float period = 60f;

		[SerializeField]
		private float animationSpeed = 0.015f;

		[SerializeField]
		private float intensity = 2f;

		private RenderTexture heightMap1;

		private RenderTexture normalMap1;

		private Texture sourceNormalMap;

		private Material normalMapMaterial;

		private int offsetProperty;

		private int periodProperty;

		private Water water;

		private void Start()
		{
			OnValidate();
			heightMap1 = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			heightMap1.wrapMode = TextureWrapMode.Repeat;
			normalMap1 = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			normalMap1.wrapMode = TextureWrapMode.Repeat;
			normalMapMaterial = new Material(normalMapShader);
			normalMapMaterial.hideFlags = HideFlags.DontSave;
			offsetProperty = Shader.PropertyToID("_Offset");
			periodProperty = Shader.PropertyToID("_Period");
			water = GetComponent<Water>();
			sourceNormalMap = water.WaterMaterial.GetTexture("_BumpMap");
			water.WaterMaterial.SetTexture("_BumpMap", normalMap1);
		}

		private void OnValidate()
		{
			if (normalMapShader == null)
			{
				normalMapShader = Shader.Find("PlayWay Water/Utilities/WaterNormalMap");
			}
		}

		private void Update()
		{
			normalMapMaterial.SetVector(offsetProperty, new Vector4(0f, 0f, Time.time * animationSpeed, 0f));
			normalMapMaterial.SetVector(periodProperty, new Vector4(period, period, period, period));
			normalMapMaterial.SetFloat("_Param", intensity);
			Graphics.Blit(sourceNormalMap, normalMap1, normalMapMaterial, 0);
		}
	}
}
