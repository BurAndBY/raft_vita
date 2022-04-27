using UnityEngine;

namespace PlayWay.Water
{
	public abstract class GpuFFT
	{
		private Texture2D butterfly;

		protected RenderTexturesCache renderTexturesSet;

		protected int resolution;

		protected int numButterflies;

		protected int numButterfliesPow2;

		protected bool twoChannels;

		private bool highPrecision;

		private bool usesUAV;

		public Texture2D Butterfly
		{
			get
			{
				return butterfly;
			}
		}

		public int Resolution
		{
			get
			{
				return resolution;
			}
		}

		public GpuFFT(int resolution, bool highPrecision, bool twoChannels, bool usesUAV)
		{
			this.resolution = resolution;
			this.highPrecision = highPrecision;
			numButterflies = (int)(Mathf.Log(resolution) / Mathf.Log(2f));
			numButterfliesPow2 = Mathf.NextPowerOfTwo(numButterflies);
			this.twoChannels = twoChannels;
			this.usesUAV = usesUAV;
			RetrieveRenderTexturesSet();
			CreateTextures();
		}

		public abstract void SetupMaterials();

		public abstract void ComputeFFT(Texture tex, RenderTexture target);

		public virtual void Dispose()
		{
			if (butterfly != null)
			{
				butterfly.Destroy();
				butterfly = null;
			}
		}

		private void CreateTextures()
		{
			CreateButterflyTexture();
		}

		private void RetrieveRenderTexturesSet()
		{
			RenderTextureFormat format = (twoChannels ? ((!highPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat) : ((!highPrecision) ? RenderTextureFormat.RGHalf : RenderTextureFormat.RGFloat));
			renderTexturesSet = RenderTexturesCache.GetCache(resolution << 1, resolution << 1, 0, format, true, usesUAV);
		}

		protected virtual void FillButterflyTexture(Texture2D butterfly, int[][] indices, Vector2[][] weights)
		{
			float num = resolution << 1;
			Color color = default(Color);
			for (int i = 0; i < numButterflies; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					int num2 = ((j != 0) ? resolution : 0);
					for (int k = 0; k < resolution; k++)
					{
						int num3 = numButterflies - i - 1;
						int num4 = k << 1;
						color.r = ((float)(indices[num3][num4] + num2) + 0.5f) / num;
						color.g = ((float)(indices[num3][num4 + 1] + num2) + 0.5f) / num;
						color.b = weights[i][k].x;
						color.a = weights[i][k].y;
						butterfly.SetPixel(num2 + k, i, color);
					}
				}
			}
		}

		private void CreateButterflyTexture()
		{
			butterfly = new Texture2D(resolution << 1, numButterfliesPow2, (!highPrecision) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat, false, true);
			butterfly.hideFlags = HideFlags.DontSave;
			butterfly.filterMode = FilterMode.Point;
			butterfly.wrapMode = TextureWrapMode.Clamp;
			int[][] indices;
			Vector2[][] weights;
			ButterflyFFTUtility.ComputeButterfly(resolution, numButterflies, out indices, out weights);
			FillButterflyTexture(butterfly, indices, weights);
			butterfly.Apply();
		}
	}
}
