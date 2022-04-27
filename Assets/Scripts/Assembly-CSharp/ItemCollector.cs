using System.Collections.Generic;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
	public ParticleSystem pickupParticles;

	public List<PickupItem> pickedUpItems = new List<PickupItem>();

	private void Awake()
	{
		if (pickupParticles != null)
		{
			pickupParticles.transform.name = "Hook pickup particles";
			pickupParticles.transform.SetParent(null);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.transform.gameObject.layer) & (int)LayerMasks.MASK_item) == 0)
		{
			return;
		}
		PickupItem component = other.transform.GetComponent<PickupItem>();
		if (component != null)
		{
			Collider component2 = component.GetComponent<Collider>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
			other.transform.SetParent(base.transform);
			pickedUpItems.Add(component);
			SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("ItemCollector", base.transform.position, true);
			if (pickupParticles != null)
			{
				pickupParticles.transform.position = other.transform.position;
				pickupParticles.Play();
			}
		}
	}

	public int GetItemCount()
	{
		return pickedUpItems.Count;
	}

	public void AddCollectedItemsToInventory()
	{
		if (GetItemCount() == 0)
		{
			return;
		}
		foreach (PickupItem pickedUpItem in pickedUpItems)
		{
			for (int i = 0; i < pickedUpItem.amount; i++)
			{
				IItem item = pickedUpItem.GetItem();
				PlayerInventory.Singleton.AddItem(item.index, 1);
			}
		}
		ClearCollectedItems();
	}

	private void ClearCollectedItems()
	{
		foreach (PickupItem pickedUpItem in pickedUpItems)
		{
			Collider component = pickedUpItem.GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = true;
			}
			PoolManager.singleton.ReturnToPool(pickedUpItem.iItem.index, pickedUpItem.gameObject);
		}
		pickedUpItems.Clear();
	}
}
