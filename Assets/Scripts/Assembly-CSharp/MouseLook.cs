using UnityEngine;

public class MouseLook : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}

	public RotationAxes axes;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	private float rotationX;

	private float rotationY;

	private Quaternion originalRotation;

	private void Start()
	{
		originalRotation = base.transform.localRotation;
	}

	private void Update()
	{
		if (!GameManager.IsInMenu)
		{
			if (axes == RotationAxes.MouseXAndY)
			{
				rotationX += Input.GetAxis("Mouse X") * SingletonGeneric<Settings>.Singleton.MouseSensitivty;
				rotationY += Input.GetAxis("Mouse Y") * SingletonGeneric<Settings>.Singleton.MouseSensitivty;
				rotationX = ClampAngle(rotationX, minimumX, maximumX);
				rotationY = ClampAngle(rotationY, minimumY, maximumY);
				Quaternion quaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
				Quaternion quaternion2 = Quaternion.AngleAxis(rotationY, -Vector3.right);
				base.transform.localRotation = originalRotation * quaternion * quaternion2;
			}
			else if (axes == RotationAxes.MouseX)
			{
				rotationX += Input.GetAxis("Mouse X") * SingletonGeneric<Settings>.Singleton.MouseSensitivty;
				rotationX = ClampAngle(rotationX, minimumX, maximumX);
				Quaternion quaternion3 = Quaternion.AngleAxis(rotationX, Vector3.up);
				base.transform.localRotation = originalRotation * quaternion3;
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * SingletonGeneric<Settings>.Singleton.MouseSensitivty * (float)((!SingletonGeneric<Settings>.Singleton.InvertYAxis) ? 1 : (-1));
				rotationY = ClampAngle(rotationY, minimumY, maximumY);
				Quaternion quaternion4 = Quaternion.AngleAxis(0f - rotationY, Vector3.right);
				base.transform.localRotation = originalRotation * quaternion4;
			}
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
