using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[RequireComponent(typeof(WaterOverlays))]
	public class WavesParticleSystem : MonoBehaviour, IOverlaysRenderer
	{
		[SerializeField]
		[HideInInspector]
		private Shader waterWavesParticlesShader;

		[SerializeField]
		private int maxParticles = 50000;

		[SerializeField]
		private int maxParticlesPerTile = 2000;

		[SerializeField]
		private float prewarmTime = 40f;

		[SerializeField]
		[Tooltip("Allowed execution time per frame.")]
		private float timePerFrame = 0.8f;

		private WaveParticlesQuadtree particles;

		private Water water;

		private Material waterWavesParticlesMaterial;

		private List<IWavesParticleSystemPlugin> plugins;

		private float simulationTime;

		private float timePerFrameExp;

		private bool prewarmed;

		private int lastLinearParticleCostlyUpdate;

		public int ParticleCount
		{
			get
			{
				return particles.Count;
			}
		}

		public WavesParticleSystem()
		{
			plugins = new List<IWavesParticleSystemPlugin>();
		}

		private void Awake()
		{
			water = GetComponent<Water>();
			OnValidate();
		}

		public bool Spawn(WaveParticle particle)
		{
			if (particle != null)
			{
				particle.group = new WaveParticlesGroup(simulationTime);
				particle.group.leftParticle = particle;
				return particles.AddElement(particle);
			}
			return false;
		}

		public bool Spawn(WaveParticle particle, int clones, float waveShapeIrregularity)
		{
			if (particle == null || particles.FreeSpace < clones * 2 + 1)
			{
				return false;
			}
			particle.group = new WaveParticlesGroup(simulationTime);
			particle.baseAmplitude *= water.UniformWaterScale;
			particle.baseFrequency /= water.UniformWaterScale;
			WaveParticle waveParticle = null;
			float min = 1f / waveShapeIrregularity;
			for (int i = -clones; i <= clones; i++)
			{
				WaveParticle waveParticle2 = particle.Clone(particle.position + new Vector2(particle.direction.y, 0f - particle.direction.x) * ((float)i * 1.48f / particle.baseFrequency));
				if (waveParticle2 == null)
				{
					continue;
				}
				waveParticle2.baseAmplitude *= Random.Range(min, 1f);
				waveParticle2.leftNeighbour = waveParticle;
				if (waveParticle != null)
				{
					waveParticle.rightNeighbour = waveParticle2;
					if (i == clones)
					{
						waveParticle2.disallowSubdivision = true;
					}
				}
				else
				{
					waveParticle2.group.leftParticle = waveParticle2;
					waveParticle2.disallowSubdivision = true;
				}
				if (!particles.AddElement(waveParticle2))
				{
					return waveParticle != null;
				}
				waveParticle = waveParticle2;
			}
			return true;
		}

		private void OnEnable()
		{
			CheckResources();
		}

		public void RenderOverlays(WaterOverlaysData overlays)
		{
			if (base.enabled)
			{
				RenderParticles(overlays);
			}
		}

		private void OnDisable()
		{
			FreeResources();
		}

		private void OnValidate()
		{
			timePerFrameExp = Mathf.Exp(timePerFrame * 0.5f);
			if (waterWavesParticlesShader == null)
			{
				waterWavesParticlesShader = Shader.Find("PlayWay Water/Particles/Particles");
			}
		}

		private void LateUpdate()
		{
			if (!prewarmed)
			{
				Prewarm();
			}
			UpdateSimulation(Time.deltaTime);
		}

		public void RegisterPlugin(IWavesParticleSystemPlugin plugin)
		{
			if (!plugins.Contains(plugin))
			{
				plugins.Add(plugin);
			}
		}

		public void UnregisterPlugin(IWavesParticleSystemPlugin plugin)
		{
			plugins.Remove(plugin);
		}

		private void Prewarm()
		{
			prewarmed = true;
			while (simulationTime < prewarmTime)
			{
				UpdateSimulationWithoutFrameBudget(0.1f);
			}
		}

		private void UpdateSimulation(float deltaTime)
		{
			simulationTime += deltaTime;
			UpdatePlugins(deltaTime);
			particles.UpdateSimulation(simulationTime, timePerFrameExp);
		}

		private void UpdateSimulationWithoutFrameBudget(float deltaTime)
		{
			simulationTime += deltaTime;
			UpdatePlugins(deltaTime);
			particles.UpdateSimulation(simulationTime);
		}

		private void UpdatePlugins(float deltaTime)
		{
			int count = plugins.Count;
			for (int i = 0; i < count; i++)
			{
				plugins[i].UpdateParticles(simulationTime, deltaTime);
			}
		}

		private void RenderParticles(WaterOverlaysData overlays)
		{
			if (overlays.UtilityMap == null)
			{
				Graphics.SetRenderTarget(new RenderBuffer[2]
				{
					overlays.DynamicDisplacementMap.colorBuffer,
					overlays.SlopeMap.colorBuffer
				}, overlays.DynamicDisplacementMap.depthBuffer);
			}
			else
			{
				Graphics.SetRenderTarget(new RenderBuffer[3]
				{
					overlays.DynamicDisplacementMap.colorBuffer,
					overlays.SlopeMap.colorBuffer,
					overlays.UtilityMap.colorBuffer
				}, overlays.DynamicDisplacementMap.depthBuffer);
			}
			Vector4 localMapsShaderCoords = overlays.Camera.LocalMapsShaderCoords;
			float uniformWaterScale = GetComponent<Water>().UniformWaterScale;
			waterWavesParticlesMaterial.SetFloat("_WaterScale", uniformWaterScale);
			waterWavesParticlesMaterial.SetVector("_LocalMapsCoords", localMapsShaderCoords);
			waterWavesParticlesMaterial.SetPass(0);
			particles.Render(overlays.Camera.LocalMapsRect);
		}

		private void CheckResources()
		{
			if (waterWavesParticlesMaterial == null)
			{
				waterWavesParticlesMaterial = new Material(waterWavesParticlesShader);
				waterWavesParticlesMaterial.hideFlags = HideFlags.DontSave;
			}
			if (particles == null)
			{
				particles = new WaveParticlesQuadtree(new Rect(-1000f, -1000f, 2000f, 2000f), maxParticlesPerTile, maxParticles);
			}
		}

		private void FreeResources()
		{
			if (waterWavesParticlesMaterial != null)
			{
				waterWavesParticlesMaterial.Destroy();
				waterWavesParticlesMaterial = null;
			}
		}
	}
}
