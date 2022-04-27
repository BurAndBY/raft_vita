using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlayWay.Water
{
	[RequireComponent(typeof(Water))]
	[RequireComponent(typeof(WindWaves))]
	[AddComponentMenu("Water/Spray", 1)]
	public class WaterSpray : MonoBehaviour, IOverlaysRenderer
	{
		public struct Particle
		{
			public Vector3 position;

			public Vector3 velocity;

			public Vector2 lifetime;

			public float offset;

			public float maxIntensity;

			public Particle(Vector3 position, Vector3 velocity, float lifetime, float offset, float maxIntensity)
			{
				this.position = position;
				this.velocity = velocity;
				this.lifetime = new Vector2(lifetime, lifetime);
				this.offset = offset;
				this.maxIntensity = maxIntensity;
			}
		}

		[SerializeField]
		[HideInInspector]
		private Shader sprayTiledGeneratorShader;

		[SerializeField]
		[HideInInspector]
		private Shader sprayLocalGeneratorShader;

		[HideInInspector]
		[SerializeField]
		private Shader sprayToFoamShader;

		[SerializeField]
		[HideInInspector]
		private ComputeShader sprayControllerShader;

		[SerializeField]
		private Material sprayMaterial;

		[SerializeField]
		[Range(16f, 327675f)]
		private int maxParticles = 65535;

		private float spawnThreshold = 1f;

		private float spawnSkipRatio = 0.9f;

		private float scale = 1f;

		private Water water;

		private WindWaves windWaves;

		private WaterOverlays overlays;

		private Material sprayTiledGeneratorMaterial;

		private Material sprayLocalGeneratorMaterial;

		private Material sprayToFoamMaterial;

		private Transform probeAnchor;

		private RenderTexture blankOutput;

		private Texture2D blankWhiteTex;

		private ComputeBuffer particlesA;

		private ComputeBuffer particlesB;

		private ComputeBuffer particlesInfo;

		private ComputeBuffer spawnBuffer;

		private int resolution;

		private Mesh mesh;

		private bool supported;

		private bool resourcesReady;

		private int[] countBuffer = new int[4];

		private float finalSpawnSkipRatio;

		private float skipRatioPrecomp;

		private Particle[] particlesToSpawn = new Particle[10];

		private int numParticlesToSpawn;

		private MaterialPropertyBlock[] propertyBlocks;

		public int MaxParticles
		{
			get
			{
				return maxParticles;
			}
		}

		public int SpawnedParticles
		{
			get
			{
				if (particlesA != null)
				{
					ComputeBuffer.CopyCount(particlesA, particlesInfo, 0);
					particlesInfo.GetData(countBuffer);
					return countBuffer[0];
				}
				return 0;
			}
		}

		public ComputeBuffer ParticlesBuffer
		{
			get
			{
				return particlesB;
			}
		}

		private void Start()
		{
			water = GetComponent<Water>();
			windWaves = GetComponent<WindWaves>();
			windWaves.ResolutionChanged.AddListener(OnResolutionChanged);
			overlays = GetComponent<WaterOverlays>();
			supported = CheckSupport();
			if (!supported)
			{
				base.enabled = false;
			}
		}

		private bool CheckSupport()
		{
			return SystemInfo.supportsComputeShaders && sprayTiledGeneratorShader != null && sprayTiledGeneratorShader.isSupported;
		}

		private void CheckResources()
		{
			if (sprayTiledGeneratorMaterial == null)
			{
				sprayTiledGeneratorMaterial = new Material(sprayTiledGeneratorShader);
				sprayTiledGeneratorMaterial.hideFlags = HideFlags.DontSave;
			}
			if (sprayLocalGeneratorMaterial == null)
			{
				sprayLocalGeneratorMaterial = new Material(sprayLocalGeneratorShader);
				sprayLocalGeneratorMaterial.hideFlags = HideFlags.DontSave;
			}
			if (sprayToFoamMaterial == null)
			{
				sprayToFoamMaterial = new Material(sprayToFoamShader);
				sprayToFoamMaterial.hideFlags = HideFlags.DontSave;
			}
			if (blankOutput == null)
			{
				UpdatePrecomputedParams();
				blankOutput = new RenderTexture(resolution, resolution, 0, SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				blankOutput.filterMode = FilterMode.Point;
				blankOutput.Create();
			}
			if (mesh == null)
			{
				int num = Mathf.Min(maxParticles, 65535);
				mesh = new Mesh();
				mesh.name = "Spray";
				mesh.hideFlags = HideFlags.DontSave;
				mesh.vertices = new Vector3[num];
				int[] array = new int[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = i;
				}
				mesh.SetIndices(array, MeshTopology.Points, 0);
				mesh.bounds = new Bounds(Vector3.zero, new Vector3(10000000f, 10000000f, 10000000f));
			}
			if (propertyBlocks == null)
			{
				int num2 = Mathf.CeilToInt((float)maxParticles / 65535f);
				propertyBlocks = new MaterialPropertyBlock[num2];
				for (int j = 0; j < num2; j++)
				{
					(propertyBlocks[j] = new MaterialPropertyBlock()).SetFloat("_ParticleOffset", j * 65535);
				}
			}
			if (particlesA == null)
			{
				particlesA = new ComputeBuffer(maxParticles, 40, ComputeBufferType.Append);
			}
			if (particlesB == null)
			{
				particlesB = new ComputeBuffer(maxParticles, 40, ComputeBufferType.Append);
			}
			if (particlesInfo == null)
			{
				particlesInfo = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
				int[] data = new int[4] { 0, 1, 0, 0 };
				particlesInfo.SetData(data);
			}
			resourcesReady = true;
		}

		private void Dispose()
		{
			if (blankOutput != null)
			{
				UnityEngine.Object.Destroy(blankOutput);
				blankOutput = null;
			}
			if (particlesA != null)
			{
				particlesA.Dispose();
				particlesA = null;
			}
			if (particlesB != null)
			{
				particlesB.Dispose();
				particlesB = null;
			}
			if (particlesInfo != null)
			{
				particlesInfo.Release();
				particlesInfo = null;
			}
			if (mesh != null)
			{
				UnityEngine.Object.Destroy(mesh);
				mesh = null;
			}
			if (probeAnchor != null)
			{
				UnityEngine.Object.Destroy(probeAnchor.gameObject);
				probeAnchor = null;
			}
			if (spawnBuffer != null)
			{
				spawnBuffer.Release();
				spawnBuffer = null;
			}
			resourcesReady = false;
		}

		private void OnEnable()
		{
			water = GetComponent<Water>();
			water.ProfilesChanged.AddListener(OnProfilesChanged);
			OnProfilesChanged(water);
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnSomeCameraPreCull));
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnSomeCameraPreCull));
		}

		private void OnDisable()
		{
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnSomeCameraPreCull));
			Dispose();
		}

		private void OnValidate()
		{
			if (sprayTiledGeneratorShader == null)
			{
				sprayTiledGeneratorShader = Shader.Find("PlayWay Water/Spray/Generator (Tiles)");
			}
			if (sprayLocalGeneratorShader == null)
			{
				sprayLocalGeneratorShader = Shader.Find("PlayWay Water/Spray/Generator (Local)");
			}
			if (sprayToFoamShader == null)
			{
				sprayToFoamShader = Shader.Find("PlayWay Water/Spray/Spray To Foam");
			}
			UpdatePrecomputedParams();
		}

		private void LateUpdate()
		{
			if (Time.frameCount >= 10)
			{
				if (!resourcesReady)
				{
					CheckResources();
				}
				SwapParticleBuffers();
				ClearParticles();
				UpdateParticles();
				if (overlays == null && Camera.main != null)
				{
					SpawnWindWavesParticlesTiled(Camera.main.transform);
				}
				if (numParticlesToSpawn != 0)
				{
					SpawnCustomParticles(particlesToSpawn, numParticlesToSpawn);
					numParticlesToSpawn = 0;
				}
			}
		}

		private void OnSomeCameraPreCull(Camera camera)
		{
			if (!resourcesReady)
			{
				return;
			}
			WaterCamera waterCamera = WaterCamera.GetWaterCamera(camera);
			if (waterCamera != null)
			{
				sprayMaterial.SetBuffer("_Particles", particlesA);
				sprayMaterial.SetVector("_CameraUp", camera.transform.up);
				sprayMaterial.SetFloat("_SpecularFresnelBias", water.WaterMaterial.GetFloat("_SpecularFresnelBias"));
				sprayMaterial.SetVector("_WrapSubsurfaceScatteringPack", water.WaterMaterial.GetVector("_WrapSubsurfaceScatteringPack"));
				sprayMaterial.SetTexture("_SubtractiveMask", waterCamera.SubtractiveMask);
				sprayMaterial.SetFloat("_UniformWaterScale", water.UniformWaterScale);
				if (probeAnchor == null)
				{
					GameObject gameObject = new GameObject("Spray Probe Anchor");
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					probeAnchor = gameObject.transform;
				}
				probeAnchor.position = camera.transform.position;
				int num = propertyBlocks.Length;
				for (int i = 0; i < num; i++)
				{
					Graphics.DrawMesh(mesh, Matrix4x4.identity, sprayMaterial, 0, camera, 0, propertyBlocks[i], ShadowCastingMode.Off, false, probeAnchor);
				}
			}
		}

		public void SpawnCustomParticle(Particle particle)
		{
			if (base.enabled)
			{
				if (particlesToSpawn.Length <= numParticlesToSpawn)
				{
					Array.Resize(ref particlesToSpawn, particlesToSpawn.Length << 1);
				}
				particlesToSpawn[numParticlesToSpawn] = particle;
				numParticlesToSpawn++;
			}
		}

		public void SpawnCustomParticles(Particle[] particles, int numParticles)
		{
			if (!base.enabled)
			{
				return;
			}
			CheckResources();
			if (spawnBuffer == null || spawnBuffer.count < particles.Length)
			{
				if (spawnBuffer != null)
				{
					spawnBuffer.Release();
				}
				spawnBuffer = new ComputeBuffer(particles.Length, 40);
			}
			spawnBuffer.SetData(particles);
			sprayControllerShader.SetFloat("particleCount", numParticles);
			sprayControllerShader.SetBuffer(2, "SourceParticles", spawnBuffer);
			sprayControllerShader.SetBuffer(2, "TargetParticles", particlesA);
			sprayControllerShader.Dispatch(2, 1, 1, 1);
		}

		private void SpawnWindWavesParticlesTiled(Transform origin)
		{
			Vector3 position = origin.position;
			float num = 400f / (float)blankOutput.width;
			sprayTiledGeneratorMaterial.CopyPropertiesFromMaterial(water.WaterMaterial);
			sprayTiledGeneratorMaterial.SetVector("_SurfaceOffset", -water.SurfaceOffset);
			sprayTiledGeneratorMaterial.SetVector("_Params", new Vector4(spawnThreshold * 0.25835f, skipRatioPrecomp, 0f, scale * 0.455f));
			sprayTiledGeneratorMaterial.SetVector("_Coordinates", new Vector4(position.x - 200f + UnityEngine.Random.value * num, position.z - 200f + UnityEngine.Random.value * num, 400f, 400f));
			if (overlays == null)
			{
				sprayTiledGeneratorMaterial.SetTexture("_LocalSlopeMap", GetBlankWhiteTex());
			}
			Graphics.SetRandomWriteTarget(1, particlesA);
			Graphics.Blit(null, blankOutput, sprayTiledGeneratorMaterial, 0);
			Graphics.ClearRandomWriteTargets();
		}

		private void SpawnWindWavesParticlesLocal(WaterOverlaysData overlays)
		{
			sprayLocalGeneratorMaterial.CopyPropertiesFromMaterial(water.WaterMaterial);
			sprayLocalGeneratorMaterial.SetVector("_SurfaceOffset", -water.SurfaceOffset);
			sprayLocalGeneratorMaterial.SetVector("_Params", new Vector4(spawnThreshold * 0.25835f, spawnSkipRatio, 0f, scale * 0.455f));
			sprayLocalGeneratorMaterial.SetTexture("_TotalDisplacementMap", overlays.GetTotalDisplacementMap());
			sprayLocalGeneratorMaterial.SetTexture("_LocalSlopeMap", overlays.SlopeMapPrevious);
			sprayLocalGeneratorMaterial.SetTexture("_LocalUtilityMap", overlays.UtilityMap);
			float num = overlays.Camera.LocalMapsRect.width / water.UniformWaterScale;
			int value = 7 + Mathf.CeilToInt(num / 650f);
			value = Mathf.Clamp(value, 8, 12);
			sprayLocalGeneratorMaterial.SetInt("_Iterations", value);
			Graphics.SetRandomWriteTarget(1, particlesA);
			Graphics.Blit(null, blankOutput, sprayLocalGeneratorMaterial, 0);
			Graphics.ClearRandomWriteTargets();
		}

		private void GenerateLocalFoam(WaterOverlaysData overlays)
		{
			Graphics.SetRenderTarget(overlays.DynamicDisplacementMap);
			sprayToFoamMaterial.SetBuffer("_Particles", particlesA);
			sprayToFoamMaterial.SetVector("_LocalMapsCoords", overlays.Camera.LocalMapsShaderCoords);
			sprayToFoamMaterial.SetFloat("_UniformWaterScale", water.UniformWaterScale);
			Vector4 vector = sprayMaterial.GetVector("_ParticleParams");
			vector.x *= 8f;
			vector.z = 1f;
			sprayToFoamMaterial.SetVector("_ParticleParams", vector);
			int num = propertyBlocks.Length;
			for (int i = 0; i < num; i++)
			{
				sprayToFoamMaterial.SetFloat("_ParticleOffset", i * 65535);
				sprayToFoamMaterial.SetPass(0);
				Graphics.DrawMeshNow(mesh, Matrix4x4.identity, 0);
			}
		}

		private void UpdateParticles()
		{
			Vector2 vector = windWaves.WindSpeed * 0.0008f;
			Vector3 gravity = Physics.gravity;
			float deltaTime = Time.deltaTime;
			if (overlays != null)
			{
				WaterOverlaysData cameraOverlaysData = overlays.GetCameraOverlaysData(Camera.main);
				sprayControllerShader.SetTexture(0, "LocalSlopeMap", cameraOverlaysData.SlopeMap);
				WaterCamera waterCamera = WaterCamera.GetWaterCamera(Camera.main);
				if (waterCamera != null)
				{
					sprayControllerShader.SetVector("localMapsCoords", waterCamera.LocalMapsShaderCoords);
				}
			}
			else
			{
				sprayControllerShader.SetTexture(0, "LocalSlopeMap", GetBlankWhiteTex());
			}
			sprayControllerShader.SetFloat("deltaTime", deltaTime);
			sprayControllerShader.SetVector("externalForces", new Vector3((vector.x + gravity.x) * deltaTime, gravity.y * deltaTime, (vector.y + gravity.z) * deltaTime));
			sprayControllerShader.SetVector("surfaceOffset", water.WaterMaterial.GetVector("_SurfaceOffset"));
			sprayControllerShader.SetVector("waterTileSize", water.WaterMaterial.GetVector("_WaterTileSize"));
			sprayControllerShader.SetTexture(0, "DisplacedHeightMap", windWaves.WaterWavesFFT.DisplacedHeightMap);
			sprayControllerShader.SetBuffer(0, "SourceParticles", particlesB);
			sprayControllerShader.SetBuffer(0, "TargetParticles", particlesA);
			sprayControllerShader.Dispatch(0, maxParticles / 128, 1, 1);
		}

		private Texture2D GetBlankWhiteTex()
		{
			if (blankWhiteTex == null)
			{
				blankWhiteTex = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
				for (int i = 0; i < 2; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						blankWhiteTex.SetPixel(i, j, new Color(1f, 1f, 1f, 1f));
					}
				}
				blankWhiteTex.Apply(false, true);
			}
			return blankWhiteTex;
		}

		private void ClearParticles()
		{
			sprayControllerShader.SetBuffer(1, "TargetParticlesFlat", particlesA);
			sprayControllerShader.Dispatch(1, maxParticles / 128, 1, 1);
		}

		private void SwapParticleBuffers()
		{
			ComputeBuffer computeBuffer = particlesB;
			particlesB = particlesA;
			particlesA = computeBuffer;
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			if (blankOutput != null)
			{
				UnityEngine.Object.Destroy(blankOutput);
				blankOutput = null;
			}
			resourcesReady = false;
		}

		private void OnProfilesChanged(Water water)
		{
			Water.WeightedProfile[] profiles = water.Profiles;
			spawnThreshold = 0f;
			spawnSkipRatio = 0f;
			scale = 0f;
			if (profiles != null)
			{
				Water.WeightedProfile[] array = profiles;
				for (int i = 0; i < array.Length; i++)
				{
					Water.WeightedProfile weightedProfile = array[i];
					WaterProfile profile = weightedProfile.profile;
					float weight = weightedProfile.weight;
					spawnThreshold += profile.SprayThreshold * weight;
					spawnSkipRatio += profile.SpraySkipRatio * weight;
					scale += profile.SpraySize * weight;
				}
			}
		}

		private void UpdatePrecomputedParams()
		{
			if (water != null)
			{
				resolution = windWaves.FinalResolution;
			}
			skipRatioPrecomp = Mathf.Pow(spawnSkipRatio, 1024f / (float)resolution);
		}

		public void RenderOverlays(WaterOverlaysData overlays)
		{
			if (base.enabled)
			{
				CheckResources();
				SpawnWindWavesParticlesLocal(overlays);
				GenerateLocalFoam(overlays);
			}
		}
	}
}
