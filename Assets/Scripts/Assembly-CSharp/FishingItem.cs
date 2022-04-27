using System;
using UnityEngine;

[Serializable]
public class FishingItem
{
	public IItem item;

	[Range(0f, 100f)]
	public float spawnChance;
}
