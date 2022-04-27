using System;
using System.Threading;
using UnityEngine;

namespace PlayWay.Water
{
	public class WaterQualitySettings : ScriptableObjectSingleton
	{
		[SerializeField]
		private WaterQualityLevel[] qualityLevels;

		[SerializeField]
		private bool synchronizeWithUnity = true;

		[SerializeField]
		private int savedCustomQualityLevel;

		private int waterQualityIndex;

		private WaterQualityLevel currentQualityLevel;

		private static WaterQualitySettings instance;

		public static WaterQualitySettings Instance
		{
			get
			{
				if ((object)instance == null)
				{
					instance = ScriptableObjectSingleton.LoadSingleton<WaterQualitySettings>();
					instance.Changed = null;
					instance.waterQualityIndex = -1;
					instance.SynchronizeQualityLevel();
				}
				return instance;
			}
		}

		public string[] Names
		{
			get
			{
				string[] array = new string[qualityLevels.Length];
				for (int i = 0; i < qualityLevels.Length; i++)
				{
					array[i] = qualityLevels[i].name;
				}
				return array;
			}
		}

		public int MaxSpectrumResolution
		{
			get
			{
				return currentQualityLevel.maxSpectrumResolution;
			}
			set
			{
				if (currentQualityLevel.maxSpectrumResolution != value)
				{
					currentQualityLevel.maxSpectrumResolution = value;
					OnChange();
				}
			}
		}

		public float TileSizeScale
		{
			get
			{
				return currentQualityLevel.tileSizeScale;
			}
			set
			{
				if (currentQualityLevel.tileSizeScale != value)
				{
					currentQualityLevel.tileSizeScale = value;
					OnChange();
				}
			}
		}

		public bool AllowHighPrecisionTextures
		{
			get
			{
				return currentQualityLevel.allowHighPrecisionTextures;
			}
			set
			{
				if (currentQualityLevel.allowHighPrecisionTextures != value)
				{
					currentQualityLevel.allowHighPrecisionTextures = value;
					OnChange();
				}
			}
		}

		public bool AllowHighQualitySlopeMaps
		{
			get
			{
				return currentQualityLevel.allowHighQualitySlopeMaps;
			}
			set
			{
				if (currentQualityLevel.allowHighQualitySlopeMaps != value)
				{
					currentQualityLevel.allowHighQualitySlopeMaps = value;
					OnChange();
				}
			}
		}

		public WaterWavesMode WavesMode
		{
			get
			{
				return currentQualityLevel.wavesMode;
			}
			set
			{
				if (currentQualityLevel.wavesMode != value)
				{
					currentQualityLevel.wavesMode = value;
					OnChange();
				}
			}
		}

		public bool AllowVolumetricLighting
		{
			get
			{
				return currentQualityLevel.allowVolumetricLighting;
			}
			set
			{
				if (currentQualityLevel.allowVolumetricLighting != value)
				{
					currentQualityLevel.allowVolumetricLighting = value;
					OnChange();
				}
			}
		}

		public float MaxTesselationFactor
		{
			get
			{
				return currentQualityLevel.maxTesselationFactor;
			}
			set
			{
				if (currentQualityLevel.maxTesselationFactor != value)
				{
					currentQualityLevel.maxTesselationFactor = value;
					OnChange();
				}
			}
		}

		public int MaxVertexCount
		{
			get
			{
				return currentQualityLevel.maxVertexCount;
			}
			set
			{
				if (currentQualityLevel.maxVertexCount != value)
				{
					currentQualityLevel.maxVertexCount = value;
					OnChange();
				}
			}
		}

		public int MaxTesselatedVertexCount
		{
			get
			{
				return currentQualityLevel.maxTesselatedVertexCount;
			}
			set
			{
				if (currentQualityLevel.maxTesselatedVertexCount != value)
				{
					currentQualityLevel.maxTesselatedVertexCount = value;
					OnChange();
				}
			}
		}

		public bool AllowAlphaBlending
		{
			get
			{
				return currentQualityLevel.allowAlphaBlending;
			}
			set
			{
				if (currentQualityLevel.allowAlphaBlending != value)
				{
					currentQualityLevel.allowAlphaBlending = value;
					OnChange();
				}
			}
		}

		public bool AllowHighQualityReflections
		{
			get
			{
				return currentQualityLevel.allowHighQualityReflections;
			}
			set
			{
				if (currentQualityLevel.allowHighQualityReflections != value)
				{
					currentQualityLevel.allowHighQualityReflections = value;
					OnChange();
				}
			}
		}

		public bool SynchronizeWithUnity
		{
			get
			{
				return synchronizeWithUnity;
			}
		}

		internal WaterQualityLevel CurrentQualityLevel
		{
			get
			{
				return currentQualityLevel;
			}
		}

		public event Action Changed;

		public int GetQualityLevel()
		{
			return waterQualityIndex;
		}

		public void SetQualityLevel(int index)
		{
			if (!Application.isPlaying)
			{
				savedCustomQualityLevel = index;
			}
			currentQualityLevel = qualityLevels[index];
			waterQualityIndex = index;
			OnChange();
		}

		public void SynchronizeQualityLevel()
		{
			int num = -1;
			if (synchronizeWithUnity)
			{
				num = FindQualityLevel(QualitySettings.names[QualitySettings.GetQualityLevel()]);
			}
			if (num == -1)
			{
				num = savedCustomQualityLevel;
			}
			num = Mathf.Clamp(num, 0, qualityLevels.Length - 1);
			if (num != waterQualityIndex)
			{
				SetQualityLevel(num);
			}
		}

		internal WaterQualityLevel[] GetQualityLevelsDirect()
		{
			return qualityLevels;
		}

		private void OnChange()
		{
			if (this.Changed != null)
			{
				this.Changed.Invoke();
			}
		}

		private int FindQualityLevel(string name)
		{
			for (int i = 0; i < qualityLevels.Length; i++)
			{
				if (qualityLevels[i].name == name)
				{
					return i;
				}
			}
			return -1;
		}

		private void SynchronizeLevelNames()
		{
		}
	}
}
