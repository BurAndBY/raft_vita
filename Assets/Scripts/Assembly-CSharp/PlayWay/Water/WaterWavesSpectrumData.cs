using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayWay.Water
{
	public class WaterWavesSpectrumData
	{
		private readonly Water water;

		private readonly WindWaves windWaves;

		private readonly WaterWavesSpectrum spectrum;

		private Vector3[][][,] spectrumValues;

		private WaterWave[][][] cpuWaves;

		private WaterWave[] shorelineCandidates;

		private Texture2D texture;

		private float weight;

		private bool cpuWavesDirty = true;

		private float totalAmplitude;

		private Vector2 lastWindDirection;

		private int displayResolutionIndex;

		public Texture2D Texture
		{
			get
			{
				if (texture == null)
				{
					CreateSpectrumTexture();
				}
				return texture;
			}
		}

		public int CpuWavesCount
		{
			get
			{
				int num = 0;
				if (cpuWaves != null)
				{
					WaterWave[][][] array = cpuWaves;
					foreach (WaterWave[][] array2 in array)
					{
						WaterWave[][] array3 = array2;
						foreach (WaterWave[] array4 in array3)
						{
							num += array4.Length;
						}
					}
				}
				return num;
			}
		}

		public float TotalAmplitude
		{
			get
			{
				return totalAmplitude;
			}
		}

		public float Weight
		{
			get
			{
				return weight;
			}
			set
			{
				weight = value;
			}
		}

		public WaterWave[] ShorelineCandidates
		{
			get
			{
				return shorelineCandidates;
			}
		}

		public WaterWavesSpectrumData(Water water, WaterWavesSpectrum spectrum)
		{
			this.water = water;
			windWaves = water.GetComponent<WindWaves>();
			this.spectrum = spectrum;
		}

		public WaterWave[][] GetCpuWaves(int scaleIndex)
		{
			return cpuWaves[scaleIndex];
		}

		public void ValidateSpectrumData()
		{
			if (cpuWaves != null)
			{
				return;
			}
			lock (this)
			{
				if (cpuWaves != null)
				{
					return;
				}
				int finalResolution = windWaves.FinalResolution;
				Vector4 tileSizeScales = windWaves.TileSizeScales;
				displayResolutionIndex = Mathf.RoundToInt(Mathf.Log(finalResolution, 2f)) - 4;
				if (spectrumValues == null)
				{
					spectrumValues = new Vector3[displayResolutionIndex + 1][][,];
				}
				if (spectrumValues.Length <= displayResolutionIndex)
				{
					Array.Resize(ref spectrumValues, displayResolutionIndex + 1);
				}
				Vector3[][,] array;
				if (spectrumValues[displayResolutionIndex] == null)
				{
					array = (spectrumValues[displayResolutionIndex] = new Vector3[4][,]);
					for (int i = 0; i < 4; i++)
					{
						array[i] = new Vector3[finalResolution, finalResolution];
					}
				}
				else
				{
					array = spectrumValues[displayResolutionIndex];
				}
				int num = ((water.Seed == 0) ? UnityEngine.Random.Range(0, 1000000) : water.Seed);
				totalAmplitude = 0f;
				WaterQualityLevel[] qualityLevelsDirect = WaterQualitySettings.Instance.GetQualityLevelsDirect();
				int maxSpectrumResolution = qualityLevelsDirect[qualityLevelsDirect.Length - 1].maxSpectrumResolution;
				if (finalResolution > maxSpectrumResolution)
				{
					Debug.LogWarningFormat("In water quality settings spectrum resolution of {0} is max, but at runtime a spectrum with resolution of {1} is generated. That may generate some unexpected behaviour. Make sure that the last water quality level has the highest resolution and don't alter it at runtime.", maxSpectrumResolution, finalResolution);
				}
				for (byte b = 0; b < 4; b = (byte)(b + 1))
				{
					UnityEngine.Random.seed = num + b;
					spectrum.ComputeSpectrum(array[b], tileSizeScales[b], maxSpectrumResolution, null);
				}
				FindCpuWaves();
			}
		}

		private void FindCpuWaves()
		{
			if (cpuWaves == null)
			{
				cpuWaves = new WaterWave[4][][];
			}
			for (int i = 0; i < 4; i++)
			{
				if (cpuWaves[i] == null)
				{
					cpuWaves[i] = new WaterWave[displayResolutionIndex + 1][];
				}
			}
			int finalResolution = windWaves.FinalResolution;
			int num = finalResolution >> 1;
			int cpuMaxWaves = windWaves.CpuMaxWaves;
			float cpuWaveThreshold = windWaves.CpuWaveThreshold;
			Vector4 tileSizeScales = windWaves.TileSizeScales;
			Heap<WaterWave> heap = new Heap<WaterWave>();
			List<WaterWave>[] array = new List<WaterWave>[displayResolutionIndex + 1];
			for (int j = 0; j <= displayResolutionIndex; j++)
			{
				array[j] = new List<WaterWave>();
			}
			for (byte b = 0; b < 4; b = (byte)(b + 1))
			{
				float num2 = spectrum.TileSize * tileSizeScales[b];
				Vector3[,] array2 = spectrumValues[displayResolutionIndex][b];
				float num3 = (float)Math.PI * 2f / num2;
				float gravity = spectrum.Gravity;
				float offsetX = num2 + 0.5f / (float)finalResolution * num2;
				float offsetZ = 0f - num2 + 0.5f / (float)finalResolution * num2;
				for (int k = 0; k < finalResolution; k++)
				{
					float num4 = num3 * (float)(k - num);
					ushort num5 = (ushort)((k + num) % finalResolution);
					for (int l = 0; l < finalResolution; l++)
					{
						float num6 = num3 * (float)(l - num);
						ushort num7 = (ushort)((l + num) % finalResolution);
						Vector3 vector = array2[num5, num7];
						float num8 = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
						float num9 = Mathf.Sqrt(num4 * num4 + num6 * num6);
						float w = Mathf.Sqrt(gravity * num9);
						float num10 = num8;
						if (num10 < 0f)
						{
							num10 = 0f - num10;
						}
						totalAmplitude += num8;
						if (num8 >= cpuWaveThreshold)
						{
							int mipIndex = GetMipIndex(Mathf.Max(Mathf.Min(num5, finalResolution - num5 - 1), Mathf.Min(num7, finalResolution - num7 - 1)));
							array[mipIndex].Add(new WaterWave(b, offsetX, offsetZ, num5, num7, num4, num6, num9, w, num8, num10));
						}
						if (num8 > 0.025f)
						{
							float cpuPriority = num9 / num8;
							heap.Insert(new WaterWave(b, offsetX, offsetZ, num5, num7, num4, num6, num9, w, num8, cpuPriority));
							if (heap.Count > 200)
							{
								heap.ExtractMax();
							}
						}
					}
				}
				lock (cpuWaves)
				{
					for (int m = 0; m <= displayResolutionIndex; m++)
					{
						cpuWaves[b][m] = array[m].ToArray();
						array[m].Clear();
						SortCpuWaves(cpuWaves[b][m], false);
						if (cpuWaves[b][m].Length > windWaves.CpuMaxWaves)
						{
							Array.Resize(ref cpuWaves[b][m], cpuMaxWaves);
						}
					}
				}
			}
			shorelineCandidates = Enumerable.ToArray<WaterWave>((IEnumerable<WaterWave>)heap);
			Array.Sort(shorelineCandidates);
		}

		public static int GetMipIndex(int i)
		{
			if (i == 0)
			{
				return 0;
			}
			int num = (int)Mathf.Log(i, 2f) - 4;
			return (num >= 0) ? num : 0;
		}

		public Vector3[][,] GetSpectrumValues(int resolution)
		{
			int num = Mathf.RoundToInt(Mathf.Log(resolution, 2f)) - 4;
			Vector3[][,] array = spectrumValues[num];
			if (array == null)
			{
				array = (spectrumValues[num] = new Vector3[4][,]);
				Vector3[][,] array2 = null;
				int i;
				for (i = num + 1; i < spectrumValues.Length; i++)
				{
					array2 = spectrumValues[i];
					if (array2 != null)
					{
						break;
					}
				}
				int num2 = resolution / 2;
				int num3 = (1 << i + 4) - resolution;
				for (int j = 0; j < 4; j++)
				{
					Vector3[,] array3 = (array[j] = new Vector3[resolution, resolution]);
					Vector3[,] array4 = array2[j];
					for (int k = 0; k < num2; k++)
					{
						for (int l = 0; l < num2; l++)
						{
							array3[k, l] = array4[k, l];
						}
						for (int m = num2; m < resolution; m++)
						{
							array3[k, m] = array4[k, num3 + m];
						}
					}
					for (int n = num2; n < resolution; n++)
					{
						for (int num4 = 0; num4 < num2; num4++)
						{
							array3[n, num4] = array4[num3 + n, num4];
						}
						for (int num5 = num2; num5 < resolution; num5++)
						{
							array3[n, num5] = array4[num3 + n, num3 + num5];
						}
					}
				}
			}
			return array;
		}

		public void SetCpuWavesDirty()
		{
			cpuWavesDirty = true;
		}

		public void UpdateSpectralValues(Vector2 windDirection, float directionality)
		{
			ValidateSpectrumData();
			if (!cpuWavesDirty)
			{
				return;
			}
			lock (this)
			{
				if (cpuWaves == null || !cpuWavesDirty)
				{
					return;
				}
				lock (cpuWaves)
				{
					cpuWavesDirty = false;
					float directionalityInv = 1f - directionality;
					float horizontalDisplacementScale = water.HorizontalDisplacementScale;
					int finalResolution = windWaves.FinalResolution;
					bool mostlySorted = Vector2.Dot(lastWindDirection, windDirection) >= 0.97f;
					for (int i = 0; i < 4; i++)
					{
						WaterWave[][] array = cpuWaves[i];
						for (int j = 0; j <= displayResolutionIndex; j++)
						{
							WaterWave[] array2 = array[j];
							int num = array2.Length;
							for (int k = 0; k < num; k++)
							{
								array2[k].UpdateSpectralValues(spectrumValues[displayResolutionIndex], windDirection, directionalityInv, finalResolution, horizontalDisplacementScale);
							}
							SortCpuWaves(array2, mostlySorted);
						}
					}
					for (int l = 0; l < shorelineCandidates.Length; l++)
					{
						shorelineCandidates[l].UpdateSpectralValues(spectrumValues[displayResolutionIndex], windDirection, directionalityInv, finalResolution, horizontalDisplacementScale);
					}
					lastWindDirection = windDirection;
				}
			}
		}

		public void SortCpuWaves(WaterWave[] windWaves, bool mostlySorted)
		{
			if (!mostlySorted)
			{
				Array.Sort(windWaves, (WaterWave a, WaterWave b) => (a.cpuPriority > b.cpuPriority) ? (-1) : ((a.cpuPriority != b.cpuPriority) ? 1 : 0));
				return;
			}
			int num = windWaves.Length;
			int num2 = 0;
			for (int i = 1; i < num; i++)
			{
				if (windWaves[num2].cpuPriority < windWaves[i].cpuPriority)
				{
					WaterWave waterWave = windWaves[num2];
					windWaves[num2] = windWaves[i];
					windWaves[i] = waterWave;
					if (i != 1)
					{
						i -= 2;
					}
				}
				num2 = i;
			}
		}

		public void Dispose(bool onlyTexture)
		{
			if (texture != null)
			{
				texture.Destroy();
				texture = null;
			}
			if (!onlyTexture)
			{
				lock (this)
				{
					spectrumValues = null;
					cpuWaves = null;
					cpuWavesDirty = true;
				}
			}
		}

		private void ResetSpectrum(Vector3[,] values)
		{
			int length = values.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length; j++)
				{
					values[i, j] = new Vector3(0f, 0f, 0f);
				}
			}
		}

		private void CreateSpectrumTexture()
		{
			ValidateSpectrumData();
			int finalResolution = windWaves.FinalResolution;
			int num = finalResolution >> 1;
			texture = new Texture2D(finalResolution << 1, finalResolution << 1, TextureFormat.RGBAFloat, false, true);
			texture.hideFlags = HideFlags.DontSave;
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Repeat;
			for (int i = 0; i < 4; i++)
			{
				Vector3[,] array = spectrumValues[displayResolutionIndex][i];
				int num2 = ((((uint)i & (true ? 1u : 0u)) != 0) ? finalResolution : 0);
				int num3 = ((((uint)i & 2u) != 0) ? finalResolution : 0);
				for (int j = 0; j < finalResolution; j++)
				{
					int num4 = (j + num) % finalResolution;
					for (int k = 0; k < finalResolution; k++)
					{
						int num5 = (k + num) % finalResolution;
						Vector3 vector = array[num4, num5];
						texture.SetPixel(num2 + num4, num3 + num5, new Color(vector.x, vector.y, vector.z, 1f));
					}
				}
			}
			texture.Apply(false, true);
		}
	}
}
