using UnityEngine;

public class RandomDropper : MonoBehaviour
{
	public RandomDropItem[] items;

	public ItemIndex GetItemToSpawn()
	{
		float num = 0f;
		RandomDropItem[] array = items;
		foreach (RandomDropItem randomDropItem in array)
		{
			num += randomDropItem.spawnChance;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		RandomDropItem[] array2 = items;
		foreach (RandomDropItem randomDropItem2 in array2)
		{
			num3 += randomDropItem2.spawnChance;
			if (num2 < num3)
			{
				return randomDropItem2.index;
			}
		}
		return ItemIndex.None;
	}
}
