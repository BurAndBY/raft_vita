using System;
using UnityEngine;

[Serializable]
public class ItemConnection
{
	[HideInInspector]
	public string name = "ItemConnection";

	public ItemIndex index;

	public GameObject obj;
}
