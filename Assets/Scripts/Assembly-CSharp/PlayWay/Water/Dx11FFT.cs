using UnityEngine;

namespace PlayWay.Water
{
	public class Dx11FFT : GpuFFT
	{
		private ComputeShader shader;

		private int kernelIndex;

		public Dx11FFT(ComputeShader shader, int resolution, bool highPrecision, bool twoChannels)
			: base(resolution, highPrecision, twoChannels, true)
		{
			this.shader = shader;
			kernelIndex = numButterflies - 5 << 1;
			if (twoChannels)
			{
				kernelIndex += 10;
			}
		}

		public override void SetupMaterials()
		{
		}

		public override void ComputeFFT(Texture tex, RenderTexture target)
		{
			TemporaryRenderTexture temporary = renderTexturesSet.GetTemporary();
			if (!target.IsCreated())
			{
				target.enableRandomWrite = true;
				target.Create();
			}
			shader.SetTexture(kernelIndex, "_ButterflyTex", base.Butterfly);
			shader.SetTexture(kernelIndex, "_SourceTex", tex);
			shader.SetTexture(kernelIndex, "_TargetTex", (RenderTexture)temporary);
			shader.Dispatch(kernelIndex, 1, tex.height, 1);
			shader.SetTexture(kernelIndex + 1, "_ButterflyTex", base.Butterfly);
			shader.SetTexture(kernelIndex + 1, "_SourceTex", (RenderTexture)temporary);
			shader.SetTexture(kernelIndex + 1, "_TargetTex", target);
			shader.Dispatch(kernelIndex + 1, 1, tex.height, 1);
			temporary.Dispose();
		}

		protected override void FillButterflyTexture(Texture2D butterfly, int[][] indices, Vector2[][] weights)
		{
			Color color = default(Color);
			for (int i = 0; i < numButterflies; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					int num = ((j != 0) ? resolution : 0);
					for (int k = 0; k < resolution; k++)
					{
						int num2 = numButterflies - i - 1;
						int num3 = k << 1;
						color.r = indices[num2][num3] + num;
						color.g = indices[num2][num3 + 1] + num;
						color.b = weights[i][k].x;
						color.a = weights[i][k].y;
						butterfly.SetPixel(num + k, i, color);
					}
				}
			}
		}
	}
}
