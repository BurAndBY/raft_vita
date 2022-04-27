using System.Collections.Generic;
using UnityEngine;

public class PlantComponent : MonoBehaviour
{
	private static List<Plant> allPlants = new List<Plant>();

	public Plant plantPrefab;

	private Cropplot lastCropplot;

	private void Awake()
	{
		if (!allPlants.Contains(plantPrefab))
		{
			allPlants.Add(plantPrefab);
		}
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		Cropplot cropplot = GetCropplot();
		if (cropplot != null)
		{
			lastCropplot = cropplot;
			if (cropplot.AcceptsItemIndex(plantPrefab.plantIndex))
			{
				cropplot.IsAimedAtWithPlantable = true;
				if (!cropplot.IsFull && Input.GetButtonDown("UseButton"))
				{
					cropplot.Plant(plantPrefab);
					PlayerInventory.Singleton.RemoveSelectedHotSlotItem(1);
					SingletonGeneric<SoundManager>.Singleton.PlaySound("PlantSeed");
					PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_Plant, true);
				}
			}
		}
		else if (lastCropplot != null)
		{
			lastCropplot.IsAimedAtWithPlantable = false;
			lastCropplot = null;
		}
	}

	public static Plant GetPlantByIndex(ItemIndex index)
	{
		foreach (Plant allPlant in allPlants)
		{
			if (allPlant.plantIndex == index)
			{
				return allPlant;
			}
		}
		return null;
	}

	public void OnDeSelect()
	{
		if (lastCropplot != null)
		{
			lastCropplot.IsAimedAtWithPlantable = false;
		}
	}

	private Cropplot GetCropplot()
	{
		RaycastHit raycastHit = Helper.HitAtCursor(Player.UseDistance, LayerMasks.MASK_item);
		if (raycastHit.transform != null && raycastHit.transform.tag == "Cropplot")
		{
			return raycastHit.transform.gameObject.GetComponent<Cropplot>();
		}
		return null;
	}
}
