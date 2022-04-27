using UnityEngine;

public class Hotbar : MonoBehaviour
{
	public RectTransform hotbarSelectionTransform;

	private PlayerItemManager playerItemManager;

	private int slotIndex;

	private void Awake()
	{
		playerItemManager = Object.FindObjectOfType<PlayerItemManager>();
	}

	private void Start()
	{
		float num = PlayerInventory.Singleton.slotSize;
		hotbarSelectionTransform.sizeDelta = new Vector2(num, num);
		Invoke("LateStart", 0.05f);
	}

	private void LateStart()
	{
		SelectHotslot(PlayerInventory.Singleton.GetSlot(1));
		SelectHotslot(PlayerInventory.Singleton.GetSlot(0));
	}

	private void Update()
	{
		HandleHotbarSelection();
	}

	public void SelectHotslot(Slot slot)
	{
		if (slot != null)
		{
			hotbarSelectionTransform.localPosition = slot.rectTransform.localPosition;
		}
		if (slot == null || slot.iItem == null)
		{
			SelectItem(null);
		}
		else if (slot.iItem is IUsableItem)
		{
			IUsableItem usableItem = slot.iItem as IUsableItem;
			if (usableItem != null)
			{
				SelectItem(usableItem);
			}
		}
		else
		{
			SelectItem(null);
		}
	}

	public void ReselectCurrentSlot()
	{
		Slot slot = PlayerInventory.Singleton.GetSlot(slotIndex);
		SelectHotslot(slot);
	}

	public int GetSelectedSlotIndex()
	{
		return slotIndex;
	}

	public void SetSelectedSlotIndex(int index)
	{
		slotIndex = index;
	}

	public bool IsSelectedHotSlot(Slot slot)
	{
		if (slot == null)
		{
			return false;
		}
		return PlayerInventory.Singleton.GetSlot(slotIndex) == slot;
	}

	private void HandleHotbarSelection()
	{
		if (GameManager.IsInMenu || !playerItemManager.CanSwitch())
		{
			return;
		}
		int num = 49;
		for (int i = num; i < num + 8; i++)
		{
			KeyCode key = (KeyCode)(num + (i - num));
			if (Input.GetKeyDown(key))
			{
				slotIndex = i - num;
				Slot slot = PlayerInventory.Singleton.GetSlot(slotIndex);
				SelectHotslot(slot);
			}
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis > 0f)
		{
			slotIndex--;
		}
		else if (axis < 0f)
		{
			slotIndex++;
		}
		if (axis > 0f || axis < 0f)
		{
			int hotslotCount = PlayerInventory.Singleton.hotslotCount;
			if (slotIndex > hotslotCount - 1)
			{
				slotIndex = 0;
			}
			else if (slotIndex < 0)
			{
				slotIndex = hotslotCount - 1;
			}
			Slot slot2 = PlayerInventory.Singleton.GetSlot(slotIndex);
			SelectHotslot(slot2);
		}
	}

	private void SelectItem(IUsableItem item)
	{
		if (item == null)
		{
			playerItemManager.SelectNothing();
		}
		else
		{
			playerItemManager.SelectUsable(item);
		}
	}
}
