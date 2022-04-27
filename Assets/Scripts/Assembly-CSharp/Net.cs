using UnityEngine;

public class Net : MonoBehaviour
{
	public ItemCollector collector;

	private CanvasHelper canvas;

	private Transform player;

	private static BlockPlacer blockplacer;

	private void Start()
	{
		canvas = CanvasHelper.singleton;
		player = GameManager.singleton.player.transform;
		if (blockplacer == null)
		{
			blockplacer = Object.FindObjectOfType<BlockPlacer>();
		}
	}

	public void OnMouseOver()
	{
		if (GameManager.IsInMenu || collector == null || (blockplacer != null && blockplacer.GetCurrentBlockType() == BlockType.Repair && blockplacer.enabled))
		{
			return;
		}
		if (!Player.IsWithingDistance(base.transform.position, Player.UseDistance * 1.5f))
		{
			canvas.SetDisplayText(false);
			return;
		}
		int itemCount = collector.GetItemCount();
		if (itemCount != 0)
		{
			canvas.SetDisplayText("Collect " + itemCount + ((itemCount != 1) ? " items" : " item"), true);
			if (Input.GetButtonDown("UseButton"))
			{
				collector.AddCollectedItemsToInventory();
			}
		}
		else
		{
			canvas.SetDisplayText("Empty net");
		}
	}

	public void OnMouseExit()
	{
		canvas.SetDisplayText(false);
	}
}
