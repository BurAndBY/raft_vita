using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace PlayWay.Water
{
	public class WaterAsynchronousTasks : MonoBehaviour
	{
		private static WaterAsynchronousTasks instance;

		private bool run;

		private List<WaterTileSpectrum> fftSpectra = new List<WaterTileSpectrum>();

		private int fftSpectrumIndex;

		private float fftTimeStep = 0.2f;

		private List<WaterSample> computations = new List<WaterSample>();

		private int computationIndex;

		private Exception threadException;

		public static WaterAsynchronousTasks Instance
		{
			get
			{
				if (instance == null)
				{
					instance = UnityEngine.Object.FindObjectOfType<WaterAsynchronousTasks>();
					if (instance == null)
					{
						GameObject gameObject = new GameObject("PlayWay Water Spectrum Sampler");
						gameObject.hideFlags = HideFlags.HideInHierarchy;
						instance = gameObject.AddComponent<WaterAsynchronousTasks>();
					}
				}
				return instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return instance != null;
			}
		}

		private void Awake()
		{
			run = true;
			for (int i = 0; i < WaterProjectSettings.Instance.PhysicsThreads; i++)
			{
				Thread thread = new Thread(RunSamplingTask);
				thread.Priority = WaterProjectSettings.Instance.PhysicsThreadsPriority;
				thread.Start();
			}
			Thread thread2 = new Thread(RunFFTTask);
			thread2.Priority = WaterProjectSettings.Instance.PhysicsThreadsPriority;
			thread2.Start();
		}

		public void AddWaterSampleComputations(WaterSample computation)
		{
			lock (computations)
			{
				computations.Add(computation);
			}
		}

		public void RemoveWaterSampleComputations(WaterSample computation)
		{
			lock (computations)
			{
				int num = computations.IndexOf(computation);
				if (num != -1)
				{
					if (num < computationIndex)
					{
						computationIndex--;
					}
					computations.RemoveAt(num);
				}
			}
		}

		public void AddFFTComputations(WaterTileSpectrum scale)
		{
			lock (fftSpectra)
			{
				fftSpectra.Add(scale);
			}
		}

		public void RemoveFFTComputations(WaterTileSpectrum scale)
		{
			lock (fftSpectra)
			{
				int num = fftSpectra.IndexOf(scale);
				if (num != -1)
				{
					if (num < fftSpectrumIndex)
					{
						fftSpectrumIndex--;
					}
					fftSpectra.RemoveAt(num);
				}
			}
		}

		private void OnDisable()
		{
			run = false;
			if (threadException != null)
			{
				UnityEngine.Debug.LogException(threadException);
			}
		}

		private void RunSamplingTask()
		{
			try
			{
				while (run)
				{
					WaterSample waterSample = null;
					lock (computations)
					{
						if (computations.Count != 0)
						{
							if (computationIndex >= computations.Count)
							{
								computationIndex = 0;
							}
							waterSample = computations[computationIndex++];
						}
					}
					if (waterSample == null)
					{
						Thread.Sleep(2);
						continue;
					}
					lock (waterSample)
					{
						waterSample.ComputationStep();
					}
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = (threadException = ex);
			}
		}

		private void RunFFTTask()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			try
			{
				CpuFFT cpuFFT = new CpuFFT();
				Stopwatch val = new Stopwatch();
				bool flag = false;
				while (run)
				{
					WaterTileSpectrum waterTileSpectrum = null;
					lock (fftSpectra)
					{
						if (fftSpectra.Count != 0)
						{
							if (fftSpectrumIndex >= fftSpectra.Count)
							{
								fftSpectrumIndex = 0;
							}
							if (fftSpectrumIndex == 0)
							{
								if ((float)val.ElapsedMilliseconds > fftTimeStep * 900f)
								{
									if (flag)
									{
										fftTimeStep += 0.05f;
									}
									else
									{
										flag = true;
									}
								}
								else
								{
									flag = false;
									if (fftTimeStep > 0.2f)
									{
										fftTimeStep -= 0.001f;
									}
								}
								val.Reset();
								val.Start();
							}
							waterTileSpectrum = fftSpectra[fftSpectrumIndex++];
						}
					}
					if (waterTileSpectrum == null)
					{
						val.Reset();
						Thread.Sleep(6);
						continue;
					}
					bool flag2 = false;
					SpectrumResolver spectrumResolver = waterTileSpectrum.windWaves.SpectrumResolver;
					if (spectrumResolver != null)
					{
						int recentResultIndex = waterTileSpectrum.recentResultIndex;
						int num = (recentResultIndex + 2) % waterTileSpectrum.resultsTiming.Length;
						int num2 = (recentResultIndex + 1) % waterTileSpectrum.resultsTiming.Length;
						float a = waterTileSpectrum.resultsTiming[recentResultIndex];
						float num3 = waterTileSpectrum.resultsTiming[num];
						float lastFrameTime = spectrumResolver.LastFrameTime;
						if (num3 <= lastFrameTime)
						{
							float num4 = Mathf.Max(a, lastFrameTime) + fftTimeStep;
							cpuFFT.Compute(waterTileSpectrum, num4, num2);
							waterTileSpectrum.resultsTiming[num2] = num4;
							waterTileSpectrum.recentResultIndex = num2;
							flag2 = true;
						}
						if (!flag2)
						{
							val.Reset();
							Thread.Sleep(3);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = (threadException = ex);
			}
		}
	}
}
