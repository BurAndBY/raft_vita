using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class Blur
	{
		[HideInInspector]
		[SerializeField]
		private Shader blurShader;

		[Range(0f, 10f)]
		[SerializeField]
		private int iterations = 1;

		[SerializeField]
		private float size = 0.005f;

		private Material blurMaterial;

		private int passIndex;

		private int offsetHash;

		public int Iterations
		{
			get
			{
				return iterations;
			}
			set
			{
				float totalSize = TotalSize;
				iterations = value;
				TotalSize = totalSize;
			}
		}

		public float Size
		{
			get
			{
				return size;
			}
			set
			{
				size = value;
			}
		}

		public float TotalSize
		{
			get
			{
				return size * (float)iterations;
			}
			set
			{
				size = value / (float)iterations;
			}
		}

		public Material BlurMaterial
		{
			get
			{
				if (blurMaterial == null)
				{
					if (blurShader == null)
					{
						Validate("PlayWay Water/Utilities/Blur");
					}
					blurMaterial = new Material(blurShader);
					blurMaterial.hideFlags = HideFlags.DontSave;
					offsetHash = Shader.PropertyToID("_Offset");
				}
				return blurMaterial;
			}
			set
			{
				blurMaterial = value;
			}
		}

		public int PassIndex
		{
			get
			{
				return passIndex;
			}
			set
			{
				passIndex = value;
			}
		}

		public void Apply(RenderTexture tex)
		{
			if (iterations != 0)
			{
				Material material = BlurMaterial;
				FilterMode filterMode = tex.filterMode;
				tex.filterMode = FilterMode.Bilinear;
				RenderTexture temporary = RenderTexture.GetTemporary(tex.width, tex.height, 0, tex.format);
				temporary.filterMode = FilterMode.Bilinear;
				for (int i = 0; i < iterations; i++)
				{
					float num = size * (1f + (float)i * 0.5f);
					material.SetVector(offsetHash, new Vector4(num, 0f, 0f, 0f));
					Graphics.Blit(tex, temporary, material, passIndex);
					material.SetVector(offsetHash, new Vector4(0f, num, 0f, 0f));
					Graphics.Blit(temporary, tex, material, passIndex);
				}
				tex.filterMode = filterMode;
				RenderTexture.ReleaseTemporary(temporary);
			}
		}

		public void Validate(string shaderName)
		{
			blurShader = Shader.Find(shaderName);
		}

		public void Dispose()
		{
			if (blurMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(blurMaterial);
			}
		}
	}
}
