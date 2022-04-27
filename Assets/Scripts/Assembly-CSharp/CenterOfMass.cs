using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
	private void OnEnable()
	{
		Rigidbody componentInParent = GetComponentInParent<Rigidbody>();
		if (componentInParent != null)
		{
			componentInParent.centerOfMass = componentInParent.transform.worldToLocalMatrix.MultiplyPoint3x4(base.transform.position);
		}
	}
}
