using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class SpectrumResolverCPU
	{
		private class FoundWave
		{
			public WaterWavesSpectrumData spectrum;

			public WaterWave wave;

			public float importance;

			public FoundWave(WaterWavesSpectrumData spectrum, WaterWave wave)
			{
				this.spectrum = spectrum;
				this.wave = wave;
				importance = wave.cpuPriority * spectrum.Weight;
			}

			public GerstnerWave ToGerstner(Vector2[] scaleOffsets)
			{
				float w = wave.w;
				float num = (scaleOffsets[wave.scaleIndex].x * wave.nkx + scaleOffsets[wave.scaleIndex].y * wave.nky) * wave.k;
				return new GerstnerWave(new Vector2(wave.nkx, wave.nky), wave.amplitude * spectrum.Weight, num + wave.offset, wave.k, w);
			}
		}

		private Water water;

		private WindWaves windWaves;

		protected Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> spectraDataCache;

		private List<WaterWavesSpectrumData> spectraDataList;

		private List<CpuFFT> workers;

		private WaterTileSpectrum[] tileSpectra;

		private Vector2 surfaceOffset;

		private Vector2 windDirection;

		private int numTiles;

		private float lastFrameTime;

		private float uniformWaterScale;

		private WaterWave[] filteredCpuWaves;

		private int filteredCpuWavesCount;

		private int cachedSeed;

		private bool cpuWavesDirty;

		private static int[] fftComputationCosts;

		private float totalAmplitude;

		private float maxVerticalDisplacement;

		private float maxHorizontalDisplacement;

		public float TotalAmplitude
		{
			get
			{
				return totalAmplitude;
			}
		}

		public float MaxVerticalDisplacement
		{
			get
			{
				return maxVerticalDisplacement;
			}
		}

		public float MaxHorizontalDisplacement
		{
			get
			{
				return maxHorizontalDisplacement;
			}
		}

		public int AvgCpuWaves
		{
			get
			{
				int num = 0;
				foreach (WaterWavesSpectrumData value in spectraDataCache.Values)
				{
					num += value.CpuWavesCount;
				}
				return Mathf.RoundToInt((float)num / (float)spectraDataCache.Count);
			}
		}

		public Vector2 WindDirection
		{
			get
			{
				return windDirection;
			}
		}

		public float LastFrameTime
		{
			get
			{
				return lastFrameTime;
			}
		}

		public SpectrumResolverCPU(WindWaves windWaves, int numScales)
		{
			water = windWaves.GetComponent<Water>();
			this.windWaves = windWaves;
			spectraDataCache = new Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>();
			spectraDataList = new List<WaterWavesSpectrumData>();
			filteredCpuWaves = new WaterWave[0];
			numTiles = numScales;
			cachedSeed = windWaves.GetComponent<Water>().Seed;
			if (fftComputationCosts == null)
			{
				PrecomputeFFTCosts();
			}
			CreateSpectraLevels();
		}

		public WaterTileSpectrum GetTile(int index)
		{
			return tileSpectra[index];
		}

		internal void Update()
		{
			surfaceOffset = water.SurfaceOffset;
			lastFrameTime = water.Time;
			uniformWaterScale = water.UniformWaterScale;
			UpdateCachedSeed();
			int computedSamplesCount = water.ComputedSamplesCount;
			bool allowCpuFFT = WaterProjectSettings.Instance.AllowCpuFFT;
			for (int i = 0; i < numTiles; i++)
			{
				int num = 16;
				int num2 = 0;
				while (true)
				{
					int num3 = 0;
					for (int j = 0; j < spectraDataList.Count; j++)
					{
						WaterWavesSpectrumData waterWavesSpectrumData = spectraDataList[j];
						waterWavesSpectrumData.ValidateSpectrumData();
						WaterWave[] array = waterWavesSpectrumData.GetCpuWaves(i)[num2];
						if (array != null)
						{
							num3 += (int)((float)array.Length * waterWavesSpectrumData.Weight);
						}
					}
					if (num3 * computedSamplesCount < fftComputationCosts[num2] + computedSamplesCount)
					{
						num >>= 1;
						break;
					}
					if (num >= windWaves.FinalResolution)
					{
						break;
					}
					num <<= 1;
					num2++;
				}
				num <<= windWaves.CpuFFTPrecisionBoost;
				if (num > windWaves.FinalResolution)
				{
					num = windWaves.FinalResolution;
				}
				WaterTileSpectrum waterTileSpectrum = tileSpectra[i];
				if (waterTileSpectrum.SetResolveMode(num >= 16 && allowCpuFFT, num))
				{
					cpuWavesDirty = true;
				}
			}
		}

		internal void SetWindDirection(Vector2 windDirection)
		{
			this.windDirection = windDirection;
			InvalidateDirectionalSpectrum();
		}

		public void DisposeCachedSpectra()
		{
			Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData>.Enumerator enumerator = spectraDataCache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Dispose(false);
			}
		}

		public WaterWavesSpectrumData GetSpectrumData(WaterWavesSpectrum spectrum)
		{
			WaterWavesSpectrumData value;
			if (!spectraDataCache.TryGetValue(spectrum, out value))
			{
				lock (spectraDataCache)
				{
					value = (spectraDataCache[spectrum] = new WaterWavesSpectrumData(water, spectrum));
				}
				value.ValidateSpectrumData();
				cpuWavesDirty = true;
				lock (spectraDataList)
				{
					spectraDataList.Add(value);
					return value;
				}
			}
			return value;
		}

		public void CacheSpectrum(WaterWavesSpectrum spectrum)
		{
			GetSpectrumData(spectrum);
		}

		public Dictionary<WaterWavesSpectrum, WaterWavesSpectrumData> GetCachedSpectraDirect()
		{
			return spectraDataCache;
		}

		private void InterpolationParams(float x, float z, int scaleIndex, float tileSize, out float fx, out float invFx, out float fy, out float invFy, out int index0, out int index1, out int index2, out int index3)
		{
			int resolutionFFT = tileSpectra[scaleIndex].ResolutionFFT;
			int finalResolution = windWaves.FinalResolution;
			x += 0.5f / (float)finalResolution * tileSize;
			z += 0.5f / (float)finalResolution * tileSize;
			float num = (float)resolutionFFT / tileSize;
			fx = x * num;
			fy = z * num;
			int num2 = (int)fx;
			if ((float)num2 > fx)
			{
				num2--;
			}
			int num3 = (int)fy;
			if ((float)num3 > fy)
			{
				num3--;
			}
			fx -= num2;
			fy -= num3;
			num2 %= resolutionFFT;
			num3 %= resolutionFFT;
			if (num2 < 0)
			{
				num2 += resolutionFFT;
			}
			if (num3 < 0)
			{
				num3 += resolutionFFT;
			}
			num2 = resolutionFFT - num2 - 1;
			num3 = resolutionFFT - num3 - 1;
			int num4 = num2 + 1;
			int num5 = num3 + 1;
			if (num4 == resolutionFFT)
			{
				num4 = 0;
			}
			if (num5 == resolutionFFT)
			{
				num5 = 0;
			}
			num3 *= resolutionFFT;
			num5 *= resolutionFFT;
			index0 = num3 + num2;
			index1 = num3 + num4;
			index2 = num5 + num2;
			index3 = num5 + num4;
			invFx = 1f - fx;
			invFy = 1f - fy;
		}

		public Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector3 result = default(Vector3);
			x = 0f - (x + surfaceOffset.x);
			z = 0f - (z + surfaceOffset.y);
			if (spectrumStart == 0f)
			{
				for (int num = numTiles - 1; num >= 0; num--)
				{
					if (tileSpectra[num].resolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int index;
						int index2;
						int index3;
						int index4;
						Vector2[] da;
						Vector2[] db;
						Vector4[] fa;
						Vector4[] fb;
						float p;
						lock (tileSpectra[num])
						{
							InterpolationParams(x, z, num, windWaves.TileSizes[num], out fx, out invFx, out fy, out invFy, out index, out index2, out index3, out index4);
							tileSpectra[num].GetResults(time, out da, out db, out fa, out fb, out p);
						}
						Vector2 vector = FastMath.Interpolate(ref da[index], ref da[index2], ref da[index3], ref da[index4], ref db[index], ref db[index2], ref db[index3], ref db[index4], fx, invFx, fy, invFy, p);
						result.x -= vector.x;
						result.z -= vector.y;
						result.y += FastMath.Interpolate(fa[index].w, fa[index2].w, fa[index3].w, fa[index4].w, fb[index].w, fb[index2].w, fb[index3].w, fb[index4].w, fx, invFx, fy, invFy, p);
					}
				}
			}
			if (filteredCpuWavesCount != 0)
			{
				SampleWavesDirectly(spectrumStart, spectrumEnd, delegate(WaterWave[] cpuWaves, int startIndex, int endIndex)
				{
					Vector3 vector2 = default(Vector3);
					for (int i = startIndex; i < endIndex; i++)
					{
						vector2 += cpuWaves[i].GetDisplacementAt(x, z, time);
					}
					result += vector2;
				});
			}
			float num2 = (0f - water.HorizontalDisplacementScale) * uniformWaterScale;
			result.x *= num2;
			result.y *= uniformWaterScale;
			result.z *= num2;
			return result;
		}

		public Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector2 result = default(Vector2);
			x = 0f - (x + surfaceOffset.x);
			z = 0f - (z + surfaceOffset.y);
			if (spectrumStart == 0f)
			{
				for (int num = numTiles - 1; num >= 0; num--)
				{
					if (tileSpectra[num].resolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int index;
						int index2;
						int index3;
						int index4;
						Vector2[] da;
						Vector2[] db;
						float p;
						lock (tileSpectra[num])
						{
							InterpolationParams(x, z, num, windWaves.TileSizes[num], out fx, out invFx, out fy, out invFy, out index, out index2, out index3, out index4);
							Vector4[] fa;
							Vector4[] fb;
							tileSpectra[num].GetResults(time, out da, out db, out fa, out fb, out p);
						}
						result -= FastMath.Interpolate(ref da[index], ref da[index2], ref da[index3], ref da[index4], ref db[index], ref db[index2], ref db[index3], ref db[index4], fx, invFx, fy, invFy, p);
					}
				}
			}
			if (filteredCpuWavesCount != 0)
			{
				SampleWavesDirectly(spectrumStart, spectrumEnd, delegate(WaterWave[] cpuWaves, int startIndex, int endIndex)
				{
					Vector2 vector = default(Vector2);
					for (int i = startIndex; i < endIndex; i++)
					{
						vector += cpuWaves[i].GetRawHorizontalDisplacementAt(x, z, time);
					}
					result += vector;
				});
			}
			float num2 = (0f - water.HorizontalDisplacementScale) * uniformWaterScale;
			result.x *= num2;
			result.y *= num2;
			return result;
		}

		public Vector4 GetForceAndHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			Vector4 result = default(Vector4);
			x = 0f - (x + surfaceOffset.x);
			z = 0f - (z + surfaceOffset.y);
			if (spectrumStart == 0f)
			{
				for (int num = numTiles - 1; num >= 0; num--)
				{
					if (tileSpectra[num].resolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int index;
						int index2;
						int index3;
						int index4;
						Vector4[] fa;
						Vector4[] fb;
						float p;
						lock (tileSpectra[num])
						{
							InterpolationParams(x, z, num, windWaves.TileSizes[num], out fx, out invFx, out fy, out invFy, out index, out index2, out index3, out index4);
							Vector2[] da;
							Vector2[] db;
							tileSpectra[num].GetResults(time, out da, out db, out fa, out fb, out p);
						}
						result += FastMath.Interpolate(fa[index], fa[index2], fa[index3], fa[index4], fb[index], fb[index2], fb[index3], fb[index4], fx, invFx, fy, invFy, p);
					}
				}
			}
			if (filteredCpuWavesCount != 0)
			{
				SampleWavesDirectly(spectrumStart, spectrumEnd, delegate(WaterWave[] cpuWaves, int startIndex, int endIndex)
				{
					Vector4 result2 = default(Vector4);
					for (int i = startIndex; i < endIndex; i++)
					{
						cpuWaves[i].GetForceAndHeightAt(x, z, time, ref result2);
					}
					result += result2;
				});
			}
			float num2 = water.HorizontalDisplacementScale * uniformWaterScale;
			result.x *= num2;
			result.z *= num2;
			result.y *= 0.5f * uniformWaterScale;
			result.w *= uniformWaterScale;
			return result;
		}

		public float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time)
		{
			float result = 0f;
			x = 0f - (x + surfaceOffset.x);
			z = 0f - (z + surfaceOffset.y);
			if (spectrumStart == 0f)
			{
				for (int num = numTiles - 1; num >= 0; num--)
				{
					if (tileSpectra[num].resolveByFFT)
					{
						float fx;
						float invFx;
						float fy;
						float invFy;
						int index;
						int index2;
						int index3;
						int index4;
						Vector4[] fa;
						Vector4[] fb;
						float p;
						lock (tileSpectra[num])
						{
							InterpolationParams(x, z, num, windWaves.TileSizes[num], out fx, out invFx, out fy, out invFy, out index, out index2, out index3, out index4);
							Vector2[] da;
							Vector2[] db;
							tileSpectra[num].GetResults(time, out da, out db, out fa, out fb, out p);
						}
						result += FastMath.Interpolate(fa[index].w, fa[index2].w, fa[index3].w, fa[index4].w, fb[index].w, fb[index2].w, fb[index3].w, fb[index4].w, fx, invFx, fy, invFy, p);
					}
				}
			}
			if (filteredCpuWavesCount != 0)
			{
				SampleWavesDirectly(spectrumStart, spectrumEnd, delegate(WaterWave[] cpuWaves, int startIndex, int endIndex)
				{
					float num2 = 0f;
					for (int i = startIndex; i < endIndex; i++)
					{
						num2 += cpuWaves[i].GetHeightAt(x, z, time);
					}
					result += num2;
				});
			}
			return result * uniformWaterScale;
		}

		private void SampleWavesDirectly(float spectrumStart, float spectrumEnd, Action<WaterWave[], int, int> func)
		{
			lock (this)
			{
				WaterWave[] array = GetFilteredCpuWaves();
				int num = (int)(spectrumStart * (float)filteredCpuWavesCount);
				int num2 = (int)(spectrumEnd * (float)filteredCpuWavesCount);
				if (num != num2)
				{
					func.Invoke(array, num, num2);
				}
			}
		}

		public WaterWave[] GetFilteredCpuWaves()
		{
			if (cpuWavesDirty)
			{
				int num = 0;
				foreach (WaterWavesSpectrumData spectraData in spectraDataList)
				{
					spectraData.UpdateSpectralValues(windDirection, water.Directionality);
					float weight = spectraData.Weight;
					for (int i = 0; i < numTiles; i++)
					{
						WaterWave[][] cpuWaves = spectraData.GetCpuWaves(i);
						int num2 = (tileSpectra[i].IsResolvedByFFT ? (tileSpectra[i].MipIndexFFT + 1) : 0);
						for (int j = num2; j < cpuWaves.Length; j++)
						{
							WaterWave[] array = cpuWaves[j];
							if (filteredCpuWaves.Length < num + array.Length)
							{
								Array.Resize(ref filteredCpuWaves, num + array.Length + 120);
							}
							for (int k = 0; k < array.Length; k++)
							{
								filteredCpuWaves[num] = array[k];
								filteredCpuWaves[num++].amplitude *= weight;
							}
						}
					}
				}
				filteredCpuWavesCount = num;
				cpuWavesDirty = false;
			}
			return filteredCpuWaves;
		}

		public GerstnerWave[] SelectShorelineWaves(int count, float angle, float coincidenceRange)
		{
			List<FoundWave> list = new List<FoundWave>();
			foreach (WaterWavesSpectrumData spectraData in spectraDataList)
			{
				if (spectraData.Weight < 0.001f)
				{
					continue;
				}
				spectraData.UpdateSpectralValues(windDirection, water.Directionality);
				lock (this)
				{
					WaterWave[] shorelineCandidates = spectraData.ShorelineCandidates;
					int num = count;
					for (int i = 0; i < shorelineCandidates.Length; i++)
					{
						if (num == 0)
						{
							break;
						}
						float current2 = Mathf.Atan2(shorelineCandidates[i].nkx, shorelineCandidates[i].nky) * 57.29578f;
						if (Mathf.Abs(Mathf.DeltaAngle(current2, angle)) < coincidenceRange && shorelineCandidates[i].amplitude > 0.025f)
						{
							list.Add(new FoundWave(spectraData, shorelineCandidates[i]));
							num--;
						}
					}
				}
			}
			list.Sort((FoundWave a, FoundWave b) => b.importance.CompareTo(a.importance));
			Vector2[] array = new Vector2[4];
			for (int j = 0; j < 4; j++)
			{
				float num2 = windWaves.TileSizes[j];
				array[j].x = num2 + 0.5f / (float)windWaves.FinalResolution * num2;
				array[j].y = 0f - num2 + 0.5f / (float)windWaves.FinalResolution * num2;
			}
			int num3 = Mathf.Min(list.Count, count);
			GerstnerWave[] array2 = new GerstnerWave[num3];
			for (int k = 0; k < num3; k++)
			{
				array2[k] = list[list.Count - k - 1].ToGerstner(array);
			}
			return array2;
		}

		public GerstnerWave[] FindMostMeaningfulWaves(int count, bool mask)
		{
			List<FoundWave> list = new List<FoundWave>();
			foreach (WaterWavesSpectrumData spectraData in spectraDataList)
			{
				if (spectraData.Weight < 0.001f)
				{
					continue;
				}
				spectraData.UpdateSpectralValues(windDirection, water.Directionality);
				lock (this)
				{
					WaterWave[] array = GetFilteredCpuWaves();
					int num = Mathf.Min(array.Length, count);
					for (int i = 0; i < num; i++)
					{
						list.Add(new FoundWave(spectraData, array[i]));
					}
				}
			}
			list.Sort((FoundWave a, FoundWave b) => b.importance.CompareTo(a.importance));
			Vector2[] array2 = new Vector2[4];
			for (int j = 0; j < 4; j++)
			{
				float num2 = windWaves.TileSizes[j];
				array2[j].x = num2 + 0.5f / (float)windWaves.FinalResolution * num2;
				array2[j].y = 0f - num2 + 0.5f / (float)windWaves.FinalResolution * num2;
			}
			GerstnerWave[] array3 = new GerstnerWave[count];
			for (int k = 0; k < count; k++)
			{
				array3[k] = list[k].ToGerstner(array2);
			}
			return array3;
		}

		public Gerstner4[] FindGerstners(int count, bool mask)
		{
			List<FoundWave> list = new List<FoundWave>();
			foreach (WaterWavesSpectrumData value in spectraDataCache.Values)
			{
				if (value.Weight < 0.001f)
				{
					continue;
				}
				value.UpdateSpectralValues(windDirection, water.Directionality);
				lock (this)
				{
					WaterWave[] array = GetFilteredCpuWaves();
					int num = Mathf.Min(array.Length, count);
					for (int i = 0; i < num; i++)
					{
						list.Add(new FoundWave(value, array[i]));
					}
				}
			}
			list.Sort((FoundWave a, FoundWave b) => b.importance.CompareTo(a.importance));
			int num2 = 0;
			int num3 = count >> 2;
			Gerstner4[] array2 = new Gerstner4[num3];
			Vector2[] array3 = new Vector2[4];
			for (int j = 0; j < 4; j++)
			{
				float num4 = windWaves.TileSizes[j];
				array3[j].x = num4 + 0.5f / (float)windWaves.FinalResolution * num4;
				array3[j].y = 0f - num4 + 0.5f / (float)windWaves.FinalResolution * num4;
			}
			for (int k = 0; k < num3; k++)
			{
				GerstnerWave wave = ((num2 >= list.Count) ? new GerstnerWave() : list[num2++].ToGerstner(array3));
				GerstnerWave wave2 = ((num2 >= list.Count) ? new GerstnerWave() : list[num2++].ToGerstner(array3));
				GerstnerWave wave3 = ((num2 >= list.Count) ? new GerstnerWave() : list[num2++].ToGerstner(array3));
				GerstnerWave wave4 = ((num2 >= list.Count) ? new GerstnerWave() : list[num2++].ToGerstner(array3));
				array2[k] = new Gerstner4(wave, wave2, wave3, wave4);
			}
			return array2;
		}

		private void UpdateCachedSeed()
		{
			if (cachedSeed != water.Seed)
			{
				cachedSeed = water.Seed;
				DisposeCachedSpectra();
				OnProfilesChanged();
			}
		}

		internal virtual void OnProfilesChanged()
		{
			Water.WeightedProfile[] profiles = water.Profiles;
			foreach (WaterWavesSpectrumData value2 in spectraDataCache.Values)
			{
				value2.Weight = 0f;
			}
			Water.WeightedProfile[] array = profiles;
			for (int i = 0; i < array.Length; i++)
			{
				Water.WeightedProfile weightedProfile = array[i];
				if (!(weightedProfile.weight <= 0.0001f))
				{
					WaterWavesSpectrum spectrum = weightedProfile.profile.Spectrum;
					WaterWavesSpectrumData value;
					if (!spectraDataCache.TryGetValue(spectrum, out value))
					{
						value = GetSpectrumData(spectrum);
					}
					value.Weight = weightedProfile.weight;
				}
			}
			totalAmplitude = 0f;
			foreach (WaterWavesSpectrumData value3 in spectraDataCache.Values)
			{
				totalAmplitude += value3.TotalAmplitude * value3.Weight;
			}
			maxVerticalDisplacement = totalAmplitude * 0.06f;
			maxHorizontalDisplacement = maxVerticalDisplacement * water.HorizontalDisplacementScale;
			InvalidateDirectionalSpectrum();
		}

		private void PrecomputeFFTCosts()
		{
			fftComputationCosts = new int[10];
			int num = 16;
			for (int i = 0; i < fftComputationCosts.Length; i++)
			{
				int num2 = (int)(Mathf.Log(num) / Mathf.Log(2f));
				fftComputationCosts[i] = num * num * num2;
				num <<= 1;
			}
			for (int num3 = fftComputationCosts.Length - 1; num3 >= 1; num3--)
			{
				fftComputationCosts[num3] -= fftComputationCosts[num3 - 1];
			}
		}

		private void CreateSpectraLevels()
		{
			tileSpectra = new WaterTileSpectrum[numTiles];
			for (int i = 0; i < numTiles; i++)
			{
				tileSpectra[i] = new WaterTileSpectrum(windWaves, i);
			}
		}

		protected virtual void InvalidateDirectionalSpectrum()
		{
			cpuWavesDirty = true;
			foreach (WaterWavesSpectrumData spectraData in spectraDataList)
			{
				spectraData.SetCpuWavesDirty();
			}
			for (int i = 0; i < numTiles; i++)
			{
				tileSpectra[i].directionalSpectrumDirty = 2;
			}
		}

		internal virtual void OnMapsFormatChanged(bool resolution)
		{
			if (spectraDataCache != null)
			{
				foreach (WaterWavesSpectrumData value in spectraDataCache.Values)
				{
					value.Dispose(!resolution);
				}
			}
			InvalidateDirectionalSpectrum();
		}

		internal virtual void OnDestroy()
		{
			OnMapsFormatChanged(true);
			spectraDataCache = null;
			lock (spectraDataList)
			{
				spectraDataList.Clear();
			}
		}
	}
}
