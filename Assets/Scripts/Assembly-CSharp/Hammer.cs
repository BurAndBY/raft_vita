using UnityEngine;

public class Hammer : MonoBehaviour
{
	public int blockRepairAmount;

	public ParticleController particles;

	private BlockMenu blockmenu;

	private BlockPlacer blockPlacer;

	private Block aimedAtBlock;

	private bool canHit;

	private void Awake()
	{
		blockmenu = Object.FindObjectOfType<BlockMenu>();
		blockmenu.gameObject.SetActive(false);
		blockPlacer = Object.FindObjectOfType<BlockPlacer>();
		particles.particleParent.SetParent(null);
	}

	private void Update()
	{
		if (Input.GetButtonDown("RightClick") && !GameManager.IsInMenu)
		{
			blockmenu.SetState(true);
		}
		if (blockPlacer.GetCurrentBlockType() != BlockType.Repair)
		{
			return;
		}
		if (Input.GetButton("LeftClick") && canHit)
		{
			canHit = false;
			PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_HammerHit);
		}
		if (PlayerInventory.Singleton.GetItemCount(ItemIndex.Plank) >= 1)
		{
			RaycastHit raycastHit = Helper.HitAtCursor(Player.UseDistance, LayerMasks.MASK_block);
			if (raycastHit.transform != null)
			{
				particles.SetPosition(raycastHit.point);
				particles.SetLookRotation(raycastHit.normal);
				Block componentInParent = raycastHit.transform.GetComponentInParent<Block>();
				if (componentInParent != null && componentInParent.CanBeRepaired())
				{
					aimedAtBlock = componentInParent;
				}
				else
				{
					aimedAtBlock = null;
				}
			}
			else
			{
				aimedAtBlock = null;
			}
		}
		else
		{
			aimedAtBlock = null;
		}
	}

	public void OnSelect()
	{
		canHit = true;
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_4_Hammer);
		blockPlacer.enabled = true;
		if (blockPlacer.GetCurrentBlockType() == BlockType.None)
		{
			blockPlacer.SetBlockTypeToBuild(BlockType.Foundation);
		}
	}

	public void OnDeSelect()
	{
		blockPlacer.SetGhostBlockState(false);
		blockPlacer.enabled = false;
		foreach (ItemCostBox costbox in blockPlacer.costboxes)
		{
			costbox.gameObject.SetActive(false);
		}
		CanvasHelper.singleton.SetDisplayText(false);
	}

	public void OnHammerHit()
	{
		canHit = true;
		if (!(aimedAtBlock == null))
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("WoodHit");
			particles.PlayParticles();
			aimedAtBlock.Repair(blockRepairAmount);
			PlayerInventory.Singleton.RemoveItem(ItemIndex.Plank, 1);
		}
	}
}
