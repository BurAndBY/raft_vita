using UnityEngine;

namespace PlayWay.Water
{
	public class WaterOverlaysData
	{
		private RenderTexture dynamicDisplacementMap;

		private RenderTexture slopeMapA;

		private RenderTexture slopeMapB;

		private RenderTexture totalDisplacementMap;

		private RenderTexture utilityMap;

		private WaterCamera camera;

		private WaterOverlays waveOverlays;

		private bool totalDisplacementMapDirty;

		private bool initialization;

		internal int lastFrameUsed;

		public RenderTexture DynamicDisplacementMap
		{
			get
			{
				return dynamicDisplacementMap;
			}
		}

		public bool Initialization
		{
			get
			{
				return initialization;
			}
			set
			{
				initialization = value;
			}
		}

		public RenderTexture SlopeMap
		{
			get
			{
				return slopeMapA;
			}
		}

		public RenderTexture SlopeMapPrevious
		{
			get
			{
				return slopeMapB;
			}
		}

		public RenderTexture UtilityMap
		{
			get
			{
				return utilityMap;
			}
		}

		public WaterCamera Camera
		{
			get
			{
				return camera;
			}
		}

		public WaterOverlaysData(WaterOverlays waveOverlays, WaterCamera camera, int resolution, int antialiasing)
		{
			this.waveOverlays = waveOverlays;
			this.camera = camera;
			initialization = true;
			dynamicDisplacementMap = CreateOverlayRT("Water Overlay: Displacement", RenderTextureFormat.ARGBHalf, resolution, antialiasing);
			slopeMapA = CreateOverlayRT("Water Overlay: Slope A", RenderTextureFormat.ARGBHalf, resolution, antialiasing);
			slopeMapB = CreateOverlayRT("Water Overlay: Slope B", RenderTextureFormat.ARGBHalf, resolution, antialiasing);
			totalDisplacementMap = CreateOverlayRT("Water Overlay: Total Displacement", RenderTextureFormat.ARGBHalf, resolution, antialiasing);
			if (waveOverlays.GetComponent<WaterSpray>() != null)
			{
				utilityMap = CreateOverlayRT("Water Overlay: Utility Map", RenderTextureFormat.RGHalf, resolution, antialiasing);
			}
			Graphics.SetRenderTarget(slopeMapA);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 1f));
			Graphics.SetRenderTarget(null);
		}

		public RenderTexture GetTotalDisplacementMap()
		{
			if (totalDisplacementMapDirty)
			{
				waveOverlays.RenderTotalDisplacementMap(totalDisplacementMap);
				totalDisplacementMapDirty = false;
			}
			return totalDisplacementMap;
		}

		public void Dispose()
		{
			if (dynamicDisplacementMap != null)
			{
				dynamicDisplacementMap.Destroy();
				dynamicDisplacementMap = null;
			}
			if (slopeMapA != null)
			{
				slopeMapA.Destroy();
				slopeMapA = null;
			}
			if (slopeMapB != null)
			{
				slopeMapB.Destroy();
				slopeMapB = null;
			}
			if (totalDisplacementMap != null)
			{
				totalDisplacementMap.Destroy();
				totalDisplacementMap = null;
			}
			if (utilityMap != null)
			{
				utilityMap.Destroy();
				utilityMap = null;
			}
		}

		public void ClearOverlays()
		{
			SwapSlopeMaps();
			Graphics.SetRenderTarget(dynamicDisplacementMap);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			Graphics.SetRenderTarget(slopeMapA);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 1f));
			if (utilityMap != null)
			{
				Graphics.SetRenderTarget(utilityMap);
				GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			}
			totalDisplacementMapDirty = true;
		}

		private RenderTexture CreateOverlayRT(string name, RenderTextureFormat format, int resolution, int antialiasing)
		{
			RenderTexture renderTexture = new RenderTexture(resolution, resolution, 0, format, RenderTextureReadWrite.Linear);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.antiAliasing = antialiasing;
			renderTexture.filterMode = FilterMode.Bilinear;
			renderTexture.wrapMode = TextureWrapMode.Clamp;
			renderTexture.name = name;
			Graphics.SetRenderTarget(renderTexture);
			GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
			return renderTexture;
		}

		public void SwapSlopeMaps()
		{
			RenderTexture renderTexture = slopeMapB;
			slopeMapB = slopeMapA;
			slopeMapA = renderTexture;
		}
	}
}
