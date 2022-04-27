using UnityEngine;

public interface IWaterDisplacements
{
	float MaxVerticalDisplacement { get; }

	float MaxHorizontalDisplacement { get; }

	Vector3 GetDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

	Vector2 GetHorizontalDisplacementAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

	float GetHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time);

	Vector4 GetForceAndHeightAt(float x, float z, float spectrumStart, float spectrumEnd, float time);
}
