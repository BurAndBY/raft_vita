using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class ShaderCollection : ScriptableObject
	{
		[SerializeField]
		private Shader[] shaders;

		private static bool errorDisplayed;

		public static Shader GetRuntimeShaderVariant(string keywordsString, bool volume)
		{
			Shader shader = Shader.Find("PlayWay Water/Variations/Water " + ((!volume) ? string.Empty : "Volume ") + keywordsString);
			if (shader == null && !errorDisplayed && Application.isPlaying)
			{
				Debug.LogError("Could not find proper water shader variation. Select your water and click \"Save Asset\" button to build proper shaders. Missing shader: \"PlayWay Water/Variations/Water " + ((!volume) ? string.Empty : "Volume ") + keywordsString + "\"");
				errorDisplayed = true;
			}
			return shader;
		}

		public Shader GetShaderVariant(string[] localKeywords, string[] sharedKeywords, string additionalCode, string keywordsString, bool volume)
		{
			Array.Sort(localKeywords);
			Array.Sort(sharedKeywords);
			string text = ((!volume) ? string.Empty : "Volume ") + keywordsString;
			return Shader.Find("PlayWay Water/Variations/Water " + text);
		}

		public Shader[] GetShadersDirect()
		{
			return shaders;
		}

		public void Build()
		{
		}

		public bool ContainsShaderVariant(string keywordsString)
		{
			if (shaders != null)
			{
				Shader[] array = shaders;
				foreach (Shader shader in array)
				{
					if (shader != null && shader.name.EndsWith(keywordsString))
					{
						return true;
					}
				}
			}
			return false;
		}

		private void AddShader(Shader shader)
		{
			if (shaders != null)
			{
				Array.Resize(ref shaders, shaders.Length + 1);
				shaders[shaders.Length - 1] = shader;
			}
			else
			{
				shaders = new Shader[1] { shader };
			}
		}
	}
}
