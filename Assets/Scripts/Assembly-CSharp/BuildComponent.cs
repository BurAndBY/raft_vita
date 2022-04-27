using UnityEngine;

public class BuildComponent : MonoBehaviour
{
	public BlockType blockType;

	private static BlockPlacer blockPlacer;

	private BlockType previousType;

	private void Start()
	{
		if (blockPlacer == null)
		{
			blockPlacer = SingletonGeneric<BlockPlacer>.Singleton;
		}
		blockPlacer.SubsribeToOnPlace(OnPlaceBlock);
	}

	private void OnDisable()
	{
		if (blockPlacer == null)
		{
			blockPlacer = SingletonGeneric<BlockPlacer>.Singleton;
		}
		blockPlacer.Disable();
	}

	public void OnSelect()
	{
		previousType = blockPlacer.GetCurrentBlockType();
		blockPlacer.enabled = true;
		blockPlacer.SetBlockTypeToBuild(blockType);
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_1_Point);
	}

	public void OnDeSelect()
	{
		blockPlacer.SetBlockTypeToBuild(previousType);
		blockPlacer.Disable();
	}

	private void OnPlaceBlock()
	{
		if (base.gameObject.activeInHierarchy)
		{
			PlayerInventory.Singleton.RemoveSelectedHotSlotItem(1);
			PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_PointPlace, true);
			blockPlacer.SetBlockTypeToBuild(previousType);
		}
	}
}
