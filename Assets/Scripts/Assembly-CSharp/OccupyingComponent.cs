using UnityEngine;

public class OccupyingComponent : MonoBehaviour
{
	public MeshRenderer[] renderers;

	public AdvancedCollision[] advancedCollisions;

	private Material[] originalMaterials;

	private void Awake()
	{
		if (renderers.Length > 0)
		{
			originalMaterials = new Material[renderers.Length];
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			originalMaterials[i] = renderers[i].material;
		}
		advancedCollisions = base.transform.GetComponentsInChildren<AdvancedCollision>();
		renderers = base.transform.GetComponentsInChildren<MeshRenderer>();
	}

	public void RestoreToDefaultMaterial()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material = originalMaterials[i];
		}
	}

	public void SetNewMaterial(Material material)
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material = material;
		}
	}

	public bool IsCollidingWithOtherBlocks()
	{
		AdvancedCollision[] array = advancedCollisions;
		foreach (AdvancedCollision advancedCollision in array)
		{
			if (advancedCollision.Overlapping)
			{
				return true;
			}
		}
		return false;
	}

	public void LookForOverlapps(bool lookForOverlapps)
	{
		AdvancedCollision[] array = advancedCollisions;
		foreach (AdvancedCollision advancedCollision in array)
		{
			advancedCollision.LookForOverlapps(lookForOverlapps);
		}
	}
}
