using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
	[HideInInspector]
	public IItem iItem;

	[HideInInspector]
	public int amount;

	public Image imageComponent;

	public Text textComponent;

	public Text slotNumberTextComponent;

	[HideInInspector]
	public RectTransform rectTransform;

	private Inventory inventory;

	private Sprite defaultSprite;

	private void Awake()
	{
		defaultSprite = imageComponent.sprite;
		slotNumberTextComponent.gameObject.SetActive(false);
		rectTransform = GetComponent<RectTransform>();
		inventory = base.transform.GetComponentInParent<Inventory>();
		if (inventory == null)
		{
			inventory = PlayerInventory.Singleton;
		}
		EventTrigger componentInParent = GetComponentInParent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback.AddListener(delegate
		{
			inventory.MoveItem(this);
		});
		componentInParent.triggers.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerUp;
		entry.callback.AddListener(delegate
		{
			inventory.MoveItem(this);
		});
		componentInParent.triggers.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener(delegate
		{
			inventory.HoverEnter(this);
		});
		componentInParent.triggers.Add(entry);
		entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerExit;
		entry.callback.AddListener(delegate
		{
			inventory.HoverExit(this);
		});
		componentInParent.triggers.Add(entry);
	}

	private void Start()
	{
		RefreshComponents();
	}

	private void Update()
	{
	}

	public void SetItem(IItem newItem, int amount)
	{
		iItem = newItem;
		this.amount = amount;
		RefreshComponents();
	}

	public void AddItem(IItem newItem)
	{
		if (iItem == null)
		{
			iItem = newItem;
			amount++;
		}
		else if (iItem.index == newItem.index)
		{
			amount++;
		}
		RefreshComponents();
	}

	public void RemoveItem(int amount)
	{
		if (!IsEmpty())
		{
			this.amount -= amount;
			if (this.amount <= 0)
			{
				Reset();
			}
			else
			{
				RefreshComponents();
			}
		}
	}

	public bool IsEmpty()
	{
		return iItem == null;
	}

	public bool StackIsFull()
	{
		if (IsEmpty())
		{
			return false;
		}
		if (amount == iItem.stackSize)
		{
			return true;
		}
		return false;
	}

	public void RefreshComponents()
	{
		if (imageComponent != null)
		{
			if (iItem == null)
			{
				imageComponent.sprite = defaultSprite;
			}
			else
			{
				imageComponent.sprite = iItem.sprite;
			}
			imageComponent.enabled = imageComponent.sprite != null;
		}
		if (iItem != null && amount > 1)
		{
			textComponent.text = amount.ToString();
		}
		else
		{
			textComponent.text = string.Empty;
		}
	}

	private void Reset()
	{
		iItem = null;
		amount = 0;
		RefreshComponents();
	}
}
