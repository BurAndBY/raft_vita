using UnityEngine;

public class LockRotation : MonoBehaviour
{
	public bool lockX;

	public bool lockY;

	public bool lockZ;

	[Space(10f)]
	public Vector3 lockrotation;

	private void Start()
	{
	}

	private void Update()
	{
		if (!lockX && !lockY && !lockZ)
		{
			base.enabled = false;
			return;
		}
		Vector3 eulerAngles = base.transform.eulerAngles;
		if (lockX)
		{
			eulerAngles.x = lockrotation.x;
		}
		if (lockY)
		{
			eulerAngles.y = lockrotation.y;
		}
		if (lockZ)
		{
			eulerAngles.z = lockrotation.z;
		}
		base.transform.eulerAngles = eulerAngles;
	}
}
