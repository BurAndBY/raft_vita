using UnityEngine;
using UnityEngine.Rendering;

public class WaterShadowCastingLight : MonoBehaviour
{
	private CommandBuffer commandBuffer1;

	private int shadowmapId;

	private void Start()
	{
		int num = Shader.PropertyToID("_WaterShadowmap");
		commandBuffer1 = new CommandBuffer();
		commandBuffer1.name = "Water: Copy Shadowmap";
		commandBuffer1.GetTemporaryRT(num, Screen.width, Screen.height, 32, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		commandBuffer1.Blit(BuiltinRenderTextureType.CurrentActive, num);
		Light component = GetComponent<Light>();
		component.AddCommandBuffer(LightEvent.AfterScreenspaceMask, commandBuffer1);
	}
}
