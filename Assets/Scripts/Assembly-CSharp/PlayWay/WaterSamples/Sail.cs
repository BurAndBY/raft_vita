using UnityEngine;

namespace PlayWay.WaterSamples
{
	public class Sail : MonoBehaviour
	{
		[SerializeField]
		private WindZone wind;

		[SerializeField]
		private float area = 100f;

		private Rigidbody rigidBody;

		private void Awake()
		{
			rigidBody = GetComponentInParent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			float num = Vector3.Dot(base.transform.forward, wind.transform.forward);
			if (num >= 0f)
			{
				num = 2f - num;
			}
			if (num < 0f)
			{
				num = 2f + num * 2f;
			}
			rigidBody.AddForce(base.transform.forward * (wind.windMain * num * area), ForceMode.Force);
		}
	}
}
