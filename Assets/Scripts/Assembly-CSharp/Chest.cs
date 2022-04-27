using UnityEngine;

public class Chest : MonoBehaviour
{
	[Header("Create inventory settings")]
	public Inventory inventoryPrefab;

	[Header("Chest settings")]
	public Animator anim;

	private Inventory inventoryReference;

	private bool isOpen;

	private bool canCloseWithUsebutton;

	private void Awake()
	{
		inventoryReference = Object.Instantiate(inventoryPrefab, Inventory.Parent);
		inventoryReference.transform.localScale = inventoryPrefab.transform.localScale;
		inventoryReference.transform.localPosition = inventoryPrefab.transform.localPosition;
	}

	private void Start()
	{
		CanvasHelper.singleton.SubscribeToMenuClose(OnMenuClose);
	}

	private void Update()
	{
		if (isOpen && Input.GetButtonDown("UseButton") && canCloseWithUsebutton)
		{
			CanvasHelper.singleton.CloseMenus();
		}
	}

	private void OnMouseOver()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		if (!isOpen && !PlayerItemManager.IsBusy && Player.IsWithingDistance(base.transform.position, Player.UseDistance + 0.5f))
		{
			CanvasHelper.singleton.SetDisplayText("Open", true);
			if (Input.GetButtonDown("UseButton"))
			{
				Open();
			}
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

	private void OnDestroy()
	{
		if (inventoryReference != null)
		{
			Object.Destroy(inventoryReference.gameObject);
		}
	}

	public Inventory GetInventoryReference()
	{
		return inventoryReference;
	}

	public void OnMenuClose()
	{
		if (!(this == null) && !(base.gameObject == null) && base.gameObject.activeInHierarchy && isOpen)
		{
			Close();
		}
	}

	private void Open()
	{
		isOpen = true;
		inventoryReference.Show();
		CanvasHelper.singleton.OpenMenus();
		SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("ChestOpen", base.transform.position, true);
		anim.SetBool("IsOpen", isOpen);
		Invoke("AllowCloseWithUsebutton", 0.1f);
	}

	private void Close()
	{
		isOpen = false;
		canCloseWithUsebutton = false;
		inventoryReference.Hide();
		SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("ChestClose", base.transform.position, true);
		Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
		anim.SetBool("IsOpen", isOpen);
	}

	private void AllowCloseWithUsebutton()
	{
		canCloseWithUsebutton = true;
	}
}
