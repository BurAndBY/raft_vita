using System.Collections.Generic;
using UnityEngine;

public class Bobber : MonoBehaviour
{
	[Range(0f, 100f)]
	public float minWaitTimeSeconds = 5f;

	[Range(0f, 100f)]
	public float maxWaitTimeSeconds = 10f;

	[Range(0f, 10f)]
	public float fishOnHookTime = 5f;

	[Header("Bobb settings")]
	public float frequency = 20f;

	public float magnitude = 0.5f;

	public float movementSpeed = 5f;

	public Vector3 waterOffset;

	[Space]
	public List<FishingItem> fishingItems = new List<FishingItem>();

	private bool active;

	private bool fishIsOnHook;

	private bool escaped;

	private Vector3 startPos;

	private GameObject reelsource;

	private void Update()
	{
		if (active)
		{
			if (fishIsOnHook)
			{
				base.transform.position -= Vector3.up * Time.deltaTime;
				return;
			}
			startPos.x = base.transform.position.x;
			startPos.z = base.transform.position.z;
			base.transform.position = startPos + waterOffset + Vector3.up * Mathf.Sin(Time.time * frequency) * magnitude;
		}
	}

	private void OnValidate()
	{
		if (minWaitTimeSeconds >= maxWaitTimeSeconds - 1f)
		{
			minWaitTimeSeconds = maxWaitTimeSeconds - 1f;
		}
		if (maxWaitTimeSeconds <= minWaitTimeSeconds + 1f)
		{
			maxWaitTimeSeconds = minWaitTimeSeconds + 1f;
		}
	}

	public IItem TryToGetFish()
	{
		if (!active || !fishIsOnHook)
		{
			return null;
		}
		float spawnChance = Random.Range(0f, 100f);
		if (reelsource != null)
		{
			Object.Destroy(reelsource);
		}
		return GetItemFromSea(spawnChance);
	}

	public void StartBobbing()
	{
		if (!active)
		{
			active = true;
			startPos = base.transform.position;
			float time = Random.Range(minWaitTimeSeconds, maxWaitTimeSeconds);
			Invoke("FishOnHook", time);
		}
	}

	public void StopBobbing()
	{
		active = false;
		fishIsOnHook = false;
		escaped = false;
		CancelInvoke();
	}

	public bool Escaped()
	{
		return escaped;
	}

	public bool FishIsOnHook()
	{
		return fishIsOnHook;
	}

	private void FishOnHook()
	{
		fishIsOnHook = true;
		base.transform.localScale *= 1.25f;
		SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("FishOnHook", base.transform.position, true);
		reelsource = SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("Reel", base.transform.position, false).gameObject;
		Invoke("FishEscaped", fishOnHookTime);
	}

	private void FishEscaped()
	{
		active = false;
		fishIsOnHook = false;
		escaped = true;
		if (reelsource != null)
		{
			Object.Destroy(reelsource);
		}
	}

	private IItem GetItemFromSea(float spawnChance)
	{
		float num = 0f;
		foreach (FishingItem fishingItem in fishingItems)
		{
			num += fishingItem.spawnChance;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (FishingItem fishingItem2 in fishingItems)
		{
			num3 += fishingItem2.spawnChance;
			if (num2 < num3)
			{
				return fishingItem2.item;
			}
		}
		return null;
	}
}
