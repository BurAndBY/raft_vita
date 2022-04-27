using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockMenu : MonoBehaviour
{
	public PieMenu piemenu;

	[Header("UI components")]
	public Text UIName;

	public Text UIDescription;

	public Image UIImage;

	public List<ItemCostBox> costboxes = new List<ItemCostBox>();

	[Header("Items to build with")]
	public List<Block> blockItems = new List<Block>();

	[HideInInspector]
	public BlockPlacer blockPlacer;

	public bool IsActive
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	private void Awake()
	{
		blockPlacer = Object.FindObjectOfType<BlockPlacer>();
		piemenu.Create(blockItems.Count);
		for (int i = 0; i < blockItems.Count; i++)
		{
			PieSlice slice = piemenu.GetSlice(i);
			slice.centerImage.sprite = blockItems[i].sprite;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		int pressButtonIndex = piemenu.GetPressButtonIndex();
		Block block = blockItems[pressButtonIndex];
		SelectBlock(block);
		if (Input.GetButtonUp("RightClick"))
		{
			SetState(false);
		}
	}

	public void SetState(bool state)
	{
		GameManager.IsInMenu = state;
		GameManager.IsInBuildMenu = state;
		base.gameObject.SetActive(state);
		if (state)
		{
			Helper.SetCursorVisibleAndLockState(false, CursorLockMode.None);
		}
		else
		{
			Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
		}
	}

	public void SelectBlock(Block block)
	{
		if (blockPlacer != null && block != null)
		{
			blockPlacer.SetBlockTypeToBuild(block.type);
			UIImage.sprite = block.sprite;
			UIName.text = block.displayName;
			UIDescription.text = block.description;
			DisableCostBoxes();
			for (int i = 0; i < block.buildCost.Count; i++)
			{
				Cost cost = block.buildCost[i];
				ItemCostBox itemCostBox = costboxes[i];
				itemCostBox.gameObject.SetActive(true);
				itemCostBox.SetCost(cost);
			}
		}
	}

	private void DisableCostBoxes()
	{
		foreach (ItemCostBox costbox in costboxes)
		{
			costbox.gameObject.SetActive(false);
		}
	}
}
