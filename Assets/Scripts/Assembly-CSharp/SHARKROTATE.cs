using UnityEngine;

public class SHARKROTATE : MonoBehaviour
{
	public float rotateSpeed = 1f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
	}
}
