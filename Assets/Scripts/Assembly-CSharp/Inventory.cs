using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
	[Header("Inventory settings")]
	public int numberOfSlots;

	public int rows;

	public int slotSize;

	public float paddingLeft;

	public float paddingTop;

	[Header("Prefabs")]
	public GameObject slotPrefab;

	public PickupItem dropBarrel;

	[Header("Components")]
	public RectTransform hoverTransform;

	public RectTransform darkenedTransform;

	public Image draggingImage;

	public Transform slotParent;

	[HideInInspector]
	public List<Slot> allSlots;

	private RectTransform invRectTransform;

	private static List<IItem> allAvailableItems = new List<IItem>();

	public static Transform Parent;

	private static Transform parent;

	private static Slot fromSlot;

	private static Slot toSlot;

	private static Slot hoverSlot;

	private static bool dragging;

	private static bool canQuickEquip = true;

	protected virtual void Awake()
	{
		LoadAllIItems();
		invRectTransform = GetComponent<RectTransform>();
		hoverTransform.sizeDelta = new Vector2(slotSize, slotSize);
		darkenedTransform.sizeDelta = new Vector2(slotSize, slotSize);
		draggingImage.rectTransform.sizeDelta = new Vector2((float)slotSize * 0.75f, (float)slotSize * 0.75f);
		CreateSlots();
	}

	protected virtual void Start()
	{
		Hide();
		CanvasHelper.singleton.SubscribeToMenuClose(OnMenuClose);
	}

	protected virtual void Update()
	{
		dragging = fromSlot != null && !fromSlot.IsEmpty();
		hoverTransform.gameObject.SetActive(hoverSlot != null);
		if (hoverTransform.gameObject.activeInHierarchy)
		{
			hoverTransform.position = hoverSlot.rectTransform.position;
		}
		darkenedTransform.gameObject.SetActive(dragging);
		if (dragging)
		{
			if (hoverSlot == fromSlot)
			{
				hoverTransform.gameObject.SetActive(false);
			}
			darkenedTransform.position = fromSlot.rectTransform.position;
		}
		draggingImage.gameObject.SetActive(dragging);
		if (dragging)
		{
			draggingImage.sprite = fromSlot.iItem.sprite;
			draggingImage.rectTransform.position = Input.mousePosition - new Vector3(draggingImage.rectTransform.sizeDelta.x / 4f, (0f - draggingImage.rectTransform.sizeDelta.y) / 4f);
		}
		if (!canQuickEquip || dragging || !(hoverSlot != null))
		{
			return;
		}
		int num = 49;
		for (int i = num; i < num + 8; i++)
		{
			KeyCode key = (KeyCode)(num + (i - num));
			if (Input.GetKeyDown(key))
			{
				int index = i - num;
				Slot slot = PlayerInventory.Singleton.GetSlot(index);
				fromSlot = hoverSlot;
				toSlot = slot;
				MoveItem(slot);
				canQuickEquip = false;
				Invoke("ResetQuickEquip", 0.05f);
				break;
			}
		}
	}

	public virtual AddResult AddItem(IItem item, int amount)
	{
		AddResult addResult = new AddResult();
		if (item.IsStackable())
		{
			int num = amount;
			for (int i = 0; i < amount; i++)
			{
				Slot slot = FindFirstStackableSlot(item.index);
				if (slot != null && !slot.StackIsFull())
				{
					slot.AddItem(item);
					num--;
					addResult.succeded = true;
					addResult.slot = slot;
					continue;
				}
				slot = FindFirstEmptySlot();
				if (slot != null)
				{
					slot.SetItem(item, num);
					addResult.succeded = true;
					addResult.slot = slot;
					break;
				}
				if (this is PlayerInventory)
				{
					PlayerInventory.Singleton.DropItem(item, amount);
				}
				addResult.slot = null;
				addResult.succeded = false;
				return addResult;
			}
		}
		else
		{
			Slot slot2 = FindFirstEmptySlot();
			if (!(slot2 != null))
			{
				if (this is PlayerInventory)
				{
					PlayerInventory.Singleton.DropItem(item, amount);
				}
				addResult.slot = null;
				addResult.succeded = false;
				return addResult;
			}
			slot2.SetItem(item, amount);
			addResult.succeded = true;
			addResult.slot = slot2;
		}
		if (PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(addResult.slot))
		{
			PlayerInventory.Singleton.hotbar.ReselectCurrentSlot();
		}
		if (addResult.succeded)
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("UIMoveItem");
		}
		return addResult;
	}

	public AddResult AddItem(ItemIndex index, int amount)
	{
		IItem itemOfType = GetItemOfType(index);
		if (itemOfType == null)
		{
			return new AddResult(false, null);
		}
		return AddItem(itemOfType, amount);
	}

	public virtual AddResult AddItem(IItem item, Slot slot, int amount)
	{
		AddResult addResult = new AddResult();
		if (slot == null)
		{
			return AddItem(item, amount);
		}
		if (item.IsStackable())
		{
			for (int i = 0; i < amount; i++)
			{
				if (!slot.StackIsFull())
				{
					slot.AddItem(item);
					addResult.slot = slot;
					addResult.succeded = true;
					continue;
				}
				slot = FindFirstEmptySlot();
				if (slot != null)
				{
					slot.SetItem(item, amount);
					addResult.succeded = true;
					addResult.slot = slot;
					break;
				}
				if (this is PlayerInventory)
				{
					PlayerInventory.Singleton.DropItem(item, amount);
				}
				addResult.slot = null;
				addResult.succeded = false;
				return addResult;
			}
		}
		else
		{
			slot.SetItem(item, amount);
			addResult.succeded = true;
			addResult.slot = slot;
		}
		if (PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(addResult.slot))
		{
			PlayerInventory.Singleton.hotbar.ReselectCurrentSlot();
		}
		if (addResult.succeded)
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("UIMoveItem");
		}
		return addResult;
	}

	public AddResult AddItem(ItemIndex index, Slot slot, int amount)
	{
		IItem itemOfType = GetItemOfType(index);
		if (itemOfType == null)
		{
			return new AddResult(false, null);
		}
		return AddItem(itemOfType, slot, amount);
	}

	public RemoveResult RemoveItem(IItem item, int amount)
	{
		RemoveResult removeResult = new RemoveResult(false, false, null);
		int num = 0;
		foreach (Slot allSlot in allSlots)
		{
			if (allSlot.IsEmpty())
			{
				continue;
			}
			if (allSlot.iItem.index == item.index)
			{
				removeResult.succeded = true;
				removeResult.slot = allSlot;
				if (allSlot.amount >= amount)
				{
					allSlot.RemoveItem(amount);
					num = amount;
				}
				else
				{
					allSlot.RemoveItem(allSlot.amount);
					num += allSlot.amount;
				}
			}
			if (num != amount)
			{
				continue;
			}
			removeResult.fullyRemoved = true;
			break;
		}
		if (PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(removeResult.slot))
		{
			PlayerInventory.Singleton.hotbar.ReselectCurrentSlot();
		}
		return removeResult;
	}

	public RemoveResult RemoveItem(ItemIndex index, int amount)
	{
		IItem itemOfType = GetItemOfType(index);
		if (itemOfType != null)
		{
			return RemoveItem(itemOfType, amount);
		}
		return new RemoveResult(false, false, null);
	}

	public void HoverEnter(Slot slot)
	{
		hoverSlot = slot;
		if (fromSlot != null && fromSlot != toSlot)
		{
			toSlot = slot;
		}
		PlayerInventory.Singleton.SetItemDescription(hoverSlot);
	}

	public void HoverExit(Slot slot)
	{
		if (fromSlot != null && toSlot == slot)
		{
			toSlot = null;
		}
		if (toSlot == null)
		{
			hoverSlot = null;
		}
		PlayerInventory.Singleton.SetItemDescription(null);
	}

	public void MoveItem(Slot slot)
	{
		if (fromSlot == null)
		{
			fromSlot = slot;
		}
		else if (toSlot == null)
		{
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				PlayerInventory.Singleton.DropItem(fromSlot);
			}
			fromSlot = (toSlot = null);
		}
		else if (fromSlot == toSlot)
		{
			fromSlot = (toSlot = null);
		}
		else
		{
			if (!(fromSlot != null) || !(toSlot != null))
			{
				return;
			}
			if (fromSlot.IsEmpty() || (fromSlot.IsEmpty() && toSlot.IsEmpty()))
			{
				fromSlot = (toSlot = null);
				return;
			}
			if (toSlot.IsEmpty())
			{
				toSlot.SetItem(fromSlot.iItem, fromSlot.amount);
				fromSlot.SetItem(null, 0);
				SingletonGeneric<SoundManager>.Singleton.PlaySound("UIMoveItem");
			}
			else
			{
				if (fromSlot.iItem.index == toSlot.iItem.index && fromSlot.iItem.IsStackable())
				{
					int amount = fromSlot.amount;
					int amount2 = toSlot.amount;
					int stackSize = fromSlot.iItem.stackSize;
					int num = amount + amount2;
					if (num > stackSize)
					{
						toSlot.amount = stackSize;
						fromSlot.amount = num - stackSize;
					}
					else
					{
						toSlot.amount = num;
						fromSlot.SetItem(null, 0);
					}
				}
				else
				{
					IItem iItem = toSlot.iItem;
					int amount3 = toSlot.amount;
					toSlot.iItem = fromSlot.iItem;
					toSlot.amount = fromSlot.amount;
					fromSlot.iItem = iItem;
					fromSlot.amount = amount3;
				}
				toSlot.RefreshComponents();
				fromSlot.RefreshComponents();
				SingletonGeneric<SoundManager>.Singleton.PlaySound("UIMoveItem");
			}
			if (PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(fromSlot) || PlayerInventory.Singleton.hotbar.IsSelectedHotSlot(toSlot))
			{
				PlayerInventory.Singleton.hotbar.ReselectCurrentSlot();
			}
			fromSlot = (toSlot = null);
		}
	}

	public static IItem GetItemOfType(ItemIndex index)
	{
		if (index == ItemIndex.None)
		{
			return null;
		}
		foreach (IItem allAvailableItem in allAvailableItems)
		{
			if (allAvailableItem.index == index)
			{
				return allAvailableItem;
			}
		}
		return null;
	}

	public int GetItemCount(ItemIndex index)
	{
		int num = 0;
		foreach (Slot allSlot in allSlots)
		{
			if (!allSlot.IsEmpty() && allSlot.iItem.index == index)
			{
				num += allSlot.amount;
			}
		}
		return num;
	}

	public int GetSlotCount()
	{
		return allSlots.Count;
	}

	public Slot GetSlot(int index)
	{
		if (index >= 0 && index < allSlots.Count)
		{
			return allSlots[index];
		}
		return null;
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}

	public void SetLocalPosition(Vector3 position)
	{
		invRectTransform.localPosition = position;
	}

	protected virtual void CreateSlots()
	{
		allSlots = new List<Slot>();
		int num = Mathf.CeilToInt((float)numberOfSlots / (float)rows);
		float num2 = (float)(num * slotSize) + paddingLeft + paddingLeft * (float)num;
		float size = (float)(rows * slotSize) + paddingTop + paddingTop * (float)rows;
		SetLocalPosition(new Vector3((0f - num2) / 2f, invRectTransform.localPosition.y));
		invRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
		invRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		int num3 = 0;
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < num; j++)
			{
				GameObject gameObject = Object.Instantiate(slotPrefab, slotParent);
				RectTransform component = gameObject.GetComponent<RectTransform>();
				gameObject.name = "Slot: " + j + ", " + i;
				gameObject.transform.localScale = slotPrefab.transform.localScale;
				component.localPosition = new Vector3(paddingLeft + (float)(j * slotSize) + (float)j * paddingLeft, 0f - paddingTop + (float)(-i * slotSize) - paddingTop * (float)i);
				component.sizeDelta = new Vector2(slotSize, slotSize);
				allSlots.Add(gameObject.GetComponent<Slot>());
				num3++;
				if (num3 == numberOfSlots)
				{
					break;
				}
			}
			if (num3 == numberOfSlots)
			{
				break;
			}
		}
	}

	private Slot FindFirstEmptySlot()
	{
		foreach (Slot allSlot in allSlots)
		{
			if (allSlot.IsEmpty())
			{
				return allSlot;
			}
		}
		return null;
	}

	private Slot FindFirstStackableSlot(ItemIndex index)
	{
		foreach (Slot allSlot in allSlots)
		{
			if (!allSlot.IsEmpty() && allSlot.iItem.index == index && !allSlot.StackIsFull())
			{
				return allSlot;
			}
		}
		return null;
	}

	private void LoadAllIItems()
	{
		allAvailableItems = Enumerable.ToList<IItem>((IEnumerable<IItem>)Resources.LoadAll<IItem>("IItems"));
	}

	private void ResetQuickEquip()
	{
		canQuickEquip = true;
	}

	private void OnMenuClose()
	{
		toSlot = null;
		fromSlot = null;
		hoverSlot = null;
	}
}
