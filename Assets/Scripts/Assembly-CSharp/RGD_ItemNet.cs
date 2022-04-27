using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RGD_ItemNet
{
	public int blockIndex;

	public List<RGD_PickupItem> itemsInNet = new List<RGD_PickupItem>();

	public RGD_ItemNet(Net net)
	{
		blockIndex = net.transform.GetComponent<Block>().blockIndex;
		List<PickupItem> pickedUpItems = net.collector.pickedUpItems;
		for (int i = 0; i < pickedUpItems.Count; i++)
		{
			PickupItem pickupItem = pickedUpItems[i];
			if (!(pickupItem == null))
			{
				RGD_PickupItem item = new RGD_PickupItem(pickupItem);
				itemsInNet.Add(item);
			}
		}
	}

	public void RestoreItemNet(Net net)
	{
		if (net == null || itemsInNet.Count == 0)
		{
			return;
		}
		for (int i = 0; i < itemsInNet.Count; i++)
		{
			RGD_PickupItem rGD_PickupItem = itemsInNet[i];
			if (rGD_PickupItem != null)
			{
				GameObject objectFromPool = PoolManager.singleton.GetObjectFromPool(rGD_PickupItem.index);
				PickupItem component = objectFromPool.GetComponent<PickupItem>();
				if (component != null)
				{
					rGD_PickupItem.RestorePickupItem(component);
				}
			}
		}
	}
}
