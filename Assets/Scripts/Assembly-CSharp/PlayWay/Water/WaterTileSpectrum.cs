using UnityEngine;

namespace PlayWay.Water
{
	public class WaterTileSpectrum
	{
		public Vector2[] directionalSpectrum;

		public Vector2[][] displacements;

		public Vector4[][] forceAndHeight;

		public float[] resultsTiming;

		public int recentResultIndex;

		public float cachedTime = float.NegativeInfinity;

		public float cachedTimeProp;

		public Vector2[] cachedDisplacementsA;

		public Vector2[] cachedDisplacementsB;

		public Vector4[] cachedForceAndHeightA;

		public Vector4[] cachedForceAndHeightB;

		public bool resolveByFFT;

		public int directionalSpectrumDirty;

		public int tileIndex;

		private int resolutionFFT;

		private int mipIndexFFT;

		public WindWaves windWaves;

		public Water water;

		public bool IsResolvedByFFT
		{
			get
			{
				return resolveByFFT;
			}
		}

		public int ResolutionFFT
		{
			get
			{
				return resolutionFFT;
			}
		}

		public int MipIndexFFT
		{
			get
			{
				return mipIndexFFT;
			}
		}

		public WaterTileSpectrum(WindWaves windWaves, int index)
		{
			this.windWaves = windWaves;
			water = windWaves.GetComponent<Water>();
			tileIndex = index;
		}

		public bool SetResolveMode(bool resolveByFFT, int resolution)
		{
			if (this.resolveByFFT != resolveByFFT || (this.resolveByFFT && resolutionFFT != resolution))
			{
				if (resolveByFFT)
				{
					lock (this)
					{
						resolutionFFT = resolution;
						mipIndexFFT = WaterWavesSpectrumData.GetMipIndex(resolution);
						int num = resolution * resolution;
						directionalSpectrum = new Vector2[num];
						displacements = new Vector2[4][];
						forceAndHeight = new Vector4[4][];
						resultsTiming = new float[4];
						directionalSpectrumDirty = 2;
						cachedTime = float.NegativeInfinity;
						for (int i = 0; i < 4; i++)
						{
							displacements[i] = new Vector2[num];
							forceAndHeight[i] = new Vector4[num];
						}
						if (!this.resolveByFFT)
						{
							WaterAsynchronousTasks.Instance.AddFFTComputations(this);
							this.resolveByFFT = true;
						}
					}
				}
				else
				{
					WaterAsynchronousTasks.Instance.RemoveFFTComputations(this);
					this.resolveByFFT = false;
				}
				return true;
			}
			return false;
		}

		public void GetResults(float time, out Vector2[] da, out Vector2[] db, out Vector4[] fa, out Vector4[] fb, out float p)
		{
			if (time == cachedTime)
			{
				da = cachedDisplacementsA;
				db = cachedDisplacementsB;
				fa = cachedForceAndHeightA;
				fb = cachedForceAndHeightB;
				p = cachedTimeProp;
				return;
			}
			int num = recentResultIndex;
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (resultsTiming[num2] <= time)
				{
					int num3 = num2 + 1;
					da = displacements[num2];
					db = displacements[num3];
					fa = forceAndHeight[num2];
					fb = forceAndHeight[num3];
					float num4 = resultsTiming[num3] - resultsTiming[num2];
					if (num4 != 0f)
					{
						p = (time - resultsTiming[num2]) / num4;
					}
					else
					{
						p = 0f;
					}
					if (time > cachedTime)
					{
						cachedDisplacementsA = da;
						cachedDisplacementsB = db;
						cachedForceAndHeightA = fa;
						cachedForceAndHeightB = fb;
						cachedTimeProp = p;
						cachedTime = time;
					}
					return;
				}
			}
			for (int num5 = resultsTiming.Length - 1; num5 > num; num5--)
			{
				if (resultsTiming[num5] <= time)
				{
					int num6 = ((num5 != displacements.Length - 1) ? (num5 + 1) : 0);
					da = displacements[num5];
					db = displacements[num6];
					fa = forceAndHeight[num5];
					fb = forceAndHeight[num6];
					float num7 = resultsTiming[num6] - resultsTiming[num5];
					if (num7 != 0f)
					{
						p = (time - resultsTiming[num5]) / num7;
					}
					else
					{
						p = 0f;
					}
					return;
				}
			}
			da = displacements[num];
			db = displacements[num];
			fa = forceAndHeight[num];
			fb = forceAndHeight[num];
			p = 0f;
		}
	}
}
