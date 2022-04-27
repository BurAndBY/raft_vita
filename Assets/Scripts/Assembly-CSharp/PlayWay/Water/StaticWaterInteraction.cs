using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class StaticWaterInteraction : MonoBehaviour, IWaterShore, IWaterInteraction
	{
		[SerializeField]
		[HideInInspector]
		private Shader maskGenerateShader;

		[SerializeField]
		[HideInInspector]
		private Shader maskDisplayShader;

		[HideInInspector]
		[SerializeField]
		private Shader heightMapperShader;

		[SerializeField]
		[HideInInspector]
		private Shader heightMapperShaderAlt;

		[SerializeField]
		[Tooltip("Specifies a distance from the shore over which a water gets one meter deeper (value of 50 means that water has a depth of 1m at a distance of 50m from the shore).")]
		[Range(0.001f, 80f)]
		private float shoreSmoothness = 50f;

		[Tooltip("If set to true, geometry that floats above water is correctly ignored.\n\nUse for objects that are closed and have faces at the bottom like basic primitives and custom meshes, but not terrain.")]
		[SerializeField]
		private bool hasBottomFaces;

		[SerializeField]
		private int resolution = 1024;

		private RenderTexture intensityMask;

		private MeshRenderer shorelineRenderer;

		private int resolutionSqr;

		private Bounds bounds;

		private Bounds totalBounds;

		private float[] heightMapData;

		private float offsetX;

		private float offsetZ;

		private float scaleX;

		private float scaleZ;

		private float terrainSize;

		private int width;

		private int height;

		private static Mesh quadMesh;

		public static List<StaticWaterInteraction> staticWaterInteractions = new List<StaticWaterInteraction>();

		private GameObject[] gameObjects;

		private Terrain[] terrains;

		private int[] originalRendererLayers;

		private float[] originalTerrainPixelErrors;

		public Bounds Bounds
		{
			get
			{
				return totalBounds;
			}
		}

		public Texture IntensityMask
		{
			get
			{
				return intensityMask;
			}
		}

		public Renderer InteractionRenderer
		{
			get
			{
				return shorelineRenderer;
			}
		}

		public int Layer
		{
			get
			{
				return base.gameObject.layer;
			}
		}

		public static int NumStaticWaterInteractions
		{
			get
			{
				return staticWaterInteractions.Count;
			}
		}

		private void Start()
		{
			if (quadMesh == null)
			{
				CreateQuadMesh();
			}
			OnValidate();
			RenderShorelineIntensityMask();
			CreateMaskRenderer();
		}

		private void OnValidate()
		{
			if (maskGenerateShader == null)
			{
				maskGenerateShader = Shader.Find("PlayWay Water/Utility/ShorelineMaskGenerate");
			}
			if (maskDisplayShader == null)
			{
				maskDisplayShader = Shader.Find("PlayWay Water/Utility/ShorelineMaskRender");
			}
			if (heightMapperShader == null)
			{
				heightMapperShader = Shader.Find("PlayWay Water/Utility/HeightMapper");
			}
			if (heightMapperShaderAlt == null)
			{
				heightMapperShaderAlt = Shader.Find("PlayWay Water/Utility/HeightMapperAlt");
			}
		}

		private void OnEnable()
		{
			staticWaterInteractions.Add(this);
			WaterOverlays.RegisterInteraction(this);
		}

		private void OnDisable()
		{
			WaterOverlays.UnregisterInteraction(this);
			staticWaterInteractions.Remove(this);
		}

		private void RenderShorelineIntensityMask()
		{
			try
			{
				PrepareRenderers();
				float num = 1f / shoreSmoothness;
				float num2 = 80f / num;
				totalBounds = bounds;
				totalBounds.Expand(new Vector3(num2, 0f, num2));
				float y = base.transform.position.y;
				RenderTexture renderTexture = RenderHeightMap(resolution, resolution);
				if (intensityMask == null)
				{
					intensityMask = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
					intensityMask.hideFlags = HideFlags.DontSave;
				}
				offsetX = 0f - totalBounds.min.x;
				offsetZ = 0f - totalBounds.min.z;
				scaleX = (float)resolution / totalBounds.size.x;
				scaleZ = (float)resolution / totalBounds.size.z;
				width = resolution;
				height = resolution;
				RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				RenderTexture temporary2 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
				Material material = new Material(maskGenerateShader);
				material.SetVector("_ShorelineExtendRange", new Vector2(totalBounds.size.x / bounds.size.x - 1f, totalBounds.size.z / bounds.size.z - 1f));
				material.SetFloat("_TerrainMinPoint", y);
				material.SetFloat("_Steepness", Mathf.Max(totalBounds.size.x, totalBounds.size.z) * num);
				RenderTexture temporary3 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
				RenderTexture temporary4 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
				Graphics.Blit(renderTexture, temporary3, material, 2);
				ComputeDistanceMap(material, temporary3, temporary4);
				RenderTexture.ReleaseTemporary(temporary3);
				material.SetTexture("_DistanceMap", temporary4);
				Graphics.Blit(renderTexture, temporary, material, 0);
				RenderTexture.ReleaseTemporary(renderTexture);
				RenderTexture.ReleaseTemporary(temporary4);
				Graphics.Blit(temporary, temporary2);
				ReadBackHeightMap(temporary);
				Graphics.Blit(temporary, intensityMask, material, 1);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(temporary);
				Object.Destroy(material);
			}
			finally
			{
				RestoreRenderers();
			}
		}

		private void PrepareRenderers()
		{
			bounds = default(Bounds);
			List<GameObject> list = new List<GameObject>();
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(false);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				StaticWaterInteraction component = componentsInChildren[i].GetComponent<StaticWaterInteraction>();
				if (component == null || component == this)
				{
					list.Add(componentsInChildren[i].gameObject);
					bounds.Encapsulate(componentsInChildren[i].bounds);
				}
			}
			terrains = GetComponentsInChildren<Terrain>(false);
			originalTerrainPixelErrors = new float[terrains.Length];
			for (int j = 0; j < terrains.Length; j++)
			{
				originalTerrainPixelErrors[j] = terrains[j].heightmapPixelError;
				StaticWaterInteraction component2 = terrains[j].GetComponent<StaticWaterInteraction>();
				if (component2 == null || component2 == this)
				{
					list.Add(terrains[j].gameObject);
					terrains[j].heightmapPixelError = 1f;
					bounds.Encapsulate(terrains[j].transform.position);
					bounds.Encapsulate(terrains[j].transform.position + terrains[j].terrainData.size);
				}
			}
			gameObjects = list.ToArray();
			originalRendererLayers = new int[gameObjects.Length];
			for (int k = 0; k < gameObjects.Length; k++)
			{
				originalRendererLayers[k] = gameObjects[k].layer;
				gameObjects[k].layer = WaterProjectSettings.Instance.WaterTempLayer;
			}
		}

		private void RestoreRenderers()
		{
			if (terrains != null)
			{
				for (int i = 0; i < terrains.Length; i++)
				{
					terrains[i].heightmapPixelError = originalTerrainPixelErrors[i];
				}
			}
			if (gameObjects != null)
			{
				for (int num = gameObjects.Length - 1; num >= 0; num--)
				{
					gameObjects[num].layer = originalRendererLayers[num];
				}
			}
		}

		private RenderTexture RenderHeightMap(int width, int height)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 32, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
			temporary.wrapMode = TextureWrapMode.Clamp;
			RenderTexture.active = temporary;
			GL.Clear(true, true, new Color(-4000f, -4000f, -4000f, -4000f), 1000000f);
			RenderTexture.active = null;
			GameObject gameObject = new GameObject();
			Camera camera = gameObject.AddComponent<Camera>();
			camera.enabled = false;
			camera.clearFlags = CameraClearFlags.Nothing;
			camera.depthTextureMode = DepthTextureMode.None;
			camera.orthographic = true;
			camera.cullingMask = 1 << WaterProjectSettings.Instance.WaterTempLayer;
			camera.nearClipPlane = 0.95f;
			camera.farClipPlane = bounds.size.y + 2f;
			camera.orthographicSize = bounds.size.z * 0.5f;
			camera.aspect = bounds.size.x / bounds.size.z;
			Vector3 center = bounds.center;
			center.y = bounds.max.y + 1f;
			camera.transform.position = center;
			camera.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
			camera.targetTexture = temporary;
			camera.RenderWithShader((!hasBottomFaces) ? heightMapperShader : heightMapperShaderAlt, "RenderType");
			camera.targetTexture = null;
			Object.Destroy(gameObject);
			return temporary;
		}

		private static void ComputeDistanceMap(Material material, RenderTexture sa, RenderTexture sb)
		{
			sa.filterMode = FilterMode.Point;
			sb.filterMode = FilterMode.Point;
			material.SetFloat("_Offset1", 1f / (float)Mathf.Max(sa.width, sa.height));
			material.SetFloat("_Offset2", 1.4142135f / (float)Mathf.Max(sa.width, sa.height));
			RenderTexture renderTexture = sa;
			RenderTexture renderTexture2 = sb;
			int num = (int)((float)Mathf.Max(sa.width, sa.height) * 0.7f);
			for (int i = 0; i < num; i++)
			{
				Graphics.Blit(renderTexture, renderTexture2, material, 3);
				RenderTexture renderTexture3 = renderTexture;
				renderTexture = renderTexture2;
				renderTexture2 = renderTexture3;
			}
			if (renderTexture != sb)
			{
				Graphics.Blit(renderTexture, sb, material, 3);
			}
		}

		private void ReadBackHeightMap(RenderTexture source)
		{
			int num = intensityMask.width;
			int num2 = intensityMask.height;
			heightMapData = new float[num * num2 + num + 1];
			RenderTexture.active = source;
			Texture2D texture2D = new Texture2D(intensityMask.width, intensityMask.height, TextureFormat.RGBAFloat, false, true);
			texture2D.ReadPixels(new Rect(0f, 0f, intensityMask.width, intensityMask.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					float num4 = texture2D.GetPixel(j, i).r;
					if (num4 > 0f && num4 < 1f)
					{
						num4 = Mathf.Sqrt(num4);
					}
					heightMapData[num3++] = num4;
				}
			}
			Object.Destroy(texture2D);
		}

		private void CreateMaskRenderer()
		{
			GameObject gameObject = new GameObject("Shoreline Mask");
			gameObject.hideFlags = HideFlags.DontSave;
			gameObject.layer = WaterProjectSettings.Instance.WaterTempLayer;
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = quadMesh;
			Material material = new Material(maskDisplayShader);
			material.hideFlags = HideFlags.DontSave;
			material.SetTexture("_MainTex", intensityMask);
			shorelineRenderer = gameObject.AddComponent<MeshRenderer>();
			shorelineRenderer.sharedMaterial = material;
			shorelineRenderer.enabled = false;
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.position = new Vector3(totalBounds.center.x, 0f, totalBounds.center.z);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = totalBounds.size;
		}

		private static void CreateQuadMesh()
		{
			quadMesh = new Mesh();
			quadMesh.name = "Shoreline Quad Mesh";
			quadMesh.hideFlags = HideFlags.DontSave;
			quadMesh.vertices = new Vector3[4]
			{
				new Vector3(-0.5f, 0f, -0.5f),
				new Vector3(-0.5f, 0f, 0.5f),
				new Vector3(0.5f, 0f, 0.5f),
				new Vector3(0.5f, 0f, -0.5f)
			};
			quadMesh.uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			};
			quadMesh.SetIndices(new int[4] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
			quadMesh.UploadMeshData(true);
		}

		public float GetDepthAt(float x, float z)
		{
			x = (x + offsetX) * scaleX;
			z = (z + offsetZ) * scaleZ;
			int num = (int)x;
			if ((float)num > x)
			{
				num--;
			}
			int num2 = (int)z;
			if ((float)num2 > z)
			{
				num2--;
			}
			if (num >= width || num < 0 || num2 >= height || num2 < 0)
			{
				return 100f;
			}
			x -= (float)num;
			z -= (float)num2;
			int num3 = num2 * width + num;
			float num4 = heightMapData[num3] * (1f - x) + heightMapData[num3 + 1] * x;
			float num5 = heightMapData[num3 + width] * (1f - x) + heightMapData[num3 + width + 1] * x;
			return num4 * (1f - z) + num5 * z;
		}

		public static float GetTotalDepthAt(float x, float z)
		{
			float num = 100f;
			int count = staticWaterInteractions.Count;
			for (int i = 0; i < count; i++)
			{
				float depthAt = staticWaterInteractions[i].GetDepthAt(x, z);
				if (num > depthAt)
				{
					num = depthAt;
				}
			}
			return num;
		}
	}
}
