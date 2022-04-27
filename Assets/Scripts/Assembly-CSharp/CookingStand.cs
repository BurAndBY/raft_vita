using System.Collections.Generic;
using UnityEngine;

public class CookingStand : MonoBehaviour
{
	public bool IsAimedAtWithCookable;

	public GameObject particleController;

	public List<CookItemConnection> itemConnections = new List<CookItemConnection>();

	public AudioSource source;

	private bool busy;

	private float cookingTimer;

	private ICookable currentItem;

	private ICookable completeCookable;

	public bool IsBusy
	{
		get
		{
			return busy;
		}
	}

	public bool IsComplete
	{
		get
		{
			return completeCookable != null;
		}
	}

	private void Awake()
	{
		source.Stop();
		busy = false;
		completeCookable = null;
		DisableItems();
		particleController.SetActive(false);
	}

	private void Update()
	{
		if (busy && currentItem != null && !IsComplete)
		{
			cookingTimer += Time.deltaTime;
			if (cookingTimer >= currentItem.cooktime)
			{
				FinishCooking(currentItem);
			}
		}
	}

	private void OnValidate()
	{
		foreach (CookItemConnection itemConnection in itemConnections)
		{
			itemConnection.name = itemConnection.index.ToString();
		}
	}

	private void OnMouseOver()
	{
		if (GameManager.IsInMenu)
		{
			CanvasHelper.singleton.SetDisplayText(false);
		}
		else if (IsAimedAtWithCookable)
		{
			if (!busy)
			{
				CanvasHelper.singleton.SetDisplayText("Cook", true);
			}
		}
		else if (IsComplete && Player.IsWithingDistance(base.transform.position))
		{
			CanvasHelper.singleton.SetDisplayText("Pickup " + completeCookable.result.displayName, true);
		}
		else
		{
			CanvasHelper.singleton.SetDisplayText(false);
		}
	}

	private void OnMouseExit()
	{
		CanvasHelper.singleton.SetDisplayText(false);
	}

	public void StartCooking(ICookable cookable)
	{
		if (!(cookable == null) && !IsBusy && CanCookItem(cookable.index))
		{
			busy = true;
			currentItem = cookable;
			cookingTimer = 0f;
			DisableItems();
			EnableRawItem(cookable.index);
			source.Play();
			if (particleController != null)
			{
				particleController.SetActive(true);
			}
		}
	}

	public IItem RetrieveItem()
	{
		if (!IsComplete)
		{
			return null;
		}
		IItem result = completeCookable.result;
		completeCookable = null;
		currentItem = null;
		cookingTimer = 0f;
		busy = false;
		DisableItems();
		return result;
	}

	public bool CanCookItem(ItemIndex index)
	{
		foreach (CookItemConnection itemConnection in itemConnections)
		{
			if (itemConnection.index == index)
			{
				return true;
			}
		}
		return false;
	}

	public void SetCookingTimer(float value)
	{
		cookingTimer = value;
	}

	public ItemIndex GetCurrentItem()
	{
		if (currentItem != null)
		{
			return currentItem.index;
		}
		return ItemIndex.None;
	}

	public ItemIndex GetCompleteItem()
	{
		if (completeCookable != null)
		{
			return completeCookable.index;
		}
		return ItemIndex.None;
	}

	public float GetCookingTimer()
	{
		return cookingTimer;
	}

	private void EnableRawItem(ItemIndex index)
	{
		foreach (CookItemConnection itemConnection in itemConnections)
		{
			if (itemConnection.index == index)
			{
				itemConnection.SetRawState(true);
				itemConnection.SetCookedState(false);
				break;
			}
		}
	}

	private void EnableCookedItem(ItemIndex index)
	{
		foreach (CookItemConnection itemConnection in itemConnections)
		{
			if (itemConnection.index == index)
			{
				itemConnection.SetRawState(false);
				itemConnection.SetCookedState(true);
				break;
			}
		}
	}

	private void DisableItems()
	{
		foreach (CookItemConnection itemConnection in itemConnections)
		{
			itemConnection.SetRawState(false);
			itemConnection.SetCookedState(false);
		}
	}

	private void FinishCooking(ICookable cookable)
	{
		EnableCookedItem(cookable.index);
		completeCookable = cookable;
		if (particleController != null)
		{
			particleController.SetActive(false);
		}
		source.Stop();
	}
}
