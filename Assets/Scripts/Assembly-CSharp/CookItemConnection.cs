using System;
using UnityEngine;

[Serializable]
public class CookItemConnection
{
	[HideInInspector]
	public string name;

	public ItemIndex index;

	public GameObject rawItem;

	public GameObject cookedItem;

	public void SetRawState(bool state)
	{
		rawItem.SetActive(state);
	}

	public void SetCookedState(bool state)
	{
		cookedItem.SetActive(state);
	}
}
