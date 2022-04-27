using UnityEngine;

public interface IWaterInteraction
{
	Renderer InteractionRenderer { get; }

	int Layer { get; }
}
