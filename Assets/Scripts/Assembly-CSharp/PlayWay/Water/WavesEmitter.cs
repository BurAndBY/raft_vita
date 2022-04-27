using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class WavesEmitter : MonoBehaviour, IWavesParticleSystemPlugin
	{
		public enum WavesSource
		{
			CustomWaveFrequency
		}

		private class SpawnPoint
		{
			public Vector2 position;

			public Vector2 direction;

			public float frequency;

			public float amplitude;

			public float timeInterval;

			public float timeLeft;

			public SpawnPoint(Vector2 position, Vector2 direction, float frequency, float amplitude, float speed)
			{
				this.position = position;
				this.direction = direction;
				this.frequency = frequency;
				this.amplitude = amplitude;
				timeInterval = (float)Math.PI * 2f / speed * UnityEngine.Random.Range(1f, 8f);
				timeLeft = UnityEngine.Random.Range(0f, timeInterval);
			}
		}

		[SerializeField]
		private WavesParticleSystem wavesParticleSystem;

		[SerializeField]
		private WavesSource wavesSource;

		[SerializeField]
		private float wavelength = 120f;

		[SerializeField]
		private float amplitude = 0.6f;

		[SerializeField]
		private float emissionRate = 2f;

		[SerializeField]
		private int width = 8;

		[SerializeField]
		[Range(0f, 180f)]
		private float spectrumCoincidenceRange = 20f;

		[SerializeField]
		[Range(0f, 100f)]
		private int spectrumWavesCount = 30;

		[SerializeField]
		[Tooltip("Affects both waves and emission area width.")]
		private float span = 1000f;

		[SerializeField]
		[Range(1f, 3.5f)]
		private float waveShapeIrregularity = 2f;

		[SerializeField]
		private float lifetime = 200f;

		[SerializeField]
		private bool shoreWaves = true;

		[SerializeField]
		private Vector2 boundsSize = new Vector2(500f, 500f);

		[Range(3f, 80f)]
		[SerializeField]
		private float spawnDepth = 8f;

		[Range(0.01f, 2f)]
		[SerializeField]
		private float emissionFrequencyScale = 1f;

		[SerializeField]
		private float spawnPointsDensity = 1f;

		private SpawnPoint[] spawnPoints;

		private WindWaves windWaves;

		private float nextSpawnTime;

		private float timeStep;

		private void Awake()
		{
			windWaves = wavesParticleSystem.GetComponent<WindWaves>();
			OnValidate();
			wavesParticleSystem.RegisterPlugin(this);
		}

		private void OnEnable()
		{
			OnValidate();
			nextSpawnTime = Time.time + UnityEngine.Random.Range(0f, timeStep);
		}

		public void UpdateParticles(float time, float deltaTime)
		{
			if (base.isActiveAndEnabled && wavesSource == WavesSource.CustomWaveFrequency && time > nextSpawnTime)
			{
				Vector3 position = base.transform.position;
				Vector3 forward = base.transform.forward;
				WaveParticle waveParticle = WaveParticle.Create(new Vector2(position.x, position.z), new Vector2(forward.x, forward.z).normalized, (float)Math.PI * 2f / wavelength, amplitude, lifetime, shoreWaves);
				if (waveParticle != null)
				{
					wavesParticleSystem.Spawn(waveParticle, width, waveShapeIrregularity);
					waveParticle.Destroy();
					waveParticle.AddToCache();
				}
				nextSpawnTime += timeStep;
			}
		}

		private void OnValidate()
		{
			timeStep = wavelength / emissionRate;
		}

		private void UpdateSpawnPoints(float deltaTime)
		{
			deltaTime *= emissionFrequencyScale;
			for (int i = 0; i < spawnPoints.Length; i++)
			{
				SpawnPoint spawnPoint = spawnPoints[i];
				spawnPoint.timeLeft -= deltaTime;
				if (spawnPoint.timeLeft < 0f)
				{
					float num = (float)Math.PI * 2f / spawnPoint.frequency;
					float num2 = span * 0.3f / num;
					int min = Mathf.Max(2, Mathf.RoundToInt(num2 * 0.7f));
					int max = Mathf.Max(2, Mathf.RoundToInt(num2 * 1.429f));
					spawnPoint.timeLeft += spawnPoint.timeInterval;
					Vector2 position = spawnPoint.position + new Vector2(spawnPoint.direction.y, 0f - spawnPoint.direction.x) * UnityEngine.Random.Range((0f - span) * 0.35f, span * 0.35f);
					WaveParticle waveParticle = WaveParticle.Create(position, spawnPoint.direction, spawnPoint.frequency, spawnPoint.amplitude, lifetime, shoreWaves);
					if (waveParticle != null)
					{
						wavesParticleSystem.Spawn(waveParticle, UnityEngine.Random.Range(min, max), waveShapeIrregularity);
						waveParticle.Destroy();
						waveParticle.AddToCache();
					}
				}
			}
		}

		private void CreateShoalingSpawnPoints()
		{
			Bounds bounds = new Bounds(base.transform.position, new Vector3(boundsSize.x, 0f, boundsSize.y));
			float x = bounds.min.x;
			float z = bounds.min.z;
			float x2 = bounds.max.x;
			float z2 = bounds.max.z;
			float num = Mathf.Sqrt(spawnPointsDensity);
			float num2 = Mathf.Max(35f, bounds.size.x / 256f) / num;
			float num3 = Mathf.Max(35f, bounds.size.z / 256f) / num;
			bool[,] array = new bool[32, 32];
			List<SpawnPoint> list = new List<SpawnPoint>();
			GerstnerWave[] array2 = windWaves.SpectrumResolver.SelectShorelineWaves(50, 0f, 360f);
			if (array2.Length == 0)
			{
				spawnPoints = new SpawnPoint[0];
				return;
			}
			float num4 = spawnDepth * 0.85f;
			float num5 = spawnDepth * 1.18f;
			Vector2 vector = default(Vector2);
			for (float num6 = z; num6 < z2; num6 += num3)
			{
				for (float num7 = x; num7 < x2; num7 += num2)
				{
					int num8 = Mathf.FloorToInt(32f * (num7 - x) / (x2 - x));
					int num9 = Mathf.FloorToInt(32f * (num6 - z) / (z2 - z));
					if (array[num8, num9])
					{
						continue;
					}
					float totalDepthAt = StaticWaterInteraction.GetTotalDepthAt(num7, num6);
					if (!(totalDepthAt > num4) || !(totalDepthAt < num5) || !(UnityEngine.Random.value < 0.06f))
					{
						continue;
					}
					array[num8, num9] = true;
					vector.x = StaticWaterInteraction.GetTotalDepthAt(num7 - 3f, num6) - StaticWaterInteraction.GetTotalDepthAt(num7 + 3f, num6);
					vector.y = StaticWaterInteraction.GetTotalDepthAt(num7, num6 - 3f) - StaticWaterInteraction.GetTotalDepthAt(num7, num6 + 3f);
					vector.Normalize();
					GerstnerWave gerstnerWave = array2[0];
					float num10 = Vector2.Dot(vector, array2[0].direction);
					for (int i = 1; i < array2.Length; i++)
					{
						float num11 = Vector2.Dot(vector, array2[i].direction);
						if (num11 > num10)
						{
							num10 = num11;
							gerstnerWave = array2[i];
						}
					}
					list.Add(new SpawnPoint(new Vector2(num7, num6), vector, gerstnerWave.frequency, Mathf.Abs(gerstnerWave.amplitude), gerstnerWave.speed));
				}
			}
			spawnPoints = list.ToArray();
		}

		private void CreateSpectralWavesSpawnPoints()
		{
			Vector3 normalized = base.transform.forward.normalized;
			float num = Mathf.Atan2(normalized.x, normalized.z);
			GerstnerWave[] array = windWaves.SpectrumResolver.SelectShorelineWaves(spectrumWavesCount, num * 57.29578f, spectrumCoincidenceRange);
			spectrumWavesCount = array.Length;
			Vector3 vector = new Vector3(base.transform.position.x + span * 0.5f, 0f, base.transform.position.z + span * 0.5f);
			Vector2 vector2 = new Vector2(vector.x, vector.z);
			List<SpawnPoint> list = new List<SpawnPoint>();
			for (int i = 0; i < spectrumWavesCount; i++)
			{
				GerstnerWave gerstnerWave = array[i];
				if (gerstnerWave.amplitude != 0f)
				{
					Vector2 position = vector2 - gerstnerWave.direction * span * 0.5f;
					list.Add(new SpawnPoint(position, gerstnerWave.direction, gerstnerWave.frequency, Mathf.Abs(gerstnerWave.amplitude), gerstnerWave.speed));
				}
			}
			spawnPoints = list.ToArray();
		}
	}
}
