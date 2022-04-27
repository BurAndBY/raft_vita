using System.Collections.Generic;
using UnityEngine;

public class Cropplot : MonoBehaviour
{
	public bool IsAimedAtWithPlantable;

	[Space(10f)]
	public List<ItemIndex> acceptableItems = new List<ItemIndex>();

	[SerializeField]
	[Space(10f)]
	private List<PlantationSlot> plantationSlots = new List<PlantationSlot>();

	private CanvasHelper canvas;

	public bool IsFull
	{
		get
		{
			return GetBusySlotCount() == plantationSlots.Count;
		}
	}

	private void Start()
	{
		canvas = CanvasHelper.singleton;
	}

	private void Update()
	{
	}

	private void OnMouseOver()
	{
		if (IsAimedAtWithPlantable && !IsFull)
		{
			canvas.SetDisplayText("Plant seed", true);
		}
		else
		{
			canvas.SetDisplayText(false);
		}
	}

	private void OnMouseExit()
	{
		canvas.SetDisplayText(false);
	}

	public Plant Plant(Plant plantPrefab)
	{
		if (IsFull)
		{
			return null;
		}
		PlantationSlot emptyPlantationSlot = GetEmptyPlantationSlot();
		if (emptyPlantationSlot == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(plantPrefab.gameObject, emptyPlantationSlot.transform.position, plantPrefab.transform.rotation, base.transform);
		Plant plant = (emptyPlantationSlot.plant = gameObject.GetComponent<Plant>());
		if (plant != null)
		{
			plant.slotPlantIsOn = emptyPlantationSlot;
			plant.SubscribeToOnPlantRemove(PlantRemoved);
			plant.SetGrowTimer(0f);
		}
		return plant;
	}

	public void PlantRemoved(PlantationSlot plantSlot)
	{
		foreach (PlantationSlot plantationSlot in plantationSlots)
		{
			if (plantationSlot == plantSlot)
			{
				plantationSlot.busy = false;
				break;
			}
		}
	}

	public bool AcceptsItemIndex(ItemIndex index)
	{
		foreach (ItemIndex acceptableItem in acceptableItems)
		{
			if (acceptableItem == index)
			{
				return true;
			}
		}
		return false;
	}

	public List<PlantationSlot> GetSlots()
	{
		return plantationSlots;
	}

	private PlantationSlot GetEmptyPlantationSlot()
	{
		for (int i = 0; i < plantationSlots.Count; i++)
		{
			if (!plantationSlots[i].busy)
			{
				plantationSlots[i].busy = true;
				return plantationSlots[i];
			}
		}
		return null;
	}

	private int GetBusySlotCount()
	{
		int num = 0;
		for (int i = 0; i < plantationSlots.Count; i++)
		{
			if (plantationSlots[i].busy)
			{
				num++;
			}
		}
		return num;
	}
}
