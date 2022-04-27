using UnityEngine;
using UnityEngine.UI;

public class RecipeMenuItem : MonoBehaviour
{
	public Image recipeImage;

	private Recipe recipe;

	private CraftingMenu menu;

	private void Start()
	{
		menu = Object.FindObjectOfType<CraftingMenu>();
	}

	private void Update()
	{
	}

	public void AttachRecipe(Recipe recipe)
	{
		this.recipe = recipe;
		recipeImage.sprite = recipe.itemToCraft.item.sprite;
	}

	public void OnButtonClick()
	{
		menu.SelectRecipe(recipe);
	}
}
