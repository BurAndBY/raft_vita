using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Crafting/Recipe", order = 1)]
public class Recipe : ScriptableObject
{
	public CraftingCategory craftingCategory;

	public List<Cost> cost = new List<Cost>();

	public Cost itemToCraft;

	private void OnValidate()
	{
		if (itemToCraft.amount <= 0)
		{
			itemToCraft.amount = 1;
		}
	}
}
