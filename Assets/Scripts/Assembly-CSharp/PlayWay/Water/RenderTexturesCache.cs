using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class RenderTexturesCache
	{
		[ExecuteInEditMode]
		public class RenderTexturesUpdater : MonoBehaviour
		{
			private static RenderTexturesUpdater instance;

			public static void EnsureInstance()
			{
				if (instance == null)
				{
					GameObject gameObject = new GameObject("Water.RenderTexturesCache");
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					if (Application.isPlaying)
					{
						Object.DontDestroyOnLoad(gameObject);
					}
					instance = gameObject.AddComponent<RenderTexturesUpdater>();
				}
			}

			private void Update()
			{
				int frameCount = Time.frameCount;
				Dictionary<ulong, RenderTexturesCache>.Enumerator enumerator = cache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					enumerator.Current.Value.Update(frameCount);
				}
			}
		}

		private static Dictionary<ulong, RenderTexturesCache> cache = new Dictionary<ulong, RenderTexturesCache>(UInt64EqualityComparer.Default);

		private Queue<RenderTexture> renderTextures;

		private int lastFrameAllUsed;

		private ulong hash;

		private int width;

		private int height;

		private int depthBuffer;

		private RenderTextureFormat format;

		private bool linear;

		private bool uav;

		private bool mipMaps;

		public RenderTexturesCache(ulong hash, int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps)
		{
			this.hash = hash;
			this.width = width;
			this.height = height;
			this.depthBuffer = depthBuffer;
			this.format = format;
			this.linear = linear;
			this.uav = uav;
			this.mipMaps = mipMaps;
			renderTextures = new Queue<RenderTexture>();
		}

		public static RenderTexturesCache GetCache(int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps = false)
		{
			RenderTexturesUpdater.EnsureInstance();
			ulong num = 0uL;
			num |= (uint)width;
			num |= (uint)(height << 16);
			num |= (ulong)((long)depthBuffer << 29);
			num |= (ulong)((long)((!linear) ? 0 : 1) << 34);
			num |= (ulong)((long)((!uav) ? 0 : 1) << 35);
			num |= (ulong)((long)((!mipMaps) ? 0 : 1) << 36);
			num |= (ulong)((long)format << 37);
			RenderTexturesCache value;
			if (!cache.TryGetValue(num, out value))
			{
				value = (cache[num] = new RenderTexturesCache(num, width, height, depthBuffer, format, linear, uav, mipMaps));
			}
			return value;
		}

		public static TemporaryRenderTexture GetTemporary(int width, int height, int depthBuffer, RenderTextureFormat format, bool linear, bool uav, bool mipMaps = false)
		{
			return GetCache(width, height, depthBuffer, format, linear, uav, mipMaps).GetTemporary();
		}

		public TemporaryRenderTexture GetTemporary()
		{
			return new TemporaryRenderTexture(this);
		}

		public RenderTexture GetTemporaryDirect()
		{
			RenderTexture renderTexture;
			if (renderTextures.Count == 0)
			{
				renderTexture = new RenderTexture(width, height, depthBuffer, format, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
				renderTexture.hideFlags = HideFlags.DontSave;
				renderTexture.name = "Temporary#" + hash;
				renderTexture.filterMode = FilterMode.Point;
				renderTexture.anisoLevel = 1;
				renderTexture.wrapMode = TextureWrapMode.Repeat;
				renderTexture.useMipMap = mipMaps;
				renderTexture.autoGenerateMips = mipMaps;
				if (uav)
				{
					renderTexture.enableRandomWrite = true;
				}
			}
			else
			{
				renderTexture = renderTextures.Dequeue();
			}
			if (uav && !renderTexture.IsCreated())
			{
				renderTexture.Create();
			}
			if (renderTextures.Count == 0)
			{
				lastFrameAllUsed = Time.frameCount;
			}
			return renderTexture;
		}

		public void ReleaseTemporaryDirect(RenderTexture renderTexture)
		{
			renderTextures.Enqueue(renderTexture);
		}

		internal void Update(int frame)
		{
			if (frame - lastFrameAllUsed > 3 && renderTextures.Count != 0)
			{
				RenderTexture obj = renderTextures.Dequeue();
				obj.Destroy();
			}
		}
	}
}
