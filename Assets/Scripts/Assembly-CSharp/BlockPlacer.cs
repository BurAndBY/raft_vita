using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockPlacer : SingletonGeneric<BlockPlacer>
{
	public delegate void OnBlockChange();

	private OnBlockChange placeBlockCallstack;

	[Header("Canvas")]
	public List<ItemCostBox> costboxes = new List<ItemCostBox>();

	[Header("Build settings")]
	public float blockSize = 1.5f;

	public Material ghostMaterial;

	[HideInInspector]
	public List<Block> allPlacedBlocks = new List<Block>();

	[Space]
	public List<Block> blockPrefabs = new List<Block>();

	private BlockType selectedBlockType = BlockType.Foundation;

	private BlockQuad selectedQuad;

	private Vector3 selectedQuadHitPoint;

	private Block selectedBlock;

	private Transform ghostBlock;

	private const float rotateDegreeSnap = 90f;

	private const float rotateDegreeSmooth = 5f;

	public float currentRotationY;

	private float rotationKeyAccumulation;

	private bool aimingAtQuad;

	private void Awake()
	{
		foreach (ItemCostBox costbox in costboxes)
		{
			costbox.gameObject.SetActive(false);
		}
		FindActiveBlocksAtStart();
		if (GameManager.IsInNewGame)
		{
			return;
		}
		foreach (Block allPlacedBlock in allPlacedBlocks)
		{
			UnityEngine.Object.Destroy(allPlacedBlock.gameObject);
		}
		allPlacedBlocks.Clear();
	}

	private void Start()
	{
		SetGhostBlockState(false);
	}

	private void Update()
	{
		Block blockFromType = GetBlockFromType(selectedBlockType);
		if (blockFromType == null)
		{
			return;
		}
		SelectBlock(blockFromType);
		selectedQuad = AquireQuadAtCursor();
		aimingAtQuad = selectedQuad != null;
		bool flag = selectedQuad != null && selectedQuad.AcceptsBlock(selectedBlock);
		SetGhostBlockState(flag);
		if (selectedBlock.isRotateable)
		{
			if (Input.GetButtonDown("Rotate"))
			{
				if (selectedBlock.canRotateFreely)
				{
					GameManager.singleton.player.SetMouseLookScripts(false);
				}
				else
				{
					RotateBlock(selectedBlock, 90f, true);
				}
			}
			if (selectedBlock.canRotateFreely)
			{
				float num = 0.2f;
				if (Input.GetButton("Rotate"))
				{
					rotationKeyAccumulation += Time.deltaTime;
					if (rotationKeyAccumulation >= num)
					{
						float axis = Input.GetAxis("Mouse X");
						RotateBlock(selectedBlock, axis * 5f);
					}
				}
				if (Input.GetButtonUp("Rotate"))
				{
					if (rotationKeyAccumulation < num)
					{
						RotateBlock(selectedBlock, 90f, true);
					}
					rotationKeyAccumulation = 0f;
					GameManager.singleton.player.SetMouseLookScripts(true);
				}
			}
		}
		SetGhostBlockPosition();
		ShowCostOfSelectedBlock();
		if (flag)
		{
			bool flag2 = true;
			bool flag3 = PositionOccupied(selectedBlock);
			bool flag4 = selectedBlock.IsStable();
			bool flag5 = HasEnoughResourcesToBuild(selectedBlock);
			if (flag3 || !flag4 || !flag5)
			{
				flag2 = false;
			}
			if (flag && flag2)
			{
				ghostMaterial.color = Color.green;
			}
			else
			{
				ghostMaterial.color = Color.red;
			}
			if (Input.GetButtonDown("LeftClick") && flag2 && !GameManager.IsInMenu)
			{
				PlaceBlock();
			}
		}
	}

	public void SubsribeToOnPlace(OnBlockChange method)
	{
		placeBlockCallstack = (OnBlockChange)Delegate.Combine(placeBlockCallstack, method);
	}

	public void SetBlockTypeToBuild(BlockType type)
	{
		selectedBlockType = type;
		if (type == BlockType.Repair)
		{
			SetGhostBlockState(false);
		}
	}

	public void RemoveBlock(Block block)
	{
		DestroyBlock(block);
		for (int i = 0; i < allPlacedBlocks.Count; i++)
		{
			if (allPlacedBlocks[i] == null)
			{
				allPlacedBlocks.RemoveAt(i--);
			}
		}
		foreach (Block allPlacedBlock in allPlacedBlocks)
		{
			allPlacedBlock.RefreshOverlapps();
		}
	}

	public void SetGhostBlockState(bool state)
	{
		if (!(ghostBlock == null))
		{
			ghostBlock.gameObject.SetActive(state);
		}
	}

	public void Disable()
	{
		if (this != null)
		{
			base.enabled = false;
			SetGhostBlockState(false);
		}
	}

	public BlockType GetCurrentBlockType()
	{
		return selectedBlockType;
	}

	public Block GetBlockFromType(BlockType type)
	{
		foreach (Block blockPrefab in blockPrefabs)
		{
			if (blockPrefab.type == type)
			{
				return blockPrefab;
			}
		}
		return null;
	}

	public void SetStartingBlockCollision()
	{
		foreach (Block allPlacedBlock in allPlacedBlocks)
		{
			allPlacedBlock.overlappingComponent.LookForOverlapps(false);
		}
	}

	private void ShowCostOfSelectedBlock()
	{
		if (selectedBlock == null || GameManager.IsInMenu)
		{
			foreach (ItemCostBox costbox in costboxes)
			{
				costbox.gameObject.SetActive(false);
			}
			CanvasHelper.singleton.SetDisplayText(false);
			return;
		}
		foreach (ItemCostBox costbox2 in costboxes)
		{
			costbox2.gameObject.SetActive(false);
		}
		for (int i = 0; i < selectedBlock.buildCost.Count; i++)
		{
			Cost cost = selectedBlock.buildCost[i];
			costboxes[i].gameObject.SetActive(true);
			costboxes[i].SetCost(cost);
		}
		if (selectedBlock.type == BlockType.Repair)
		{
			return;
		}
		string text = selectedBlock.displayName;
		if (selectedBlock.isRotateable)
		{
			text += "\n'R' to rotate";
			if (selectedBlock.canRotateFreely)
			{
				text += ", hold for smooth rotation";
			}
		}
		CanvasHelper.singleton.SetDisplayText(text);
	}

	private void SelectBlock(Block block)
	{
		if (!(selectedBlock != null) || !(block != null) || selectedBlock.type != block.type)
		{
			DestroyGhost();
			ghostBlock = UnityEngine.Object.Instantiate(block.prefab, Vector3.zero, block.prefab.transform.rotation).transform;
			ghostBlock.SetParent(GameManager.singleton.globalRaftParent);
			ghostBlock.eulerAngles = new Vector3(ghostBlock.parent.eulerAngles.x, block.prefab.transform.eulerAngles.y, ghostBlock.parent.eulerAngles.z);
			selectedBlock = ghostBlock.GetComponent<Block>();
			currentRotationY = Mathf.RoundToInt(currentRotationY / 90f) * 90;
			if (block.isRotateable)
			{
				ghostBlock.Rotate(Vector3.up, currentRotationY, Space.Self);
			}
			selectedBlock.OnStartingPlacement();
			if (selectedBlock.overlappingComponent != null)
			{
				selectedBlock.overlappingComponent.SetNewMaterial(ghostMaterial);
			}
			SetGhostBlockState(false);
		}
	}

	private void DestroyBlock(Block block)
	{
		if (block != null)
		{
			UnityEngine.Object.DestroyImmediate(block.gameObject);
		}
		foreach (Block allPlacedBlock in allPlacedBlocks)
		{
			if (!(allPlacedBlock == block) && allPlacedBlock != null && !allPlacedBlock.IsStable())
			{
				DestroyBlock(allPlacedBlock);
			}
		}
	}

	private void RotateBlock(Block block, float degrees, bool snap = false)
	{
		if (!(block == null))
		{
			currentRotationY += degrees;
			if (snap)
			{
				currentRotationY = Mathf.RoundToInt(currentRotationY / 90f) * 90;
			}
			if (currentRotationY > 360f)
			{
				currentRotationY -= 360f;
			}
			Transform transform = block.transform;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentRotationY, transform.eulerAngles.z);
		}
	}

	private void PlaceBlock()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_HammerHit);
		CanvasHelper.singleton.SetDisplayText(false);
		SingletonGeneric<SoundManager>.Singleton.PlaySound("PlaceObject");
		GameManager.singleton.player.SetMouseLookScripts(true);
		foreach (Cost item in selectedBlock.buildCost)
		{
			PlayerInventory.Singleton.RemoveItem(item.item, item.amount);
		}
		ghostBlock = null;
		selectedQuad = null;
		allPlacedBlocks.Add(selectedBlock);
		if (selectedBlock.overlappingComponent != null)
		{
			selectedBlock.overlappingComponent.RestoreToDefaultMaterial();
		}
		if (placeBlockCallstack != null)
		{
			placeBlockCallstack();
		}
		selectedBlock.RefreshOverlapps();
		selectedBlock.OnFinishedPlacement();
		selectedBlock = null;
	}

	private void SetGhostBlockPosition()
	{
		if (!(ghostBlock == null) && !(selectedQuad == null))
		{
			if (selectedBlock.snapsToQuads)
			{
				ghostBlock.position = selectedQuad.transform.position + selectedBlock.buildOffset;
			}
			else
			{
				ghostBlock.position = selectedQuadHitPoint + selectedBlock.buildOffset;
			}
			ghostBlock.rotation = selectedBlock.prefab.transform.rotation;
		}
	}

	private void DestroyGhost()
	{
		if (ghostBlock != null)
		{
			UnityEngine.Object.Destroy(ghostBlock.gameObject);
		}
	}

	private void FindActiveBlocksAtStart()
	{
		allPlacedBlocks = Enumerable.ToList<Block>((IEnumerable<Block>)UnityEngine.Object.FindObjectsOfType<Block>());
		foreach (Block allPlacedBlock in allPlacedBlocks)
		{
			allPlacedBlock.transform.SetParent(GameManager.singleton.globalRaftParent);
			allPlacedBlock.OnFinishedPlacement();
		}
	}

	private bool PositionOccupied(Block block)
	{
		if (block.type == BlockType.Wall_Window || block.type == BlockType.Wall)
		{
			bool result = false;
			AdvancedCollision[] advancedCollisions = block.overlappingComponent.advancedCollisions;
			foreach (AdvancedCollision advancedCollision in advancedCollisions)
			{
				foreach (Collider overlappingCollider in advancedCollision.overlappingColliders)
				{
					if (!(overlappingCollider != null))
					{
						continue;
					}
					Block componentInParent = overlappingCollider.GetComponentInParent<Block>();
					if (componentInParent != null)
					{
						if (componentInParent.type != BlockType.Wall && componentInParent.type != BlockType.Wall_Window)
						{
							result = true;
						}
						if (componentInParent.transform.position == block.transform.position && Mathf.RoundToInt(componentInParent.transform.localEulerAngles.y) == Mathf.RoundToInt(block.transform.localEulerAngles.y))
						{
							result = true;
						}
					}
				}
			}
			return result;
		}
		return block.overlappingComponent.IsCollidingWithOtherBlocks();
	}

	private BlockQuad AquireQuadAtCursor()
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, Player.UseDistance * 2f, LayerMasks.MASK_buildQuad))
		{
			BlockQuad component = hitInfo.transform.GetComponent<BlockQuad>();
			if (selectedBlock.snapsToTopCollider)
			{
				Collider component2 = hitInfo.transform.GetComponent<Collider>();
				if (component2 != null)
				{
					Vector3 point = hitInfo.point;
					point.y = component2.bounds.max.y;
					selectedQuadHitPoint = point;
				}
			}
			else
			{
				selectedQuadHitPoint = hitInfo.point;
			}
			return component;
		}
		return null;
	}

	private bool HasEnoughResourcesToBuild(Block block)
	{
		if (block == null)
		{
			return false;
		}
		foreach (Cost item in block.buildCost)
		{
			int itemCount = PlayerInventory.Singleton.GetItemCount(item.item.index);
			if (itemCount < item.amount)
			{
				return false;
			}
		}
		return true;
	}
}
