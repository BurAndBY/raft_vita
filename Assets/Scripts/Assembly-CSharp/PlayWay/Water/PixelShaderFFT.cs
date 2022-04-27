using UnityEngine;

namespace PlayWay.Water
{
	public sealed class PixelShaderFFT : GpuFFT
	{
		private Material material;

		private int butterflyTexProperty;

		private int butterflyPassProperty;

		private TemporaryRenderTexture rt1;

		private TemporaryRenderTexture rt2;

		public PixelShaderFFT(Shader fftShader, int resolution, bool highPrecision, bool twoChannels)
			: base(resolution, highPrecision, twoChannels, false)
		{
			material = new Material(fftShader);
			material.hideFlags = HideFlags.DontSave;
			butterflyTexProperty = Shader.PropertyToID("_ButterflyTex");
			butterflyPassProperty = Shader.PropertyToID("_ButterflyPass");
		}

		public override void Dispose()
		{
			base.Dispose();
			if (material == null)
			{
				Object.Destroy(material);
			}
		}

		public override void SetupMaterials()
		{
			material.SetTexture(butterflyTexProperty, base.Butterfly);
		}

		public override void ComputeFFT(Texture tex, RenderTexture target)
		{
			using (rt1 = renderTexturesSet.GetTemporary())
			{
				using (rt2 = renderTexturesSet.GetTemporary())
				{
					ComputeFFT(tex, null, twoChannels ? 2 : 0);
					ComputeFFT((RenderTexture)rt1, target, (!twoChannels) ? 1 : 3);
				}
			}
		}

		private void ComputeFFT(Texture tex, RenderTexture target, int passIndex)
		{
			material.SetFloat(butterflyPassProperty, 0.5f / (float)numButterfliesPow2);
			Graphics.Blit(tex, rt2, material, passIndex);
			SwapRT();
			for (int i = 1; i < numButterflies; i++)
			{
				if (target != null && i == numButterflies - 1)
				{
					material.SetFloat(butterflyPassProperty, ((float)i + 0.5f) / (float)numButterfliesPow2);
					//Graphics.Blit((RenderTexture)rt1, target, material, (passIndex != 1) ? 5 : 4);
				}
				else
				{
					material.SetFloat(butterflyPassProperty, ((float)i + 0.5f) / (float)numButterfliesPow2);
					//Graphics.Blit((RenderTexture)rt1, rt2, material, passIndex);
				}
				SwapRT();
			}
		}

		private void SwapRT()
		{
			TemporaryRenderTexture temporaryRenderTexture = rt1;
			rt1 = rt2;
			rt2 = temporaryRenderTexture;
		}
	}
}
