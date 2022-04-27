using System;
using UnityEngine;

[Serializable]
public class PlantationSlot
{
	public Transform transform;

	[HideInInspector]
	public bool busy;

	public Plant plant;
}
