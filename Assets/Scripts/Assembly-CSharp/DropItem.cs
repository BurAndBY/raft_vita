using UnityEngine;

public class DropItem : MonoBehaviour
{
	public static float destroyTime = 20f;

	private float timer;

	private void Start()
	{
		ResetItem();
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			PoolManager.singleton.ReturnToPool(ItemIndex.DropItem, base.transform.gameObject);
		}
	}

	public void ResetItem()
	{
		timer = destroyTime;
	}
}
