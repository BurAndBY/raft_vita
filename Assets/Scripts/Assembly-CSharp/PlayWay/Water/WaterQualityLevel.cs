using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public struct WaterQualityLevel
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public int maxSpectrumResolution;

		[SerializeField]
		public bool allowHighPrecisionTextures;

		[SerializeField]
		public bool allowHighQualitySlopeMaps;

		[Range(0f, 1f)]
		[SerializeField]
		public float tileSizeScale;

		[SerializeField]
		public WaterWavesMode wavesMode;

		[SerializeField]
		public bool allowSpray;

		[SerializeField]
		[Range(0f, 1f)]
		public float foamQuality;

		[SerializeField]
		public bool allowVolumetricLighting;

		[Range(0f, 1f)]
		[SerializeField]
		public float maxTesselationFactor;

		[SerializeField]
		public int maxVertexCount;

		[SerializeField]
		public int maxTesselatedVertexCount;

		[SerializeField]
		public bool allowAlphaBlending;

		[SerializeField]
		public bool allowHighQualityReflections;

		public void ResetToDefaults()
		{
			name = string.Empty;
			maxSpectrumResolution = 256;
			allowHighPrecisionTextures = true;
			tileSizeScale = 1f;
			wavesMode = WaterWavesMode.AllowAll;
			allowSpray = true;
			foamQuality = 1f;
			allowVolumetricLighting = true;
			maxTesselationFactor = 1f;
			maxVertexCount = 500000;
			maxTesselatedVertexCount = 120000;
			allowAlphaBlending = true;
			allowHighQualityReflections = false;
		}
	}
}
