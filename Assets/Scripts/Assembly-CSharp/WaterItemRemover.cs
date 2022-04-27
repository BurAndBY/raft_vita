using UnityEngine;

public class WaterItemRemover : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (((1 << other.transform.gameObject.layer) & (int)LayerMasks.MASK_item) == 0)
		{
			return;
		}
		PickupItem component = other.transform.GetComponent<PickupItem>();
		if (component != null)
		{
			Collider component2 = component.GetComponent<Collider>();
			if (component2 != null)
			{
				component2.enabled = true;
			}
			PoolManager.singleton.ReturnToPool(component.iItem.index, component.gameObject);
		}
	}
}
