using UnityEngine;

public class CollisionTest : MonoBehaviour
{
	public Material mat;

	private void Start()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		mat.color = Color.red;
	}

	private void OnTriggerExit(Collider other)
	{
		mat.color = Color.green;
	}
}
