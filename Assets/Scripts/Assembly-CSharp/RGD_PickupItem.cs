using System;

[Serializable]
public class RGD_PickupItem
{
	public ItemIndex index;

	public int amount;

	public bool canBePickedUp;

	public bool isDropped;

	public SerializableTransform serializableTransform;

	public RGD_PickupItem(PickupItem pickupItem)
	{
		if (!(pickupItem == null))
		{
			serializableTransform = new SerializableTransform(pickupItem.transform);
			index = pickupItem.iItem.index;
			amount = pickupItem.amount;
			canBePickedUp = pickupItem.canBePickedUp;
			isDropped = pickupItem.isDropped;
		}
	}

	public void RestorePickupItem(PickupItem pickupItem)
	{
		serializableTransform.SetTransform(pickupItem.transform);
		pickupItem.amount = amount;
		pickupItem.canBePickedUp = canBePickedUp;
		pickupItem.isDropped = isDropped;
	}
}
