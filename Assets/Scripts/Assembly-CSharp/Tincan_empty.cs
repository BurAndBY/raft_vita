using UnityEngine;

public class Tincan_empty : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (AimingAtWater())
		{
			CanvasHelper.singleton.SetDisplayText("Fill with water", true);
		}
		else
		{
			CanvasHelper.singleton.SetDisplayText(false);
		}
	}

	public void OnUse()
	{
		if (!GameManager.IsInMenu && AimingAtWater())
		{
			RemoveResult removeResult = PlayerInventory.Singleton.RemoveSelectedHotSlotItem(1);
			PlayerInventory.Singleton.AddItem(ItemIndex.Tincan_saltWater, 1);
			CanvasHelper.singleton.SetDisplayText(false);
			SingletonGeneric<SoundManager>.Singleton.PlaySound("PickupWater");
		}
	}

	private bool AimingAtWater()
	{
		RaycastHit raycastHit = Helper.HitAtCursor(Player.UseDistance * 3f);
		return raycastHit.transform != null && raycastHit.transform.tag == "Water";
	}
}
