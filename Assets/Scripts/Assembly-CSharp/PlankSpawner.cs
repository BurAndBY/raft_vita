using UnityEngine;

public class PlankSpawner : MonoBehaviour
{
	public RandomDropper dropper;

	public float floatSpeed = 5f;

	[Header("Spawn settings")]
	public int spawnMinAmount;

	public int spawnMaxAmount;

	public float spawnIntervalSec;

	[Space(10f)]
	public float spawnWidth;

	public float spawnZ;

	private float spawnTimer;

	private void Start()
	{
		spawnTimer = 0f;
	}

	private void Update()
	{
		spawnTimer += Time.deltaTime;
		if (spawnTimer >= spawnIntervalSec)
		{
			spawnTimer -= spawnIntervalSec;
			SpawnItems();
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).position -= Vector3.forward * floatSpeed * Time.deltaTime;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(Vector3.up + Vector3.right * (0f - spawnWidth), 0.5f);
		Gizmos.DrawSphere(Vector3.up + Vector3.right * spawnWidth, 0.5f);
		Gizmos.DrawSphere(Vector3.zero + Vector3.forward * spawnZ, 0.5f);
	}

	private void SpawnItems()
	{
		Vector3 vector = new Vector3(Random.Range(0f - spawnWidth, spawnWidth), 0f, spawnZ);
		int num = Random.Range(spawnMinAmount, spawnMaxAmount);
		for (int i = 0; i < num; i++)
		{
			ItemIndex itemToSpawn = dropper.GetItemToSpawn();
			if (itemToSpawn != ItemIndex.None)
			{
				Vector3 position = vector + new Vector3(Random.Range(-3, 3), -0.45f, Random.Range(-3, 3));
				GameObject objectFromPool = PoolManager.singleton.GetObjectFromPool(itemToSpawn);
				if (objectFromPool != null)
				{
					objectFromPool.transform.position = position;
					objectFromPool.transform.rotation = Quaternion.Euler(Random.Range(-5, 5), Random.Range(0, 360), Random.Range(-5, 5));
					objectFromPool.transform.SetParent(base.transform);
				}
			}
		}
	}
}
