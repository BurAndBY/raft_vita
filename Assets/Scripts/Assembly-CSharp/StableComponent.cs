using UnityEngine;

public class StableComponent : MonoBehaviour
{
	public Collider[] requiredColliders;

	public int GetOverlapCount()
	{
		int num = 0;
		Collider[] array = requiredColliders;
		foreach (Collider collider in array)
		{
			Vector3 halfExtents = Helper.GetColliderExtents(collider) * 0.99f;
			Vector3 colliderCenter = Helper.GetColliderCenter(collider);
			if (Physics.CheckBox(colliderCenter, halfExtents, collider.transform.rotation, LayerMasks.MASK_block))
			{
				num++;
			}
		}
		return num;
	}
}
