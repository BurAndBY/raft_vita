using UnityEngine;

namespace PlayWay.Water
{
	public interface IShaderCollectionBuilder
	{
		Shader BuildShaderVariant(string[] localKeywords, string[] sharedKeywords, string additionalCode, string keywordsString, bool volume);

		void BuildSceneShaderCollection(ShaderCollection shaderCollection);
	}
}
