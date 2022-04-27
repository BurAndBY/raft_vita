using System;
using UnityEngine;

[RequireComponent(typeof(ChargeMeter))]
public class Throwable : MonoBehaviour
{
	public delegate void ThrowableDelegate();

	private ThrowableDelegate waterHitCallstack;

	private ThrowableDelegate throwCallStack;

	public Transform throwableObject;

	public Rigidbody throwableObjectBody;

	public Vector3 throwForceMultiplier;

	public float waterLevel = -0.324f;

	public bool flowWithWater;

	private Transform throwableObjectParent;

	private ChargeMeter chargeMeter;

	private Vector3 throwableStartPosition;

	private Vector3 throwableStartRotation;

	private Vector3 throwableStartLocalScale;

	private bool inHand = true;

	private bool inWater;

	private bool canThrow = true;

	public bool InHand
	{
		get
		{
			return inHand;
		}
		set
		{
			inHand = value;
		}
	}

	public bool InWater
	{
		get
		{
			return inWater;
		}
		set
		{
			inWater = value;
		}
	}

	public bool CanThrow
	{
		get
		{
			return canThrow;
		}
		set
		{
			canThrow = value;
		}
	}

	private void Awake()
	{
		chargeMeter = GetComponent<ChargeMeter>();
	}

	private void Start()
	{
		throwableObjectParent = throwableObject.parent;
		throwableStartPosition = throwableObject.localPosition;
		throwableStartRotation = throwableObject.localEulerAngles;
		throwableStartLocalScale = throwableObject.localScale;
		throwableObjectBody.isKinematic = true;
	}

	private void Update()
	{
		if (GameManager.IsInMenu)
		{
			return;
		}
		if (InWater || !InHand || !CanThrow || chargeMeter.ChargeNormal > 0f)
		{
			PlayerItemManager.IsBusy = true;
		}
		else
		{
			PlayerItemManager.IsBusy = false;
		}
		if (CanThrow)
		{
			if (Input.GetButton("LeftClick"))
			{
				chargeMeter.Charge();
			}
			if (Input.GetButtonUp("LeftClick") && chargeMeter.ChargeNormal > 0f)
			{
				InHand = false;
				CanThrow = false;
				if (throwCallStack != null)
				{
					throwCallStack();
				}
				SingletonGeneric<SoundManager>.Singleton.PlaySound("Throw");
				throwableObject.parent = null;
				throwableObjectBody.isKinematic = false;
				Vector3 force = Camera.main.transform.forward * (chargeMeter.ChargeNormal * throwForceMultiplier.z) + Camera.main.transform.right * (chargeMeter.ChargeNormal * throwForceMultiplier.x) + Vector3.up * (chargeMeter.ChargeNormal * throwForceMultiplier.y);
				throwableObjectBody.AddForce(force);
				chargeMeter.Reset();
			}
		}
		if (!InHand && throwableObject.position.y <= waterLevel && !InWater)
		{
			if (flowWithWater && throwableObject.parent == null)
			{
				throwableObject.parent = GameManager.singleton.waterFlowParent;
			}
			if (waterHitCallstack != null)
			{
				waterHitCallstack();
				AudioSource audioSource = SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("WaterPlump", throwableObject.position, true);
				audioSource.minDistance = 4f;
			}
			InWater = true;
			throwableObjectBody.isKinematic = true;
			throwableObject.position = new Vector3(throwableObject.position.x, waterLevel, throwableObject.position.z);
		}
		CanvasHelper.singleton.SetRemoveBlock(CanThrow && chargeMeter.ChargeNormal > 0f);
		CanvasHelper.singleton.SetRemoveBlock(GetChargeNormal());
	}

	public void ResetCanThrow()
	{
		CanThrow = true;
		PlayerItemManager.IsBusy = false;
	}

	public void PickupThrowable()
	{
		if (InWater)
		{
			throwableObject.parent = throwableObjectParent;
			throwableObject.localPosition = throwableStartPosition;
			throwableObject.localEulerAngles = throwableStartRotation;
			throwableObject.localScale = throwableStartLocalScale;
			InHand = true;
			InWater = false;
		}
	}

	public float GetChargeNormal()
	{
		return chargeMeter.ChargeNormal;
	}

	public void SetPosition(Vector3 position)
	{
		throwableObject.position = position;
	}

	public Vector3 GetPositon()
	{
		return throwableObject.position;
	}

	public void SubscribeToWaterHitEvent(ThrowableDelegate method)
	{
		waterHitCallstack = (ThrowableDelegate)Delegate.Combine(waterHitCallstack, method);
	}

	public void SubscribeToThrowEvent(ThrowableDelegate method)
	{
		throwCallStack = (ThrowableDelegate)Delegate.Combine(throwCallStack, method);
	}
}
