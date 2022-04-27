using System;
using UnityEngine;

[Serializable]
public class RandomDropItem
{
	[Range(0f, 100f)]
	public float spawnChance;

	public ItemIndex index;
}
