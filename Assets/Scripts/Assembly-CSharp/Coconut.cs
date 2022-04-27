using UnityEngine;

public class Coconut : MonoBehaviour
{
	private Rigidbody body;

	private PickupItem pickup;

	private void Start()
	{
		body = GetComponent<Rigidbody>();
		pickup = GetComponent<PickupItem>();
		body.isKinematic = true;
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, 2.7f, base.transform.localPosition.z);
	}

	private void Update()
	{
	}

	public void DropFromTree()
	{
		body.isKinematic = false;
		pickup.canBePickedUp = true;
	}
}
