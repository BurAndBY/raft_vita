using System.Collections.Generic;
using UnityEngine;

public class BlockRemover : MonoBehaviour
{
	public float destroyTime = 2f;

	public float timer;

	private Block currentBlock;

	private CanvasHelper canvas;

	public void UpdateOwn(Block block)
	{
		if (canvas == null)
		{
			canvas = CanvasHelper.singleton;
		}
		canvas.SetRemoveBlock(timer > 0f);
		canvas.SetRemoveBlock(timer / destroyTime);
		if (block != currentBlock)
		{
			ResetAxe();
			currentBlock = block;
		}
		timer += Time.deltaTime;
		if (timer >= destroyTime)
		{
			DestroyBlock(currentBlock);
			ResetAxe();
		}
	}

	public void Select()
	{
	}

	public void DeSelect()
	{
		canvas.SetRemoveBlock(false);
		ResetAxe();
		currentBlock = null;
	}

	public void OnHammerHit()
	{
		if (currentBlock != null)
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("WoodHit");
		}
	}

	public void ResetAxe()
	{
		timer = 0f;
	}

	private void DestroyBlock(Block block)
	{
		if (block == null)
		{
			return;
		}
		if (block.buildCost.Count == 0)
		{
			PlayerInventory.Singleton.AddItem(block.itemIndex, 1);
			switch (block.type)
			{
			case BlockType.ChestSmall:
			{
				Chest component2 = block.gameObject.GetComponent<Chest>();
				if (!(component2 != null))
				{
					break;
				}
				Inventory inventoryReference = component2.GetInventoryReference();
				for (int k = 0; k < inventoryReference.GetSlotCount(); k++)
				{
					Slot slot = inventoryReference.GetSlot(k);
					if (!slot.IsEmpty())
					{
						PlayerInventory.Singleton.AddItem(slot.iItem, slot.amount);
					}
				}
				break;
			}
			case BlockType.CookingStand_Food:
			case BlockType.CookingStand_Purifier:
			{
				CookingStand component3 = block.gameObject.GetComponent<CookingStand>();
				if (component3 != null && component3.IsComplete)
				{
					ItemIndex completeItem = component3.GetCompleteItem();
					PlayerInventory.Singleton.AddItem(Inventory.GetItemOfType(completeItem), 1);
				}
				break;
			}
			case BlockType.Net:
			{
				Net component4 = block.gameObject.GetComponent<Net>();
				if (component4 != null)
				{
					component4.collector.AddCollectedItemsToInventory();
				}
				break;
			}
			case BlockType.Cropplot_Tree:
			case BlockType.Cropplot:
			{
				Cropplot component = block.gameObject.GetComponent<Cropplot>();
				if (!(component != null))
				{
					break;
				}
				List<PlantationSlot> slots = component.GetSlots();
				for (int i = 0; i < slots.Count; i++)
				{
					if (!slots[i].busy)
					{
						continue;
					}
					Plant plant = slots[i].plant;
					if (plant.FullyGrown())
					{
						for (int j = 0; j < plant.yieldItems.Count; j++)
						{
							Cost cost = plant.yieldItems[j];
							PlayerInventory.Singleton.AddItem(cost.item, cost.amount);
						}
					}
				}
				break;
			}
			}
		}
		else
		{
			for (int l = 0; l < block.buildCost.Count; l++)
			{
				Cost cost2 = block.buildCost[l];
				int amount = Mathf.CeilToInt((float)cost2.amount * 0.5f);
				PlayerInventory.Singleton.AddItem(cost2.item, amount);
			}
		}
		SingletonGeneric<BlockPlacer>.Singleton.RemoveBlock(block);
		currentBlock = null;
	}
}
