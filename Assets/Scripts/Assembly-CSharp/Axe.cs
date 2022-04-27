using UnityEngine;

public class Axe : MonoBehaviour
{
	public AxeMode mode;

	public BlockRemover blockRemover;

	public TreeChopper treeChopper;

	public LayerMask hitmask;

	private void Start()
	{
		blockRemover.enabled = false;
		treeChopper.enabled = false;
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		if (Input.GetButton("LeftClick"))
		{
			PlayerAnimator.SetAnimationAxeHit(true);
			RaycastHit hit = Helper.HitAtCursor(5f, hitmask);
			if (hit.transform != null)
			{
				if (hit.transform.tag == "Plant")
				{
					Plant componentInParent = hit.transform.GetComponentInParent<Plant>();
					if (componentInParent != null && !componentInParent.harvestable && componentInParent.FullyGrown())
					{
						SwitchMode(AxeMode.Chopping);
						treeChopper.currentPlant = componentInParent;
						treeChopper.UpdateOwn(hit);
						return;
					}
				}
				else
				{
					Block componentInParent2 = hit.transform.GetComponentInParent<Block>();
					if (componentInParent2 != null && componentInParent2.removeableWithAxe)
					{
						SwitchMode(AxeMode.RemovingBlock);
						blockRemover.UpdateOwn(componentInParent2);
						return;
					}
				}
			}
			SwitchMode(AxeMode.None);
		}
		else
		{
			PlayerAnimator.SetAnimationAxeHit(false);
			SwitchMode(AxeMode.None);
		}
	}

	public void OnSelect()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_7_Axe);
		switch (mode)
		{
		case AxeMode.Chopping:
			treeChopper.Select();
			break;
		case AxeMode.RemovingBlock:
			blockRemover.Select();
			break;
		}
	}

	public void OnDeSelect()
	{
		PlayerAnimator.SetAnimationAxeHit(false);
		switch (mode)
		{
		case AxeMode.Chopping:
			treeChopper.DeSelect();
			break;
		case AxeMode.RemovingBlock:
			blockRemover.DeSelect();
			break;
		}
		SwitchMode(AxeMode.None);
	}

	private void SwitchMode(AxeMode newMode)
	{
		if (newMode != mode)
		{
			switch (mode)
			{
			case AxeMode.RemovingBlock:
				blockRemover.DeSelect();
				break;
			case AxeMode.Chopping:
				treeChopper.DeSelect();
				break;
			}
			mode = newMode;
		}
	}
}
