using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StableComponent))]
[RequireComponent(typeof(OccupyingComponent))]
public class Block : MonoBehaviour
{
	public static int GlobalIndex;

	public int blockIndex;

	[Header("Settings")]
	public BlockType type;

	public ItemIndex itemIndex = ItemIndex.None;

	public GameObject prefab;

	public bool specialySaved;

	public bool showHealthOnMouseOver;

	public int health = 100;

	public int maxHealth = 100;

	[Header("Build settings")]
	public bool isRotateable;

	public bool canRotateFreely;

	public bool removeableWithAxe;

	public bool snapsToQuads = true;

	public bool snapsToTopCollider;

	public Vector3 buildOffset;

	public List<Cost> buildCost = new List<Cost>();

	[Header("UI Settings")]
	[Space(20f)]
	public Sprite sprite;

	public string displayName;

	[TextArea]
	public string description;

	[Header("On/Off colliders... Off when placing")]
	public Collider[] onoffColliders;

	[HideInInspector]
	[Space(20f)]
	public OccupyingComponent overlappingComponent;

	private StableComponent stabilityComponent;

	[Header("Overlapping check")]
	public List<BlockQuad> overlappChecks;

	[Header("Stability")]
	public bool requireAll;

	public int requireHitCount = 1;

	private static BlockPlacer blockPlacer;

	private CanvasHelper canvas;

	private void Awake()
	{
		if (blockPlacer == null)
		{
			blockPlacer = Object.FindObjectOfType<BlockPlacer>();
		}
		overlappingComponent = GetComponent<OccupyingComponent>();
		stabilityComponent = GetComponent<StableComponent>();
	}

	private void Start()
	{
		canvas = CanvasHelper.singleton;
	}

	private void OnMouseOver()
	{
		if (!(blockPlacer == null) && showHealthOnMouseOver && blockPlacer.enabled)
		{
			if (GameManager.IsInMenu)
			{
				canvas.SetDisplayText(false);
			}
			else if (canvas != null)
			{
				canvas.SetDisplayText(displayName + "\nHealth: " + health + "/" + maxHealth);
			}
		}
	}

	private void OnMouseExit()
	{
		if (showHealthOnMouseOver)
		{
			canvas.SetDisplayText(false);
		}
	}

	private void OnDestroy()
	{
		if (canvas != null)
		{
			canvas.SetDisplayText(false);
		}
	}

	private void OnValidate()
	{
		if (stabilityComponent == null)
		{
			stabilityComponent = GetComponent<StableComponent>();
			return;
		}
		if (requireAll)
		{
			requireHitCount = stabilityComponent.requiredColliders.Length;
		}
		if (requireHitCount > stabilityComponent.requiredColliders.Length)
		{
			requireHitCount = stabilityComponent.requiredColliders.Length;
		}
		if (requireHitCount < 1)
		{
			requireHitCount = 1;
		}
	}

	public void OnStartingPlacement()
	{
		Collider[] array = onoffColliders;
		foreach (Collider collider in array)
		{
			collider.enabled = false;
		}
		foreach (BlockQuad overlappCheck in overlappChecks)
		{
			overlappCheck.gameObject.SetActive(false);
		}
		overlappingComponent.LookForOverlapps(true);
	}

	public void OnFinishedPlacement()
	{
		Collider[] array = onoffColliders;
		foreach (Collider collider in array)
		{
			collider.enabled = true;
		}
		GlobalIndex++;
		blockIndex = GlobalIndex;
		overlappingComponent.LookForOverlapps(false);
	}

	public void Damage(int damage, ref bool isDestroyed)
	{
		health -= damage;
		if (health <= 0)
		{
			isDestroyed = true;
			blockPlacer.RemoveBlock(this);
		}
		else
		{
			isDestroyed = false;
		}
	}

	public bool IsStable()
	{
		if (stabilityComponent == null)
		{
			return true;
		}
		int overlapCount = stabilityComponent.GetOverlapCount();
		if (requireAll)
		{
			return overlapCount == stabilityComponent.requiredColliders.Length;
		}
		return overlapCount >= requireHitCount;
	}

	public void Repair(int amount)
	{
		health += amount;
		if (health > maxHealth)
		{
			health = maxHealth;
		}
	}

	public bool CanBeRepaired()
	{
		return health < maxHealth;
	}

	public void RefreshOverlapps()
	{
		for (int i = 0; i < overlappChecks.Count; i++)
		{
			BlockQuad blockQuad = overlappChecks[i];
			Collider component = blockQuad.GetComponent<Collider>();
			Vector3 halfExtents = Helper.GetColliderExtents(component) * 0.99f;
			Vector3 colliderCenter = Helper.GetColliderCenter(component);
			Collider[] array = Physics.OverlapBox(colliderCenter, halfExtents, Quaternion.identity, LayerMasks.MASK_buildQuad);
			bool flag = false;
			if (array.Length > 0)
			{
				Collider[] array2 = array;
				foreach (Collider collider in array2)
				{
					if (collider != component)
					{
						flag = true;
						break;
					}
				}
			}
			blockQuad.gameObject.SetActive(!flag);
		}
	}
}
