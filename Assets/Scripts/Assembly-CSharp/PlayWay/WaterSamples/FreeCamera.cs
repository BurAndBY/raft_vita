using UnityEngine;

namespace PlayWay.WaterSamples
{
	public class FreeCamera : MonoBehaviour
	{
		[SerializeField]
		private float speed = 20f;

		[SerializeField]
		private float mouseSensitivity = 2f;

		private Camera localCamera;

		private void Awake()
		{
			localCamera = GetComponent<Camera>();
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.W))
			{
				base.transform.position += base.transform.forward * speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.S))
			{
				base.transform.position -= base.transform.forward * speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.A))
			{
				base.transform.position -= base.transform.right * speed * Time.deltaTime;
			}
			if (Input.GetKey(KeyCode.D))
			{
				base.transform.position += base.transform.right * speed * Time.deltaTime;
			}
			if (Input.GetMouseButton(1))
			{
				base.transform.localEulerAngles += new Vector3((0f - Input.GetAxisRaw("Mouse Y")) * mouseSensitivity, 0f, 0f);
				base.transform.localEulerAngles += new Vector3(0f, Input.GetAxisRaw("Mouse X") * mouseSensitivity, 0f);
			}
			localCamera.farClipPlane = Mathf.Max(4000f, 2000f + base.transform.position.y * 40f);
			localCamera.nearClipPlane = localCamera.farClipPlane * 0.00025f;
		}
	}
}
