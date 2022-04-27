using System.Collections.Generic;
using UnityEngine;

public class CraftingMenu : MonoBehaviour
{
	public GameObject recipeMenuItem;

	public RectTransform recipeMenuItemParent;

	public SelectedRecipeBox selectedRecipeBox;

	private CraftingCategory selectedCategory;

	private Dictionary<CraftingCategory, List<Recipe>> allRecipes = new Dictionary<CraftingCategory, List<Recipe>>();

	private List<RecipeMenuItem> recipeMenuItems = new List<RecipeMenuItem>();

	private void Awake()
	{
		Recipe[] array = Resources.LoadAll<Recipe>("Recipes");
		foreach (Recipe recipe in array)
		{
			if (allRecipes.ContainsKey(recipe.craftingCategory))
			{
				allRecipes[recipe.craftingCategory].Add(recipe);
				continue;
			}
			List<Recipe> list = new List<Recipe>();
			list.Add(recipe);
			allRecipes.Add(recipe.craftingCategory, list);
		}
		int num = 0;
		foreach (KeyValuePair<CraftingCategory, List<Recipe>> allRecipe in allRecipes)
		{
			if (allRecipe.Value.Count > num)
			{
				num = allRecipe.Value.Count;
			}
		}
		for (int j = 0; j < num; j++)
		{
			GameObject gameObject = Object.Instantiate(recipeMenuItem, recipeMenuItemParent);
			RecipeMenuItem component = gameObject.GetComponent<RecipeMenuItem>();
			if (component != null)
			{
				recipeMenuItems.Add(component);
			}
		}
		SelectCraftingCategory(selectedCategory);
	}

	private void Start()
	{
		selectedRecipeBox.gameObject.SetActive(false);
		CanvasHelper.singleton.SubscribeToMenuClose(OnMenuClose);
	}

	public void SelectCategoryTools()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		SelectCraftingCategory(CraftingCategory.Tools);
	}

	public void SelectCategoryPlaceables()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		SelectCraftingCategory(CraftingCategory.Placeables);
	}

	public void SelectCategoryBasics()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		SelectCraftingCategory(CraftingCategory.Basics);
	}

	public void SelectCategoryDecorations()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		SelectCraftingCategory(CraftingCategory.Decorations);
	}

	public void SelectCraftingCategory(CraftingCategory category)
	{
		if (!allRecipes.ContainsKey(category))
		{
			return;
		}
		List<Recipe> list = allRecipes[category];
		selectedCategory = category;
		foreach (RecipeMenuItem recipeMenuItem in recipeMenuItems)
		{
			recipeMenuItem.gameObject.SetActive(false);
		}
		for (int i = 0; i < list.Count; i++)
		{
			recipeMenuItems[i].gameObject.SetActive(true);
			recipeMenuItems[i].AttachRecipe(list[i]);
		}
		Vector2 sizeDelta = recipeMenuItemParent.sizeDelta;
		sizeDelta.y = 80 * list.Count;
		recipeMenuItemParent.sizeDelta = sizeDelta;
	}

	public void SelectRecipe(Recipe recipe)
	{
		if (!(recipe == null))
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
			Recipe selectedRecipe = selectedRecipeBox.selectedRecipe;
			if (selectedRecipe == null || selectedRecipe != recipe)
			{
				selectedRecipeBox.DisplayRecipe(recipe);
			}
			else if (selectedRecipe == recipe)
			{
				selectedRecipeBox.DisplayRecipe(null);
			}
		}
	}

	public void CraftItem()
	{
		if (selectedRecipeBox.selectedRecipe == null)
		{
			return;
		}
		Recipe selectedRecipe = selectedRecipeBox.selectedRecipe;
		foreach (Cost item in selectedRecipe.cost)
		{
			PlayerInventory.Singleton.RemoveItem(item.item, item.amount);
		}
		PlayerInventory.Singleton.AddItem(selectedRecipe.itemToCraft.item, selectedRecipe.itemToCraft.amount);
	}

	private void OnMenuClose()
	{
		if (GameManager.IsInMenu)
		{
			selectedRecipeBox.DisplayRecipe(null);
		}
	}
}
