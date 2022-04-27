using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : Entity
{
	[Header("Shark settings")]
	public float waterHeight;

	public float diveHeight = -30f;

	public float minDistanceToSurface = 40f;

	public float maxDistanceToSurface = 60f;

	public int requiredHitsMin = 3;

	public int requiredHitsMax = 6;

	[Header("Swim settings")]
	public float swimSpeed;

	public float rotationSpeed;

	[Header("Attack settings")]
	public float searchBlockInterval;

	public float biteRange;

	public int biteDamage;

	public float biteInterval;

	public float biteOffsetPositionY;

	[Header("Harvest yield")]
	public List<IItem> yield = new List<IItem>();

	[Space(10f)]
	[Header("Components")]
	public Shark sharkPrefab;

	public Animator animator;

	public ParticleController woodParticles;

	public ParticleSystem waterSplash;

	private BlockPlacer blockPlacer;

	private Vector3 circulatePoint;

	private Block targetBlock;

	private bool diveComplete;

	private bool eating;

	private int hitByPlayerCount;

	private int requiredHits;

	private PersonController player;

	public float searchForNewBlockAccumulator;

	private AudioSource sharkAttackSource;

	private AudioSource sharkBeginSource;

	protected override void Start()
	{
		base.Start();
		blockPlacer = Object.FindObjectOfType<BlockPlacer>();
		player = Object.FindObjectOfType<PersonController>();
		StartCoroutine(Dive());
	}

	private void Update()
	{
		if (IsDead())
		{
			return;
		}
		if (player != null && player.inWater && !eating)
		{
			CancelInvoke();
			Vector3 position = player.transform.position;
			position.y = base.transform.position.y;
			MoveTowardsDirection(position - base.transform.position);
			float num = Vector3.Distance(player.transform.position, base.transform.position);
			if (num <= 3f)
			{
				animator.SetBool("BitingPlayer", true);
				player.transform.GetComponent<PlayerStats>().AddHealth(-0.25f);
			}
			else
			{
				animator.SetBool("BitingPlayer", false);
			}
			return;
		}
		if (animator.GetBool("BitingPlayer"))
		{
			animator.SetBool("BitingPlayer", false);
		}
		if (targetBlock == null)
		{
			if (diveComplete)
			{
				diveComplete = false;
				base.transform.position = GetRandomWaterPosition();
				Vector3 forward = Vector3.Cross((Vector3.zero - base.transform.position).normalized, Vector3.up);
				base.transform.rotation = Quaternion.LookRotation(forward);
				SetNewCirculatePoint();
			}
			else
			{
				searchForNewBlockAccumulator += Time.deltaTime;
				if (searchForNewBlockAccumulator >= searchBlockInterval)
				{
					searchForNewBlockAccumulator -= searchBlockInterval;
					if (FindNewBlockToAttack())
					{
						return;
					}
				}
				Vector3 targetDirection = circulatePoint - base.transform.position;
				MoveTowardsDirection(targetDirection);
				float num2 = Vector3.Distance(base.transform.position, circulatePoint);
				if (num2 <= 5f)
				{
					SetNewCirculatePoint();
				}
			}
		}
		else
		{
			Vector3 directionToBlock = GetDirectionToBlock(targetBlock);
			if (InRangeToEat(targetBlock))
			{
				if (!eating)
				{
					eating = true;
					hitByPlayerCount = 0;
					requiredHits = Random.Range(requiredHitsMin, requiredHitsMax + 1);
					waterSplash.Play();
					InvokeRepeating("BiteBlock", 0f, biteInterval);
					sharkAttackSource = SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("SharkAttack", base.transform.position, false);
					base.transform.parent = GameManager.singleton.globalRaftParent;
				}
			}
			else
			{
				MoveTowardsDirection(directionToBlock);
			}
		}
		if (eating && targetBlock != null)
		{
			Vector3 position2 = base.transform.position;
			position2.y = targetBlock.transform.position.y + biteOffsetPositionY;
			base.transform.position = position2;
		}
	}

	protected override void Die()
	{
		base.Die();
		animator.SetTrigger("Dead");
		Object.Instantiate(sharkPrefab, GetRandomWaterPosition(), sharkPrefab.transform.rotation);
		StopAllCoroutines();
		CancelInvoke();
		waterSplash.Stop();
		woodParticles.StopParticles();
		if (sharkAttackSource != null)
		{
			Object.Destroy(sharkAttackSource.gameObject);
			sharkAttackSource = null;
		}
	}

	public override void Damage(int damage)
	{
		base.Damage(damage);
		if (!IsDead())
		{
			if (hitByPlayerCount < requiredHits)
			{
				hitByPlayerCount++;
				if (hitByPlayerCount >= requiredHits)
				{
					StopBiting();
				}
			}
		}
		else if (yield.Count > 0)
		{
			PlayerInventory.Singleton.AddItem(yield[0], 1);
			yield.RemoveAt(0);
			if (yield.Count <= 0)
			{
				StartCoroutine(Dive());
			}
		}
	}

	public void StopBiting()
	{
		CancelInvoke("BiteBlock");
		eating = false;
		targetBlock = null;
		StartCoroutine(Dive());
		woodParticles.StopParticles();
		waterSplash.Stop();
		if (sharkAttackSource != null)
		{
			Object.Destroy(sharkAttackSource.gameObject);
			sharkAttackSource = null;
		}
		if (sharkBeginSource != null)
		{
			Object.Destroy(sharkBeginSource.gameObject);
			sharkBeginSource = null;
		}
		animator.SetBool("IsBiting", false);
		base.transform.parent = null;
	}

	private IEnumerator Dive()
	{
		diveComplete = false;
		while (base.transform.position.y > diveHeight)
		{
			searchForNewBlockAccumulator = 0f;
			if (!IsDead())
			{
				base.transform.position += Vector3.down * swimSpeed * Time.deltaTime * 0.4f;
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(Vector3.down), Time.deltaTime * rotationSpeed);
			}
			else
			{
				base.transform.position += Vector3.down * swimSpeed * Time.deltaTime * 0.2f;
			}
			yield return null;
		}
		diveComplete = true;
		hitByPlayerCount = 0;
		if (IsDead())
		{
			Object.Destroy(base.gameObject);
		}
	}

	private bool FindNewBlockToAttack()
	{
		targetBlock = GetTargetBlock();
		if (targetBlock != null)
		{
			CancelInvoke("FindNewBlockToAttack");
			return true;
		}
		Debug.Log("Could not find block to attack");
		return false;
	}

	private Vector3 GetRandomWaterPosition()
	{
		float x = Random.Range(minDistanceToSurface, maxDistanceToSurface) * (float)((Random.Range(0, 2) != 0) ? 1 : (-1));
		float z = Random.Range(minDistanceToSurface, maxDistanceToSurface) * (float)((Random.Range(0, 2) != 0) ? 1 : (-1));
		return new Vector3(x, waterHeight, z);
	}

	private void BiteBlock()
	{
		bool isDestroyed = false;
		if (targetBlock != null)
		{
			woodParticles.PlayParticles();
			targetBlock.Damage(biteDamage, ref isDestroyed);
		}
		else
		{
			isDestroyed = true;
		}
		if (isDestroyed)
		{
			StopBiting();
		}
	}

	private Vector3 GetDirectionToBlock(Block block)
	{
		Vector3 result = block.transform.position - base.transform.position;
		result.y = 0f;
		result.Normalize();
		return result;
	}

	private void MoveTowardsDirection(Vector3 targetDirection)
	{
		Quaternion to = Quaternion.LookRotation(targetDirection);
		Quaternion from = Quaternion.LookRotation(base.transform.forward);
		float num = ((!animator.GetBool("BitingPlayer")) ? swimSpeed : (swimSpeed * 0.15f));
		base.transform.position += base.transform.forward * num * Time.deltaTime;
		base.transform.rotation = Quaternion.RotateTowards(from, to, Time.deltaTime * rotationSpeed);
	}

	private bool InRangeToEat(Block block)
	{
		Vector3 position = targetBlock.transform.position;
		Vector3 position2 = base.transform.position;
		position2.y = (position.y = 0f);
		float num = Vector3.Distance(position2, position);
		if (sharkBeginSource == null && num <= biteRange + 1.5f)
		{
			sharkBeginSource = SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("SharkBegin", base.transform.position, false);
			animator.SetBool("IsBiting", true);
		}
		if (num <= biteRange)
		{
			return true;
		}
		return false;
	}

	private Block GetTargetBlock()
	{
		List<Block> allPlacedBlocks = blockPlacer.allPlacedBlocks;
		List<Block> list = new List<Block>();
		for (int i = 0; i < allPlacedBlocks.Count; i++)
		{
			if (allPlacedBlocks[i].type == BlockType.Foundation)
			{
				list.Add(allPlacedBlocks[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Block block = list[Random.Range(0, list.Count)];
		Vector3 direction = block.transform.position - base.transform.position;
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position, direction, out hitInfo, float.MaxValue, LayerMasks.MASK_block))
		{
			Block componentInParent = hitInfo.transform.GetComponentInParent<Block>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	private void SetNewCirculatePoint()
	{
		Vector3 randomWaterPosition = GetRandomWaterPosition();
		Vector3 direction = randomWaterPosition - base.transform.position;
		RaycastHit hitInfo;
		while (Physics.BoxCast(base.transform.position, Vector3.one * 3f, direction, out hitInfo, Quaternion.identity, float.MaxValue, LayerMasks.MASK_block))
		{
			randomWaterPosition = GetRandomWaterPosition();
			direction = randomWaterPosition - base.transform.position;
		}
		circulatePoint = randomWaterPosition;
	}
}
