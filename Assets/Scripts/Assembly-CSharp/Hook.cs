using UnityEngine;

[RequireComponent(typeof(Throwable))]
[RequireComponent(typeof(LineRenderer))]
public class Hook : MonoBehaviour
{
	public Collider hookObjectCollider;

	public ItemCollector hookObject;

	public ParticleSystem waterSplash;

	public float pullSpeed = 3f;

	private Throwable throwable;

	private LineRenderer lineRenderer;

	private void Awake()
	{
		throwable = GetComponent<Throwable>();
		lineRenderer = GetComponent<LineRenderer>();
		throwable.SubscribeToThrowEvent(OnTrow);
		throwable.SubscribeToWaterHitEvent(OnHitWater);
		hookObjectCollider.enabled = false;
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		if (throwable.InWater)
		{
			Vector3 position = base.transform.position;
			Vector3 position2 = hookObject.transform.position;
			position2.y = position.y;
			Vector3 up = position2 - position;
			up.Normalize();
			hookObject.transform.up = up;
			hookObject.transform.eulerAngles = new Vector3(-270f, hookObject.transform.eulerAngles.y, hookObject.transform.eulerAngles.z);
			if (Input.GetButton("LeftClick"))
			{
				waterSplash.transform.position = hookObject.transform.position;
				if (!waterSplash.isPlaying)
				{
					waterSplash.loop = true;
					waterSplash.Play();
				}
				Vector3 positon = throwable.GetPositon();
				Vector3 vector = positon - base.transform.position;
				vector.y = 0f;
				vector.Normalize();
				positon -= vector * Time.deltaTime * pullSpeed;
				throwable.SetPosition(positon);
				Vector3 position3 = base.transform.position;
				position3.y = throwable.waterLevel;
				float num = Vector3.Distance(positon, position3);
				if (num <= 1f)
				{
					ResetHookToPlayer();
					hookObject.AddCollectedItemsToInventory();
				}
			}
			else if (Input.GetButtonUp("LeftClick"))
			{
				if (waterSplash.isPlaying)
				{
					waterSplash.Stop();
					waterSplash.loop = false;
				}
			}
			else if (Input.GetButtonDown("RightClick") && hookObject.GetItemCount() <= 0)
			{
				throwable.ResetCanThrow();
				ResetHookToPlayer();
			}
		}
		if (throwable.InHand)
		{
			if (throwable.CanThrow && Input.GetButtonDown("LeftClick"))
			{
				PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_HookLoad);
			}
			if (Input.GetButtonUp("LeftClick"))
			{
				throwable.ResetCanThrow();
			}
		}
		lineRenderer.enabled = !throwable.InHand;
		if (lineRenderer.enabled)
		{
			lineRenderer.SetPosition(0, base.transform.position);
			lineRenderer.SetPosition(1, throwable.GetPositon());
		}
	}

	public void OnSelect()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_2_Hook);
	}

	private void OnTrow()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_HookThrow);
	}

	private void OnHitWater()
	{
		waterSplash.transform.parent = null;
		waterSplash.transform.eulerAngles = Vector3.zero;
		waterSplash.transform.position = hookObject.transform.position;
		waterSplash.Play();
		hookObjectCollider.enabled = true;
	}

	private void ResetHookToPlayer()
	{
		waterSplash.loop = false;
		waterSplash.Stop();
		throwable.PickupThrowable();
		hookObjectCollider.enabled = false;
		PlayerAnimator.SetAnimation(PlayerAnimation.Trigger_HookFinished);
	}
}
