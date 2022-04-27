using UnityEngine;

[RequireComponent(typeof(UseItemController))]
public class PlayerItemManager : MonoBehaviour
{
	private enum PlayerState
	{
		Nothing,
		Using
	}

	public static bool IsBusy;

	[HideInInspector]
	public Hotbar hotbar;

	private PlayerState state;

	private UseItemController useItemController;

	private void Awake()
	{
		hotbar = Object.FindObjectOfType<Hotbar>();
		useItemController = GetComponent<UseItemController>();
	}

	public bool CanSwitch()
	{
		return !IsBusy;
	}

	public void SelectUsable(IUsableItem item)
	{
		SwitchState(PlayerState.Using);
		useItemController.StartUsing(item);
	}

	public void SelectNothing()
	{
		SwitchState(PlayerState.Nothing);
	}

	private void SwitchState(PlayerState newState)
	{
		SwitchOut(state);
		SwitchIn(newState);
	}

	private void SwitchOut(PlayerState stateOut)
	{
		if (stateOut != 0 && stateOut == PlayerState.Using)
		{
			useItemController.Deselect();
		}
	}

	private void SwitchIn(PlayerState stateIn)
	{
		IsBusy = false;
		switch (stateIn)
		{
		case PlayerState.Nothing:
			PlayerAnimator.SetAnimation(PlayerAnimation.Index_0_Idle);
			break;
		}
		state = stateIn;
	}
}
