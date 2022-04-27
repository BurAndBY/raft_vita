using System.Collections.Generic;
using UnityEngine;

public class AdvancedCollision : MonoBehaviour
{
	public bool useAsPlayerCollision;

	[Header("For debugging only, dont set these")]
	[Space(20f)]
	public Collider[] colliders;

	public List<Collider> overlappingColliders = new List<Collider>();

	private bool overlapping;

	private Rigidbody body;

	public int OverlappingCount
	{
		get
		{
			return overlappingColliders.Count;
		}
	}

	public bool Overlapping
	{
		get
		{
			return overlapping;
		}
		private set
		{
			overlapping = value;
		}
	}

	private void Awake()
	{
		colliders = GetComponents<Collider>();
		SetCollidersToTrigger(true);
	}

	private void OnTriggerStay(Collider other)
	{
		if (((1 << other.transform.gameObject.layer) & (int)LayerMasks.MASK_block) != 0)
		{
			if (!overlappingColliders.Contains(other))
			{
				overlappingColliders.Add(other);
			}
			Overlapping = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (((1 << other.transform.gameObject.layer) & (int)LayerMasks.MASK_block) != 0)
		{
			if (overlappingColliders.Contains(other))
			{
				overlappingColliders.Remove(other);
			}
			Overlapping = false;
		}
	}

	private void OnDisable()
	{
		Overlapping = false;
		overlappingColliders.Clear();
	}

	public void LookForOverlapps(bool lookForOverlapps)
	{
		if (lookForOverlapps)
		{
			AddRigidbody();
			SetCollidersToTrigger(true);
			return;
		}
		RemoveRigidbody();
		if (useAsPlayerCollision)
		{
			SetCollidersToTrigger(false);
		}
	}

	private void SetCollidersToTrigger(bool state)
	{
		Collider[] array = colliders;
		foreach (Collider collider in array)
		{
			collider.isTrigger = state;
		}
	}

	private void AddRigidbody()
	{
		if (body == null)
		{
			body = base.gameObject.AddComponent<Rigidbody>();
			body.isKinematic = true;
		}
	}

	private void RemoveRigidbody()
	{
		if (body != null)
		{
			Object.Destroy(body);
		}
	}
}
