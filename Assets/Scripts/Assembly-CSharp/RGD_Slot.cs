using System;

[Serializable]
public class RGD_Slot
{
	public ItemIndex index;

	public int amount;

	public RGD_Slot(Slot slot)
	{
		if (slot.IsEmpty())
		{
			index = ItemIndex.None;
			amount = 0;
		}
		else
		{
			index = slot.iItem.index;
			amount = slot.amount;
		}
	}
}
