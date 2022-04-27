using System;

[Serializable]
public class RGD_Yield
{
	public ItemIndex itemIndex;

	public int amount;

	public RGD_Yield(Cost cost)
	{
		if (cost != null)
		{
			itemIndex = cost.item.index;
			amount = cost.amount;
		}
	}
}
