using System;
using System.Threading;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WavesRendererFFT
	{
		public enum SpectrumType
		{
			Phillips,
			Unified
		}

		[Flags]
		public enum MapType
		{
			Displacement = 1,
			Slope = 2
		}

		public enum FlattenMode
		{
			Auto,
			ForcedOn,
			ForcedOff
		}

		[SerializeField]
		[HideInInspector]
		private Shader fftShader;

		[SerializeField]
		[HideInInspector]
		private Shader fftUtilitiesShader;

		[SerializeField]
		private ComputeShader dx11FFT;

		[Tooltip("Determines if GPU partial derivatives or Fast Fourier Transform (high quality) should be used to compute slope map (Recommended: on). Works only if displacement map rendering is enabled.")]
		[SerializeField]
		private bool highQualitySlopeMaps = true;

		[SerializeField]
		[Tooltip("Check this option, if your water is flat or game crashes instantly on a DX11 GPU (in editor or build). Compute shaders are very fast, so use this as a last resort.")]
		private bool forcePixelShader;

		[SerializeField]
		[Tooltip("Fixes crest artifacts during storms, but lowers overall quality. Enabled by default when used with additive water volumes as it is actually needed and disabled in all other cases.")]
		private FlattenMode flattenMode;

		private RenderTexture[] slopeMaps;

		private RenderTexture[] displacementMaps;

		private RenderTexture displacedHeightMap;

		private RenderTexturesCache singleTargetCache;

		private RenderTexturesCache doubleTargetCache;

		private RenderTexturesCache displacedHeightMapsCache;

		private Water water;

		private WindWaves windWaves;

		private GpuFFT heightFFT;

		private GpuFFT slopeFFT;

		private GpuFFT displacementFFT;

		private Material fftUtilitiesMaterial;

		private MapType renderedMaps;

		private bool finalHighQualitySlopeMaps;

		private bool flatten;

		private bool enabled;

		private bool copyModeDirty;

		private int waveMapsFrame;

		private int displacementMapJacobianFrame;

		private WindWaves lastCopyFrom;

		private static ComputeShader defaultDx11Fft;

		private static Vector4[] offsets = new Vector4[4]
		{
			new Vector4(0f, 0f, 0f, 0f),
			new Vector4(0.5f, 0f, 0f, 0f),
			new Vector4(0f, 0.5f, 0f, 0f),
			new Vector4(0.5f, 0.5f, 0f, 0f)
		};

		private static Vector4[] offsetsDual = new Vector4[2]
		{
			new Vector4(0f, 0f, 0.5f, 0f),
			new Vector4(0f, 0.5f, 0.5f, 0.5f)
		};

		public MapType RenderedMaps
		{
			get
			{
				return renderedMaps;
			}
			set
			{
				renderedMaps = value;
				if (enabled && Application.isPlaying)
				{
					Dispose(false);
					ValidateResources();
				}
			}
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
		}

		private bool FloatingPointMipMapsSupport
		{
			get
			{
				string text = SystemInfo.graphicsDeviceVendor.ToLower();
				return !text.Contains("amd") && !text.Contains("ati") && !SystemInfo.graphicsDeviceName.ToLower().Contains("radeon") && WaterProjectSettings.Instance.AllowFloatingPointMipMaps;
			}
		}

		public Texture DisplacedHeightMap
		{
			get
			{
				return displacedHeightMap;
			}
		}

		public event Action ResourcesChanged;

		internal void Enable(WindWaves windWaves)
		{
			if (enabled)
			{
				return;
			}
			if (dx11FFT != null)
			{
				defaultDx11Fft = dx11FFT;
			}
			else
			{
				dx11FFT = defaultDx11Fft;
			}
			enabled = true;
			water = windWaves.GetComponent<Water>();
			this.windWaves = windWaves;
			OnCopyModeChanged();
			if (Application.isPlaying)
			{
				if (lastCopyFrom == null)
				{
					ValidateResources();
				}
				windWaves.ResolutionChanged.AddListener(OnResolutionChanged);
			}
			OnValidate(windWaves);
			water.InvalidateMaterialKeywords();
			fftUtilitiesMaterial = new Material(fftUtilitiesShader);
			fftUtilitiesMaterial.hideFlags = HideFlags.DontSave;
		}

		internal void Disable()
		{
			if (enabled)
			{
				enabled = false;
				Dispose(false);
				if (water != null)
				{
					water.InvalidateMaterialKeywords();
				}
			}
		}

		public Texture GetDisplacementMap(int index)
		{
			return (displacementMaps == null) ? null : displacementMaps[index];
		}

		public Texture GetSlopeMap(int index)
		{
			return slopeMaps[index];
		}

		public void BuildShaderVariant(ShaderVariant variant, Water water, WindWaves windWaves, WaterQualityLevel qualityLevel)
		{
			OnValidate(windWaves);
			ResolveFinalSettings(qualityLevel);
			variant.SetWaterKeyword("_WAVES_FFT_SLOPE", enabled && renderedMaps == MapType.Slope);
			variant.SetUnityKeyword("_WAVES_ALIGN", (!water.Volume.Boundless && water.Volume.HasRenderableAdditiveVolumes && flattenMode == FlattenMode.Auto) || flattenMode == FlattenMode.ForcedOn);
			variant.SetUnityKeyword("_WAVES_FFT", enabled && (renderedMaps & MapType.Displacement) != 0);
		}

		private void ValidateResources()
		{
			if (windWaves.CopyFrom == null)
			{
				ValidateFFT(ref heightFFT, (renderedMaps & MapType.Displacement) != 0, false);
				ValidateFFT(ref displacementFFT, (renderedMaps & MapType.Displacement) != 0, true);
				ValidateFFT(ref slopeFFT, (renderedMaps & MapType.Slope) != 0, true);
			}
			if (displacementMaps != null && slopeMaps != null && !(displacedHeightMap == null))
			{
				return;
			}
			bool flag = (!water.Volume.Boundless && flattenMode == FlattenMode.Auto) || flattenMode == FlattenMode.ForcedOn;
			if (flatten != flag)
			{
				flatten = flag;
				if (displacedHeightMap != null)
				{
					displacedHeightMap.Destroy();
					displacedHeightMap = null;
				}
			}
			RenderTexture[] array;
			RenderTexture[] array2;
			RenderTexture value;
			if (windWaves.CopyFrom == null)
			{
				int finalResolution = windWaves.FinalResolution;
				int num = ((!flag) ? (finalResolution >> 2) : finalResolution);
				int num2 = finalResolution << 1;
				singleTargetCache = RenderTexturesCache.GetCache(num2, num2, 0, RenderTextureFormat.RHalf, true, heightFFT is Dx11FFT);
				doubleTargetCache = RenderTexturesCache.GetCache(num2, num2, 0, RenderTextureFormat.RGHalf, true, displacementFFT is Dx11FFT);
				displacedHeightMapsCache = RenderTexturesCache.GetCache(num, num, 0, RenderTextureFormat.ARGBHalf, true, false);
				if (displacementMaps == null && (renderedMaps & MapType.Displacement) != 0)
				{
					CreateRenderTextures(ref displacementMaps, RenderTextureFormat.ARGBHalf, 4, true);
				}
				if (slopeMaps == null && (renderedMaps & MapType.Slope) != 0)
				{
					CreateRenderTextures(ref slopeMaps, RenderTextureFormat.ARGBHalf, 2, true);
				}
				if (displacedHeightMap == null)
				{
					displacedHeightMap = new RenderTexture(num, num, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
					displacedHeightMap.hideFlags = HideFlags.DontSave;
					displacedHeightMap.wrapMode = TextureWrapMode.Repeat;
					if (FloatingPointMipMapsSupport)
					{
						displacedHeightMap.filterMode = FilterMode.Trilinear;
						displacedHeightMap.useMipMap = true;
						displacedHeightMap.autoGenerateMips = true;
					}
					else
					{
						displacedHeightMap.filterMode = FilterMode.Bilinear;
					}
				}
				array = displacementMaps;
				array2 = slopeMaps;
				value = displacedHeightMap;
			}
			else
			{
				WindWaves copyFrom = windWaves.CopyFrom;
				if (copyFrom.WaterWavesFFT.windWaves == null)
				{
					copyFrom.ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
				}
				copyFrom.WaterWavesFFT.ValidateResources();
				array = copyFrom.WaterWavesFFT.displacementMaps;
				array2 = copyFrom.WaterWavesFFT.slopeMaps;
				value = copyFrom.WaterWavesFFT.displacedHeightMap;
			}
			for (int i = 0; i < 4; i++)
			{
				string text = ((i == 0) ? string.Empty : i.ToString());
				if (array != null)
				{
					water.WaterMaterial.SetTexture("_GlobalDisplacementMap" + text, array[i]);
					water.WaterBackMaterial.SetTexture("_GlobalDisplacementMap" + text, array[i]);
				}
				if (i < 2 && array2 != null)
				{
					water.WaterMaterial.SetTexture("_GlobalNormalMap" + text, array2[i]);
					water.WaterBackMaterial.SetTexture("_GlobalNormalMap" + text, array2[i]);
				}
			}
			water.WaterMaterial.SetTexture("_DisplacedHeightMaps", value);
			water.WaterBackMaterial.SetTexture("_DisplacedHeightMaps", value);
			water.WaterVolumeMaterial.SetTexture("_DisplacedHeightMaps", value);
			if (this.ResourcesChanged != null)
			{
				this.ResourcesChanged.Invoke();
			}
		}

		public void OnCopyModeChanged()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			copyModeDirty = true;
			if (lastCopyFrom != null)
			{
				lastCopyFrom.WaterWavesFFT.ResourcesChanged -= new Action(ValidateResources);
			}
			if (windWaves.CopyFrom != null)
			{
				windWaves.CopyFrom.WaterWavesFFT.ResourcesChanged += new Action(ValidateResources);
			}
			lastCopyFrom = windWaves.CopyFrom;
			Dispose(false);
		}

		private void CreateRenderTextures(ref RenderTexture[] renderTextures, RenderTextureFormat format, int count, bool mipMaps)
		{
			renderTextures = new RenderTexture[count];
			for (int i = 0; i < count; i++)
			{
				renderTextures[i] = CreateRenderTexture(format, mipMaps);
			}
		}

		private RenderTexture CreateRenderTexture(RenderTextureFormat format, bool mipMaps)
		{
			RenderTexture renderTexture = new RenderTexture(windWaves.FinalResolution, windWaves.FinalResolution, 0, format, RenderTextureReadWrite.Linear);
			renderTexture.hideFlags = HideFlags.DontSave;
			renderTexture.wrapMode = TextureWrapMode.Repeat;
			if (mipMaps && FloatingPointMipMapsSupport)
			{
				renderTexture.filterMode = FilterMode.Trilinear;
				renderTexture.useMipMap = true;
				renderTexture.autoGenerateMips = true;
			}
			else
			{
				renderTexture.filterMode = FilterMode.Bilinear;
			}
			return renderTexture;
		}

		private void ValidateFFT(ref GpuFFT fft, bool present, bool twoChannels)
		{
			if (present)
			{
				if (fft == null)
				{
					fft = ChooseBestFFTAlgorithm(twoChannels);
				}
			}
			else if (fft != null)
			{
				fft.Dispose();
				fft = null;
			}
		}

		private GpuFFT ChooseBestFFTAlgorithm(bool twoChannels)
		{
			int finalResolution = windWaves.FinalResolution;
			GpuFFT gpuFFT = ((forcePixelShader || !(dx11FFT != null) || !SystemInfo.supportsComputeShaders || finalResolution > 512) ? ((GpuFFT)new PixelShaderFFT(fftShader, finalResolution, windWaves.FinalHighPrecision || finalResolution >= 2048, twoChannels)) : ((GpuFFT)new Dx11FFT(dx11FFT, finalResolution, windWaves.FinalHighPrecision || finalResolution >= 2048, twoChannels)));
			gpuFFT.SetupMaterials();
			return gpuFFT;
		}

		internal void ResolveFinalSettings(WaterQualityLevel qualityLevel)
		{
			finalHighQualitySlopeMaps = highQualitySlopeMaps;
			if (!qualityLevel.allowHighQualitySlopeMaps)
			{
				finalHighQualitySlopeMaps = false;
			}
			if ((renderedMaps & MapType.Displacement) == 0)
			{
				finalHighQualitySlopeMaps = true;
			}
		}

		internal void OnValidate(WindWaves windWaves)
		{
			if (fftShader == null)
			{
				fftShader = Shader.Find("PlayWay Water/Base/FFT");
			}
			if (fftUtilitiesShader == null)
			{
				fftUtilitiesShader = Shader.Find("PlayWay Water/Utilities/FFT Utilities");
			}
			if (Application.isPlaying && enabled)
			{
				ResolveFinalSettings(WaterQualitySettings.Instance.CurrentQualityLevel);
			}
		}

		private void OnDestroy()
		{
			Dispose(true);
		}

		private void Dispose(bool total)
		{
			waveMapsFrame = -1;
			if (heightFFT != null)
			{
				heightFFT.Dispose();
				heightFFT = null;
			}
			if (slopeFFT != null)
			{
				slopeFFT.Dispose();
				slopeFFT = null;
			}
			if (displacementFFT != null)
			{
				displacementFFT.Dispose();
				displacementFFT = null;
			}
			if (displacedHeightMap != null)
			{
				displacedHeightMap.Destroy();
				displacedHeightMap = null;
			}
			if (slopeMaps != null)
			{
				RenderTexture[] array = slopeMaps;
				foreach (RenderTexture obj in array)
				{
					obj.Destroy();
				}
				slopeMaps = null;
			}
			if (displacementMaps != null)
			{
				RenderTexture[] array2 = displacementMaps;
				foreach (RenderTexture obj2 in array2)
				{
					obj2.Destroy();
				}
				displacementMaps = null;
			}
			if (total && fftUtilitiesMaterial != null)
			{
				fftUtilitiesMaterial.Destroy();
				fftUtilitiesMaterial = null;
			}
		}

		public void OnWaterRender(Camera camera)
		{
			if (!(fftUtilitiesMaterial == null))
			{
				ValidateWaveMaps();
			}
		}

		private void OnResolutionChanged(WindWaves windWaves)
		{
			Dispose(false);
			ValidateResources();
		}

		private void ValidateWaveMaps()
		{
			int frameCount = Time.frameCount;
			if (waveMapsFrame == frameCount || !Application.isPlaying)
			{
				return;
			}
			if (lastCopyFrom != null)
			{
				if (copyModeDirty)
				{
					copyModeDirty = false;
					ValidateResources();
				}
				return;
			}
			waveMapsFrame = frameCount;
			Texture heightSpectrum;
			Texture slopeSpectrum;
			Texture displacementSpectrum;
			RenderSpectra(out heightSpectrum, out slopeSpectrum, out displacementSpectrum);
			if ((renderedMaps & MapType.Displacement) != 0)
			{
				ClearDisplacedHeightMaps();
				TemporaryRenderTexture temporary = displacedHeightMapsCache.GetTemporary();
				TemporaryRenderTexture temporary2 = singleTargetCache.GetTemporary();
				TemporaryRenderTexture temporary3 = doubleTargetCache.GetTemporary();
				heightFFT.ComputeFFT(heightSpectrum, temporary2);
				displacementFFT.ComputeFFT(displacementSpectrum, temporary3);
				fftUtilitiesMaterial.SetTexture("_HeightTex", (RenderTexture)temporary2);
				fftUtilitiesMaterial.SetTexture("_DisplacementTex", (RenderTexture)temporary3);
				fftUtilitiesMaterial.SetFloat("_HorizontalDisplacementScale", water.HorizontalDisplacementScale);
				for (int i = 0; i < 4; i++)
				{
					fftUtilitiesMaterial.SetFloat("_JacobianScale", water.HorizontalDisplacementScale * 0.1f * (float)displacementMaps[i].width / windWaves.TileSizes[i]);
					fftUtilitiesMaterial.SetVector("_Offset", offsets[i]);
					Graphics.Blit(null, displacementMaps[i], fftUtilitiesMaterial, 1);
					RenderDisplacedHeightMaps(displacementMaps[i], temporary, i);
				}
				temporary2.Dispose();
				temporary3.Dispose();
				Graphics.Blit((RenderTexture)temporary, displacedHeightMap);
				temporary.Dispose();
			}
			if ((renderedMaps & MapType.Slope) == 0)
			{
				return;
			}
			if (!finalHighQualitySlopeMaps)
			{
				for (int j = 0; j < 2; j++)
				{
					int finalResolution = windWaves.FinalResolution;
					fftUtilitiesMaterial.SetFloat("_Intensity1", 0.58f * (float)finalResolution / windWaves.TileSizes[j * 2]);
					fftUtilitiesMaterial.SetFloat("_Intensity2", 0.58f * (float)finalResolution / windWaves.TileSizes[j * 2 + 1]);
					fftUtilitiesMaterial.SetTexture("_MainTex", displacementMaps[j * 2]);
					fftUtilitiesMaterial.SetTexture("_SecondTex", displacementMaps[j * 2 + 1]);
					Graphics.Blit(null, slopeMaps[j], fftUtilitiesMaterial, 0);
				}
			}
			else
			{
				TemporaryRenderTexture temporary4 = doubleTargetCache.GetTemporary();
				slopeFFT.ComputeFFT(slopeSpectrum, temporary4);
				for (int k = 0; k < 2; k++)
				{
					fftUtilitiesMaterial.SetVector("_Offset", offsetsDual[k]);
					Graphics.Blit((RenderTexture)temporary4, slopeMaps[k], fftUtilitiesMaterial, 3);
				}
				temporary4.Dispose();
			}
		}

		private void ClearDisplacedHeightMaps()
		{
		}

		private void RenderDisplacedHeightMaps(RenderTexture displacementMap, RenderTexture target, int channel)
		{
			int width = displacementMap.width;
			int num = ((!flatten) ? (width >> 2) : width);
			int vertexCount = num * num;
			Mesh[] meshes = water.Geometry.GetMeshes(WaterGeometryType.UniformGrid, vertexCount, false);
			fftUtilitiesMaterial.SetFloat("_ColorMask", 8 >> channel);
			fftUtilitiesMaterial.SetFloat("_WorldToPixelSpace", 2f / windWaves.TileSizes[channel]);
			fftUtilitiesMaterial.SetTexture("_MainTex", displacementMap);
			Graphics.SetRenderTarget(target);
			//if (fftUtilitiesMaterial.SetPass(5))
			//{
			//	Mesh[] array = meshes;
			//	foreach (Mesh mesh in array)
			//	{
			//		Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
			//	}
			//}
		}

		private void RenderSpectra(out Texture heightSpectrum, out Texture slopeSpectrum, out Texture displacementSpectrum)
		{
			float time = water.Time;
			if (renderedMaps == MapType.Slope)
			{
				heightSpectrum = null;
				displacementSpectrum = null;
				slopeSpectrum = windWaves.SpectrumResolver.RenderSlopeSpectrumAt(time);
			}
			else if ((renderedMaps & MapType.Slope) == 0 || !finalHighQualitySlopeMaps)
			{
				slopeSpectrum = null;
				windWaves.SpectrumResolver.RenderDisplacementsSpectraAt(time, out heightSpectrum, out displacementSpectrum);
			}
			else
			{
				windWaves.SpectrumResolver.RenderCompleteSpectraAt(time, out heightSpectrum, out slopeSpectrum, out displacementSpectrum);
			}
		}
	}
}
