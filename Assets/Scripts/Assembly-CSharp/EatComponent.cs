using UnityEngine;

public class EatComponent : MonoBehaviour
{
	public IEdible eatable;

	public Cost itemAfterUse;

	private bool canEat;

	private void Start()
	{
		canEat = true;
	}

	private void Update()
	{
	}

	public void OnUse()
	{
		if (!GameManager.IsInMenu)
		{
			Eat();
		}
	}

	private void Eat()
	{
		if (canEat)
		{
			canEat = false;
			Invoke("ResetCanEat", eatable.useCooldown);
			GameManager.singleton.playerStats.AddHunger(eatable.hungerYield);
			GameManager.singleton.playerStats.AddThirst(eatable.thirstYield);
			GameManager.singleton.playerStats.AddHealth(eatable.healthYield);
			if (eatable.hungerYield < eatable.thirstYield)
			{
				SingletonGeneric<SoundManager>.Singleton.PlaySound("Drink");
			}
			else
			{
				SingletonGeneric<SoundManager>.Singleton.PlaySound("Chew");
			}
			RemoveResult removeResult = PlayerInventory.Singleton.RemoveSelectedHotSlotItem(1);
			if (itemAfterUse.item != null)
			{
				PlayerInventory.Singleton.AddItem(itemAfterUse.item, itemAfterUse.amount);
			}
		}
	}

	private void ResetCanEat()
	{
		canEat = true;
	}
}
