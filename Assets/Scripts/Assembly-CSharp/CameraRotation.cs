using UnityEngine;

public class CameraRotation : MonoBehaviour
{
	private Vector3 v3;

	public float speed = 100f;

	private void Start()
	{
		v3 = base.transform.localEulerAngles;
		Application.targetFrameRate = 60;
	}

	private void LateUpdate()
	{
		if (Input.GetMouseButton(1))
		{
			v3.x -= Input.GetAxis("Mouse Y") * speed * Time.deltaTime;
			v3.y += Input.GetAxis("Mouse X") * speed * Time.deltaTime;
		}
		v3 = clamp(v3);
		base.transform.localEulerAngles = v3;
	}

	private Vector3 clamp(Vector3 v3)
	{
		if (v3.x > 360f)
		{
			v3.x -= 360f;
		}
		if (v3.x < -360f)
		{
			v3.x += 360f;
		}
		if (v3.y > 360f)
		{
			v3.y -= 360f;
		}
		if (v3.y < -360f)
		{
			v3.y += 360f;
		}
		return v3;
	}
}
