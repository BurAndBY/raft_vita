using UnityEngine;

[RequireComponent(typeof(Throwable))]
[RequireComponent(typeof(LineRenderer))]
public class FishingRod : MonoBehaviour
{
	[Header("Components")]
	public ParticleSystem waterSystem;

	public ParticleSystem waterRingSystem;

	[Space]
	public Bobber bobber;

	public Transform fishingLineStart;

	private Throwable throwable;

	private LineRenderer lineRenderer;

	private void Awake()
	{
		throwable = GetComponent<Throwable>();
		throwable.SubscribeToWaterHitEvent(OnWaterHit);
		throwable.SubscribeToThrowEvent(OnThrow);
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.SetVertexCount(2);
		Transform obj = waterRingSystem.transform;
		Transform parent = null;
		waterSystem.transform.parent = parent;
		obj.parent = parent;
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		if (throwable.InWater && !throwable.CanThrow && Input.GetButtonDown("LeftClick"))
		{
			PullItemFromSea();
		}
		if (throwable.InHand)
		{
			if (throwable.CanThrow && Input.GetButtonDown("LeftClick"))
			{
				PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_FishingLoad);
			}
			if (Input.GetButtonUp("LeftClick"))
			{
				throwable.ResetCanThrow();
			}
		}
		if (bobber.Escaped())
		{
			PullItemFromSea();
			throwable.ResetCanThrow();
		}
		if (bobber.FishIsOnHook())
		{
			waterRingSystem.Stop();
		}
		lineRenderer.SetPosition(0, fishingLineStart.position);
		lineRenderer.SetPosition(1, bobber.transform.position);
	}

	public void OnSelect()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_3_Fishing);
	}

	private void PullItemFromSea()
	{
		IItem item = bobber.TryToGetFish();
		if (item != null)
		{
			PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_FishingRetract);
			PlayerInventory.Singleton.AddItem(item, 1);
		}
		else
		{
			PlayerAnimator.SetAnimation(PlayerAnimation.Index_3_Fishing, true, false);
		}
		bobber.StopBobbing();
		throwable.PickupThrowable();
		waterSystem.Play();
		waterRingSystem.Stop();
	}

	private void OnThrow()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_FishingThrow);
	}

	private void OnWaterHit()
	{
		bobber.StartBobbing();
		waterRingSystem.transform.position = bobber.transform.position;
		waterRingSystem.Play();
		waterSystem.transform.position = bobber.transform.position;
		waterSystem.Play();
		bobber.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
	}
}
