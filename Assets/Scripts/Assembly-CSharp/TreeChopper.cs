using UnityEngine;

public class TreeChopper : MonoBehaviour
{
	[HideInInspector]
	public Plant currentPlant;

	public ParticleController woodParticles;

	private RaycastHit hit;

	private void Start()
	{
		woodParticles.particleParent.parent = null;
	}

	public void OnHammerHit()
	{
		//Discarded unreachable code: IL_0165
		if (currentPlant == null)
		{
			return;
		}
		if (hit.transform != null)
		{
			woodParticles.SetPosition(hit.point);
			woodParticles.SetLookRotation(hit.normal);
			woodParticles.PlayParticles();
			SingletonGeneric<SoundManager>.Singleton.PlaySound("WoodHit", base.transform.position);
		}
		Cost yieldItem = currentPlant.GetYieldItem();
		PlayerInventory.Singleton.AddItem(yieldItem.item, yieldItem.amount);
		if (currentPlant.YieldLeft() == 0)
		{
			Coconut[] componentsInChildren = currentPlant.transform.GetComponentsInChildren<Coconut>();
			Coconut[] array = componentsInChildren;
			foreach (Coconut coconut in array)
			{
				coconut.transform.SetParent(GameManager.singleton.globalRaftParent);
				coconut.DropFromTree();
			}
			currentPlant.Pickup();
		}
		else if (Random.Range(0, 4) == 0)
		{
			Coconut[] componentsInChildren2 = currentPlant.transform.GetComponentsInChildren<Coconut>();
			Coconut[] array2 = componentsInChildren2;
			int num = 0;
			if (num < array2.Length)
			{
				Coconut coconut2 = array2[num];
				coconut2.transform.SetParent(GameManager.singleton.globalRaftParent);
				coconut2.DropFromTree();
			}
		}
	}

	public void UpdateOwn(RaycastHit hit)
	{
		this.hit = hit;
	}

	public void Select()
	{
	}

	public void DeSelect()
	{
		currentPlant = null;
	}
}
