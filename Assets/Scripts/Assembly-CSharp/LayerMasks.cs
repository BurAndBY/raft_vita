using UnityEngine;

public class LayerMasks : MonoBehaviour
{
	public LayerMask ignoreMask;

	public static LayerMask MASK_water;

	public static LayerMask MASK_item;

	public static LayerMask MASK_ignorePlayer;

	public static LayerMask MASK_block;

	public static LayerMask MASK_buildQuad;

	public const string TAG_CookingStand = "CookingStand";

	public const string TAG_Plant = "Plant";

	private void Awake()
	{
		MASK_ignorePlayer = ignoreMask;
		MASK_water = 1 << LayerMask.NameToLayer("Water");
		MASK_item = 1 << LayerMask.NameToLayer("Item");
		MASK_block = 1 << LayerMask.NameToLayer("Block");
		MASK_buildQuad = 1 << LayerMask.NameToLayer("BuildQuad");
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
