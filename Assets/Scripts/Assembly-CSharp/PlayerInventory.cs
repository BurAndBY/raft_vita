using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : Inventory
{
	public static PlayerInventory Singleton;

	[Header("Hotbar settings")]
	public Hotbar hotbar;

	public RectTransform hotslotBackground;

	public Transform hotslotParent;

	public int hotslotCount;

	public float hotbarDistanceFromBottom;

	[Header("Pickup components")]
	public InventoryPickup inventoryPickup;

	[Header("Item description")]
	public Text itemNameText;

	public Text itemDescriptionText;

	public Image itemImage;

	protected override void Awake()
	{
		if (Singleton == null)
		{
			Singleton = this;
		}
		else if (Singleton != this)
		{
			Object.Destroy(base.gameObject);
		}
		base.Awake();
		Text text = itemNameText;
		string empty = string.Empty;
		itemDescriptionText.text = empty;
		text.text = empty;
		itemImage.enabled = false;
	}

	protected override void CreateSlots()
	{
		base.CreateSlots();
		float num = (float)(hotslotCount * slotSize) + paddingLeft * (float)hotslotCount + paddingLeft * 2f;
		float num2 = (float)slotSize + paddingTop * 2f;
		hotslotBackground.sizeDelta = new Vector2(num, num2);
		hotslotBackground.localPosition = new Vector3(0f, num2 / 2f - hotbarDistanceFromBottom);
		List<Slot> list = new List<Slot>();
		for (int i = 0; i < hotslotCount; i++)
		{
			GameObject gameObject = Object.Instantiate(slotPrefab, hotslotParent);
			RectTransform component = gameObject.GetComponent<RectTransform>();
			gameObject.name = "HotSlot: " + i;
			gameObject.transform.localScale = slotPrefab.transform.localScale;
			float num3 = (0f - num) / 2f;
			float y = hotslotParent.position.y + (float)slotSize + hotbarDistanceFromBottom + paddingTop;
			component.localPosition = new Vector3(num3 + (float)(i * slotSize) + paddingLeft * (float)i + paddingLeft, y);
			component.sizeDelta = new Vector2(slotSize, slotSize);
			Slot component2 = gameObject.GetComponent<Slot>();
			component2.slotNumberTextComponent.gameObject.SetActive(true);
			component2.slotNumberTextComponent.text = (i + 1).ToString();
			list.Add(component2);
		}
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			allSlots.Insert(0, list[num4]);
		}
	}

	public override AddResult AddItem(IItem item, int amount)
	{
		AddResult addResult = base.AddItem(item, amount);
		if (addResult.succeded)
		{
			inventoryPickup.ShowItem(item, amount);
		}
		return addResult;
	}

	public override AddResult AddItem(IItem item, Slot slot, int amount)
	{
		AddResult addResult = base.AddItem(item, slot, amount);
		if (addResult.succeded)
		{
			inventoryPickup.ShowItem(item, amount);
		}
		return addResult;
	}

	public void SetItemDescription(Slot slot)
	{
		if (slot != null && !slot.IsEmpty())
		{
			IItem iItem = slot.iItem;
			itemNameText.text = iItem.displayName;
			itemDescriptionText.text = iItem.displayDescription;
			itemImage.enabled = true;
			itemImage.sprite = iItem.sprite;
		}
		else if (slot == null)
		{
			Text text = itemNameText;
			string empty = string.Empty;
			itemDescriptionText.text = empty;
			text.text = empty;
			itemImage.enabled = false;
		}
	}

	public RemoveResult RemoveSelectedHotSlotItem(int amount)
	{
		RemoveResult removeResult = new RemoveResult();
		Slot slot = GetSlot(hotbar.GetSelectedSlotIndex());
		if (slot == null || slot.IsEmpty())
		{
			removeResult.succeded = false;
			removeResult.slot = slot;
			removeResult.fullyRemoved = false;
			return removeResult;
		}
		removeResult.slot = slot;
		if (slot.amount >= amount)
		{
			slot.RemoveItem(amount);
			removeResult.succeded = true;
		}
		else
		{
			slot.RemoveItem(slot.amount);
			removeResult.succeded = false;
		}
		removeResult.fullyRemoved = slot.IsEmpty();
		hotbar.ReselectCurrentSlot();
		return removeResult;
	}

	public void DropItem(Slot slot)
	{
		if (!(slot == null) && !slot.IsEmpty())
		{
			Helper.DropItem(slot.iItem, slot.amount, GameManager.singleton.player.transform.position);
			slot.SetItem(null, 0);
			if (hotbar.IsSelectedHotSlot(slot))
			{
				hotbar.ReselectCurrentSlot();
			}
		}
	}

	public void DropItem(IItem item, int amount)
	{
		Helper.DropItem(item, amount, GameManager.singleton.player.transform.position);
	}
}
