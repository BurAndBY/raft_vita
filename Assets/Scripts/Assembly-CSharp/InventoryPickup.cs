using System.Collections.Generic;
using UnityEngine;

public class InventoryPickup : MonoBehaviour
{
	public InventoryPickupMenuItem menuItemPrefab;

	[SerializeField]
	private int numberOfItems;

	private List<InventoryPickupMenuItem> items = new List<InventoryPickupMenuItem>();

	private void Start()
	{
		CreateItems();
	}

	public void ShowItem(IItem item, int amount)
	{
		InventoryPickupMenuItem firstItem = GetFirstItem();
		if (firstItem == null || item == null)
		{
			return;
		}
		foreach (InventoryPickupMenuItem item2 in items)
		{
			if (item2.gameObject.activeInHierarchy)
			{
				item2.index++;
			}
		}
		firstItem.rect.localPosition = new Vector3(0f, -2f * firstItem.rect.sizeDelta.y, 0f);
		firstItem.gameObject.SetActive(true);
		firstItem.SetItem(item, amount);
		firstItem.index = 0;
	}

	private InventoryPickupMenuItem GetFirstItem()
	{
		for (int i = 0; i < items.Count; i++)
		{
			InventoryPickupMenuItem inventoryPickupMenuItem = items[i];
			if (!inventoryPickupMenuItem.gameObject.activeInHierarchy)
			{
				return inventoryPickupMenuItem;
			}
		}
		if (items.Count > 0)
		{
			InventoryPickupMenuItem inventoryPickupMenuItem2 = items[0];
			{
				foreach (InventoryPickupMenuItem item in items)
				{
					if (item.index > inventoryPickupMenuItem2.index)
					{
						inventoryPickupMenuItem2 = item;
					}
				}
				return inventoryPickupMenuItem2;
			}
		}
		return null;
	}

	private void CreateItems()
	{
		for (int i = 0; i < numberOfItems + numberOfItems / 2; i++)
		{
			GameObject gameObject = Object.Instantiate(menuItemPrefab.gameObject, base.transform);
			gameObject.transform.localScale = menuItemPrefab.transform.localScale;
			InventoryPickupMenuItem component = gameObject.GetComponent<InventoryPickupMenuItem>();
			component.rect.localPosition = new Vector3(0f, -2f * component.rect.sizeDelta.y, 0f);
			items.Add(component);
			gameObject.SetActive(false);
		}
	}
}
