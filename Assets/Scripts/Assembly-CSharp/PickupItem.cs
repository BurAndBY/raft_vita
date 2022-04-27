using UnityEngine;

public class PickupItem : MonoBehaviour
{
	public IItem iItem;

	public int amount;

	public bool canBePickedUp = true;

	[Header("Use random dropper?")]
	public RandomDropper dropper;

	[Header("Drop settings")]
	public bool isDropped;

	private void OnMouseExit()
	{
		CanvasHelper.singleton.SetDisplayText(false);
	}

	public IItem GetItem()
	{
		if (dropper == null)
		{
			return iItem;
		}
		return Inventory.GetItemOfType(dropper.GetItemToSpawn());
	}
}
