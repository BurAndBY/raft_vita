using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Eatable", order = 1)]
public class IEdible : IUsableItem
{
	[Header("Eatable settings")]
	public float hungerYield;

	public float thirstYield;

	public float healthYield;

	public float useCooldown = 0.1f;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
