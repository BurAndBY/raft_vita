using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	public static PoolManager singleton;

	public List<PoolItem> poolableItem = new List<PoolItem>();

	private Dictionary<ItemIndex, List<GameObject>> itemsInPool = new Dictionary<ItemIndex, List<GameObject>>();

	private void Start()
	{
		singleton = this;
		foreach (PoolItem item in poolableItem)
		{
			AddItemToPool(item.index, item.poolCount);
		}
	}

	public GameObject GetObjectFromPool(ItemIndex index)
	{
		if (!itemsInPool.ContainsKey(index))
		{
			return null;
		}
		GameObject gameObject = null;
		List<GameObject> list = itemsInPool[index];
		foreach (GameObject item in list)
		{
			if (item.activeInHierarchy)
			{
				continue;
			}
			gameObject = item;
			break;
		}
		if (gameObject == null)
		{
			AddItemToPool(index, 1);
			gameObject = GetObjectFromPool(index);
		}
		if (gameObject != null)
		{
			gameObject.SetActive(true);
		}
		return gameObject;
	}

	public void ReturnToPool(ItemIndex index, GameObject obj)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.name == index.ToString())
			{
				obj.transform.SetParent(child);
				break;
			}
		}
		obj.SetActive(false);
	}

	private void AddItemToPool(ItemIndex index, int amount)
	{
		bool flag = itemsInPool.ContainsKey(index);
		List<GameObject> list = null;
		if (!flag)
		{
			itemsInPool.Add(index, new List<GameObject>());
		}
		list = itemsInPool[index];
		IItem itemOfType = Inventory.GetItemOfType(index);
		if (!(itemOfType == null) && !(itemOfType.prefab == null))
		{
			GameObject gameObject = null;
			if (!flag)
			{
				gameObject = new GameObject(itemOfType.index.ToString());
				gameObject.transform.SetParent(base.transform);
			}
			else
			{
				gameObject = base.transform.Find(itemOfType.index.ToString()).gameObject;
			}
			int count = list.Count;
			for (int i = 0; i < amount; i++)
			{
				GameObject gameObject2 = Object.Instantiate(itemOfType.prefab, gameObject.transform);
				gameObject2.transform.name = itemOfType.index.ToString() + ": " + (count + i);
				gameObject2.SetActive(false);
				list.Add(gameObject2);
			}
		}
	}
}
