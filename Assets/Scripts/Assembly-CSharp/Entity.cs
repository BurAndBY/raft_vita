using UnityEngine;

public class Entity : MonoBehaviour
{
	[Header("Entity settings")]
	[SerializeField]
	private int maxHealth = 100;

	[SerializeField]
	private int health;

	private bool isDead;

	protected virtual void Start()
	{
		Reset();
	}

	protected virtual void Die()
	{
		isDead = true;
	}

	protected bool IsDead()
	{
		return isDead;
	}

	public virtual void Damage(int damage)
	{
		if (!isDead)
		{
			health -= damage;
			if (health <= 0)
			{
				Die();
			}
		}
	}

	private void Reset()
	{
		health = maxHealth;
	}
}
