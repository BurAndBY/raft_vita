using System;
using System.Collections.Generic;

[Serializable]
public class RGD_Cropplot
{
	public int blockIndex;

	public List<RGD_PlantationSlot> plantationSlots = new List<RGD_PlantationSlot>();

	public RGD_Cropplot(Cropplot plot)
	{
		blockIndex = plot.transform.GetComponent<Block>().blockIndex;
		List<PlantationSlot> slots = plot.GetSlots();
		foreach (PlantationSlot item2 in slots)
		{
			RGD_PlantationSlot item = new RGD_PlantationSlot(item2.plant);
			plantationSlots.Add(item);
		}
	}

	public void RestorePlot(Cropplot plot)
	{
		foreach (RGD_PlantationSlot plantationSlot in plantationSlots)
		{
			Plant plantByIndex = PlantComponent.GetPlantByIndex(plantationSlot.plantIndex);
			if (plantByIndex != null)
			{
				Plant plant = plot.Plant(plantByIndex);
				plant.SetGrowTimer(plantationSlot.growTimer);
				plant.SetYield(plantationSlot.GetYield());
			}
		}
	}
}
