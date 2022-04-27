using System;
using System.Collections.Generic;

[Serializable]
public class RGD_PlantationSlot
{
	public ItemIndex plantIndex;

	public float growTimer;

	public List<RGD_Yield> yields = new List<RGD_Yield>();

	public RGD_PlantationSlot(Plant plant)
	{
		if (plant != null)
		{
			plantIndex = plant.plantIndex;
			growTimer = plant.GetGrowTimer();
			{
				foreach (Cost yieldItem in plant.yieldItems)
				{
					RGD_Yield item = new RGD_Yield(yieldItem);
					yields.Add(item);
				}
				return;
			}
		}
		plantIndex = ItemIndex.None;
	}

	public List<Cost> GetYield()
	{
		List<Cost> list = new List<Cost>();
		foreach (RGD_Yield yield in yields)
		{
			Cost cost = new Cost();
			cost.amount = yield.amount;
			cost.item = Inventory.GetItemOfType(yield.itemIndex);
			list.Add(cost);
		}
		return list;
	}
}
