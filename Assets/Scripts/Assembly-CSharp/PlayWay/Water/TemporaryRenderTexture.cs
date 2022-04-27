using System;
using UnityEngine;

namespace PlayWay.Water
{
	public struct TemporaryRenderTexture : IDisposable
	{
		private RenderTexture renderTexture;

		private RenderTexturesCache renderTexturesCache;

		public RenderTexture Texture
		{
			get
			{
				return renderTexture;
			}
		}

		internal TemporaryRenderTexture(RenderTexturesCache renderTexturesCache)
		{
			this.renderTexturesCache = renderTexturesCache;
			renderTexture = renderTexturesCache.GetTemporaryDirect();
		}

		public void Dispose()
		{
			if (renderTexture != null)
			{
				renderTexturesCache.ReleaseTemporaryDirect(renderTexture);
				renderTexture = null;
			}
		}

		public static implicit operator RenderTexture(TemporaryRenderTexture that)
		{
			return that.Texture;
		}
	}
}
