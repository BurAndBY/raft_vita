using UnityEngine;

public class Pickup : MonoBehaviour
{
	public LayerMask pickupMask;

	private GameObject prevItem;

	private void Update()
	{
		RaycastHit raycastHit = Helper.HitAtCursor(Player.UseDistance, pickupMask);
		PickupItem pickupItem = null;
		if (raycastHit.transform != null)
		{
			pickupItem = raycastHit.transform.GetComponent<PickupItem>();
			if (pickupItem != null && pickupItem.canBePickedUp)
			{
				CanvasHelper.singleton.SetDisplayText("Pickup\n'" + pickupItem.iItem.displayName + "'", true);
			}
			if (prevItem != raycastHit.transform.gameObject)
			{
				prevItem = raycastHit.transform.gameObject;
			}
		}
		else if (prevItem != null)
		{
			prevItem = null;
			CanvasHelper.singleton.SetDisplayText(false);
		}
		if (Input.GetButtonDown("UseButton"))
		{
			if (pickupItem != null)
			{
				PickupItem(pickupItem);
			}
			else
			{
				PickupWithTag(raycastHit.transform);
			}
		}
	}

	private void PickupItem(PickupItem pickup)
	{
		if (!pickup.canBePickedUp || !Player.IsWithingDistance(pickup.transform.position))
		{
			return;
		}
		PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_Grab);
		if (pickup.dropper != null)
		{
			for (int i = 0; i < pickup.amount; i++)
			{
				IItem item = pickup.GetItem();
				PlayerInventory.Singleton.AddItem(item.index, 1);
			}
		}
		else
		{
			PlayerInventory.Singleton.AddItem(pickup.iItem, pickup.amount);
		}
		if (pickup.isDropped)
		{
			PoolManager.singleton.ReturnToPool(ItemIndex.DropItem, pickup.transform.gameObject);
		}
		else
		{
			PoolManager.singleton.ReturnToPool(pickup.iItem.index, pickup.transform.gameObject);
		}
	}

	private void PickupWithTag(Transform obj)
	{
		if (obj == null || !Player.IsWithingDistance(obj.position))
		{
			return;
		}
		string text = obj.tag;
		if (text == null || !(text == "CookingStand"))
		{
			return;
		}
		CookingStand component = obj.GetComponent<CookingStand>();
		if (component != null)
		{
			IItem item = component.RetrieveItem();
			if (item != null && item.index != ItemIndex.None)
			{
				PlayerInventory.Singleton.AddItem(item, 1);
				PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_Grab);
			}
		}
	}
}
