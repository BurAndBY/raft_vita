using UnityEngine;

public class Helper
{
	public static PlayerItemManager playerItemManager;

	public static GameObject aimCursor;

	private static CursorLockMode lockMode;

	public static void Update()
	{
		Cursor.lockState = lockMode;
	}

	public static void Initialize()
	{
		if (playerItemManager == null)
		{
			playerItemManager = Object.FindObjectOfType<PlayerItemManager>();
		}
	}

	public static RaycastHit HitAtCursor(float rayDistance)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hitInfo;
		Physics.Raycast(ray, out hitInfo, rayDistance, LayerMasks.MASK_ignorePlayer);
		return hitInfo;
	}

	public static RaycastHit HitAtCursor(float rayDistance, LayerMask mask)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hitInfo;
		Physics.Raycast(ray, out hitInfo, rayDistance, mask);
		return hitInfo;
	}

	public static RaycastHit HitAtCursorCompareLayers(float rayDistance, LayerMask layerToCompareWith)
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, rayDistance, LayerMasks.MASK_ignorePlayer) && ((1 << hitInfo.transform.gameObject.layer) & (int)layerToCompareWith) != 0)
		{
			return hitInfo;
		}
		return hitInfo;
	}

	public static void SetCursorVisible(bool state)
	{
		Cursor.visible = state;
		if (aimCursor != null)
		{
			aimCursor.SetActive(!state);
		}
	}

	public static void SetCursorLockState(CursorLockMode mode)
	{
		Cursor.lockState = mode;
		lockMode = mode;
	}

	public static void SetCursorVisibleAndLockState(bool state, CursorLockMode mode)
	{
		SetCursorVisible(state);
		SetCursorLockState(mode);
	}

	public static Vector3 GetColliderExtents(Collider col)
	{
		if (col == null)
		{
			return Vector3.zero;
		}
		if (col.enabled)
		{
			return col.bounds.extents;
		}
		col.enabled = true;
		Vector3 extents = col.bounds.extents;
		col.enabled = false;
		return extents;
	}

	public static Vector3 GetColliderCenter(Collider col)
	{
		if (col == null)
		{
			return Vector3.zero;
		}
		if (col.enabled)
		{
			return col.bounds.center;
		}
		col.enabled = true;
		Vector3 center = col.bounds.center;
		col.enabled = false;
		return center;
	}

	public static void DropItem(IItem item, int amount, Vector3 position)
	{
		if (item == null)
		{
			return;
		}
		GameObject objectFromPool = PoolManager.singleton.GetObjectFromPool(ItemIndex.DropItem);
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIDropItem");
		if (!(objectFromPool != null))
		{
			return;
		}
		PickupItem component = objectFromPool.GetComponent<PickupItem>();
		if (component != null)
		{
			component.transform.SetParent(GameManager.singleton.globalRaftParent);
			component.transform.position = position;
			component.iItem = item;
			component.amount = amount;
			component.isDropped = true;
			Rigidbody component2 = component.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.velocity = Vector3.zero;
				component2.AddForce(Camera.main.transform.forward * 50f + Vector3.up * 150f);
			}
			DropItem component3 = component.transform.GetComponent<DropItem>();
			if (component3 != null)
			{
				component3.ResetItem();
			}
		}
	}
}
