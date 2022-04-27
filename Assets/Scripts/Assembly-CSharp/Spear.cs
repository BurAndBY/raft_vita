using UnityEngine;

public class Spear : Weapon
{
	[Header("Spear settings")]
	public LayerMask attackMask;

	public float attackRange = 3f;

	[SerializeField]
	[Header("Feedback settings")]
	public ParticleController bloodParticles;

	protected override void Start()
	{
		base.Start();
		bloodParticles.particleParent.parent = null;
	}

	protected override void OnWeaponUse()
	{
		base.OnWeaponUse();
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, attackRange, attackMask))
		{
			Entity componentInParent = hitInfo.transform.GetComponentInParent<Entity>();
			if (componentInParent != null)
			{
				componentInParent.Damage(damage);
				bloodParticles.particleParent.rotation = Quaternion.LookRotation(hitInfo.normal);
				bloodParticles.SetPosition(hitInfo.point);
				bloodParticles.PlayParticles();
				SingletonGeneric<SoundManager>.Singleton.PlaySoundCopy("SpearHit", hitInfo.point, true);
			}
		}
	}

	public void OnSelect()
	{
		PlayerAnimator.SetAnimation(PlayerAnimation.Index_6_Spear);
	}

	public void OnDeSelect()
	{
	}
}
