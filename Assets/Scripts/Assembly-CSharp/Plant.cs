using System;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
	public delegate void OnPlantRemoved(PlantationSlot slot);

	private OnPlantRemoved onRemovedCallstack;

	public ItemIndex plantIndex;

	[Tooltip("In seconds")]
	[Header("Grow settings")]
	public float growTime = 1f;

	public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

	public Vector3 maxScale = Vector3.one;

	[Header("Harvest settings")]
	public bool harvestable = true;

	public List<Cost> yieldItems = new List<Cost>();

	[HideInInspector]
	public PlantationSlot slotPlantIsOn;

	private float growTimer;

	private bool fullyGrown;

	private CanvasHelper canvas;

	private void Awake()
	{
		ResetStats();
	}

	private void Start()
	{
		canvas = CanvasHelper.singleton;
	}

	private void Update()
	{
		float num = Mathf.Clamp(growTimer / growTime, 0f, 1f);
		Vector3 target = minScale + (maxScale - minScale) * num;
		base.transform.localScale = Vector3.MoveTowards(base.transform.localScale, target, Time.deltaTime * 20f);
		if (!fullyGrown)
		{
			growTimer += Time.deltaTime;
			if (growTimer >= growTime)
			{
				Complete();
			}
		}
	}

	private void OnMouseOver()
	{
		if (GameManager.IsInMenu || !FullyGrown() || !harvestable || !Player.IsWithingDistance(base.transform.position))
		{
			return;
		}
		canvas.SetDisplayText("Harvest", true);
		if (Input.GetButtonDown("UseButton"))
		{
			Cost yieldItem = GetYieldItem();
			if (PlayerInventory.Singleton.AddItem(yieldItem.item, yieldItem.amount).succeded)
			{
				Pickup();
			}
		}
	}

	private void OnMouseExit()
	{
		canvas.SetDisplayText(false);
	}

	public void SubscribeToOnPlantRemove(OnPlantRemoved method)
	{
		onRemovedCallstack = (OnPlantRemoved)Delegate.Combine(onRemovedCallstack, method);
	}

	public bool FullyGrown()
	{
		return fullyGrown;
	}

	public void Pickup()
	{
		if (onRemovedCallstack != null)
		{
			onRemovedCallstack(slotPlantIsOn);
		}
		slotPlantIsOn = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public Cost GetYieldItem()
	{
		if (YieldLeft() > 0)
		{
			Cost cost = yieldItems[UnityEngine.Random.Range(0, yieldItems.Count)];
			yieldItems.Remove(cost);
			return cost;
		}
		return null;
	}

	public int YieldLeft()
	{
		return yieldItems.Count;
	}

	public void SetYield(List<Cost> newYields)
	{
		yieldItems.Clear();
		yieldItems.AddRange(newYields);
	}

	public void SetGrowTimer(float value)
	{
		growTimer = value;
		if (growTimer < growTime)
		{
			fullyGrown = false;
		}
	}

	public float GetGrowTimer()
	{
		return growTimer;
	}

	private void ResetStats()
	{
		growTimer = 0f;
		fullyGrown = false;
		base.transform.localScale = minScale;
	}

	private void Complete()
	{
		fullyGrown = true;
	}
}
