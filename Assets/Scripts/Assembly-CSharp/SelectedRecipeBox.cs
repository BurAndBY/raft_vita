using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedRecipeBox : MonoBehaviour
{
	public Text recipeLabel;

	public Text recipeDescription;

	public Image recipeImage;

	public Button craftButton;

	public List<ItemCostBox> costBoxes = new List<ItemCostBox>();

	[HideInInspector]
	public Recipe selectedRecipe;

	private void Start()
	{
	}

	private void Update()
	{
		if (selectedRecipe == null)
		{
			return;
		}
		bool interactable = true;
		foreach (Cost item in selectedRecipe.cost)
		{
			int itemCount = PlayerInventory.Singleton.GetItemCount(item.item.index);
			if (itemCount < item.amount)
			{
				interactable = false;
				break;
			}
		}
		craftButton.interactable = interactable;
	}

	public void DisplayRecipe(Recipe recipe)
	{
		selectedRecipe = recipe;
		if (selectedRecipe == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		base.gameObject.SetActive(true);
		recipeLabel.text = recipe.itemToCraft.item.displayName;
		recipeDescription.text = recipe.itemToCraft.item.displayDescription;
		recipeImage.sprite = recipe.itemToCraft.item.sprite;
		foreach (ItemCostBox costBox in costBoxes)
		{
			costBox.gameObject.SetActive(false);
		}
		for (int i = 0; i < recipe.cost.Count; i++)
		{
			costBoxes[i].gameObject.SetActive(true);
			costBoxes[i].SetCost(recipe.cost[i]);
		}
	}
}
