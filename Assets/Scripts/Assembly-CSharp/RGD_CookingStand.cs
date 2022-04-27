using System;

[Serializable]
public class RGD_CookingStand
{
	public int blockIndex;

	public bool busy;

	public bool isComplete;

	public ItemIndex currentItemIndex;

	public ItemIndex finishedItemIndex;

	public float cookingTimer;

	public RGD_CookingStand(CookingStand stand)
	{
		blockIndex = stand.transform.GetComponent<Block>().blockIndex;
		busy = stand.IsBusy;
		isComplete = stand.IsComplete;
		currentItemIndex = stand.GetCurrentItem();
		finishedItemIndex = stand.GetCompleteItem();
		cookingTimer = stand.GetCookingTimer();
	}

	public void RestoreStand(CookingStand stand)
	{
		if (busy || isComplete)
		{
			if (busy && !isComplete)
			{
				stand.StartCooking(Inventory.GetItemOfType(currentItemIndex) as ICookable);
				stand.SetCookingTimer(cookingTimer);
			}
			else if (isComplete)
			{
				stand.StartCooking(Inventory.GetItemOfType(currentItemIndex) as ICookable);
				stand.SetCookingTimer(float.MaxValue);
			}
		}
	}
}
