using UnityEngine;

public class CookComponent : MonoBehaviour
{
	public ICookable cookable;

	private CookingStand lastLookedAtCookingstand;

	private void Start()
	{
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		CookingStand cookingStand = GetCookingStand();
		if (cookingStand != null)
		{
			lastLookedAtCookingstand = cookingStand;
			if (cookingStand.CanCookItem(cookable.index))
			{
				cookingStand.IsAimedAtWithCookable = true;
				if (Input.GetButtonDown("UseButton") && !cookingStand.IsBusy)
				{
					cookingStand.StartCooking(cookable);
					PlayerInventory.Singleton.RemoveSelectedHotSlotItem(1);
				}
			}
		}
		else if (lastLookedAtCookingstand != null)
		{
			lastLookedAtCookingstand.IsAimedAtWithCookable = false;
			lastLookedAtCookingstand = null;
		}
	}

	public void OnDeSelect()
	{
		if (lastLookedAtCookingstand != null)
		{
			lastLookedAtCookingstand.IsAimedAtWithCookable = false;
		}
	}

	private CookingStand GetCookingStand()
	{
		RaycastHit raycastHit = Helper.HitAtCursor(Player.UseDistance, LayerMasks.MASK_item);
		if (raycastHit.transform != null && raycastHit.transform.tag == "CookingStand")
		{
			return raycastHit.transform.gameObject.GetComponent<CookingStand>();
		}
		return null;
	}
}
