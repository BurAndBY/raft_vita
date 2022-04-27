using System;
using System.Collections.Generic;

[Serializable]
public class RGD_Inventory
{
	public List<RGD_Slot> slots = new List<RGD_Slot>();

	public int hotslotIndex;

	public int blockIndex;

	public RGD_Inventory(int blockIndex, Inventory inventory)
	{
		this.blockIndex = blockIndex;
		bool flag = inventory is PlayerInventory;
		if (!flag)
		{
			hotslotIndex = -1;
		}
		int slotCount = inventory.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			Slot slot = inventory.GetSlot(i);
			if (!(slot == null))
			{
				RGD_Slot item = new RGD_Slot(slot);
				slots.Add(item);
				if (flag && PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(slot))
				{
					hotslotIndex = i;
				}
			}
		}
	}

	public void RestoreInventory(Inventory inventory)
	{
		if (inventory == null)
		{
			return;
		}
		for (int i = 0; i < slots.Count; i++)
		{
			if (i < inventory.GetSlotCount())
			{
				Slot slot = inventory.GetSlot(i);
				RGD_Slot rGD_Slot = slots[i];
				IItem itemOfType = Inventory.GetItemOfType(rGD_Slot.index);
				slot.SetItem(itemOfType, rGD_Slot.amount);
			}
		}
	}
}
