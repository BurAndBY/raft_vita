using UnityEngine;

namespace PlayWay.Water
{
	public class WaterFloat : MonoBehaviour
	{
		public enum DisplacementMode
		{
			Height,
			Displacement
		}

		[SerializeField]
		private Water water;

		[SerializeField]
		private float heightBonus;

		[SerializeField]
		[Range(0.04f, 1f)]
		private float precision = 0.2f;

		[SerializeField]
		private DisplacementMode displacementMode = DisplacementMode.Displacement;

		private WaterSample sample;

		private Vector3 initialPosition;

		private Vector3 previousPosition;

		private void Start()
		{
			initialPosition = base.transform.position;
			previousPosition = initialPosition;
			if (water == null)
			{
				water = Object.FindObjectOfType<Water>();
			}
			sample = new WaterSample(water, (WaterSample.DisplacementMode)displacementMode, precision);
		}

		private void OnDisable()
		{
			if (sample != null)
			{
				sample.Stop();
			}
		}

		private void LateUpdate()
		{
			initialPosition += base.transform.position - previousPosition;
			Vector3 andReset = sample.GetAndReset(initialPosition.x, initialPosition.z, WaterSample.ComputationsMode.ForceCompletion);
			andReset.y += heightBonus;
			base.transform.position = andReset;
			previousPosition = andReset;
		}
	}
}
