using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PlayWay.Water
{
	public class WaterVolumeProbe : MonoBehaviour
	{
		[SerializeField]
		private UnityEvent enter;

		[SerializeField]
		private UnityEvent leave;

		private Water currentWater;

		private Transform target;

		private bool targetted;

		private WaterVolumeSubtract[] exclusions;

		private float size;

		public Water CurrentWater
		{
			get
			{
				return currentWater;
			}
		}

		public UnityEvent Enter
		{
			get
			{
				if (enter == null)
				{
					enter = new UnityEvent();
				}
				return enter;
			}
		}

		public UnityEvent Leave
		{
			get
			{
				if (leave == null)
				{
					leave = new UnityEvent();
				}
				return leave;
			}
		}

		private void Start()
		{
			ScanWaters();
		}

		private void FixedUpdate()
		{
			if (targetted)
			{
				if (target == null)
				{
					Object.Destroy(base.gameObject);
					return;
				}
				base.transform.position = target.position;
			}
			if (currentWater != null && currentWater.Volume.Boundless)
			{
				if (!currentWater.Volume.IsPointInsideMainVolume(base.transform.position) && !currentWater.Volume.IsPointInside(base.transform.position, exclusions, size))
				{
					LeaveCurrentWater();
				}
			}
			else if (currentWater == null)
			{
				ScanBoundlessWaters();
			}
		}

		private void OnDestroy()
		{
			currentWater = null;
			if (enter != null)
			{
				enter.RemoveAllListeners();
				enter = null;
			}
			if (leave != null)
			{
				leave.RemoveAllListeners();
				leave = null;
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			if (currentWater != null)
			{
				WaterVolumeSubtract component = other.GetComponent<WaterVolumeSubtract>();
				if (component != null)
				{
					LeaveCurrentWater();
				}
			}
			else
			{
				WaterVolumeAdd component2 = other.GetComponent<WaterVolumeAdd>();
				if (component2 != null)
				{
					EnterWater(component2.Water);
				}
			}
		}

		public void OnTriggerExit(Collider other)
		{
			if (currentWater == null)
			{
				WaterVolumeSubtract component = other.GetComponent<WaterVolumeSubtract>();
				if (component != null)
				{
					ScanWaters();
				}
			}
			else
			{
				WaterVolumeAdd component2 = other.GetComponent<WaterVolumeAdd>();
				if (component2 != null && component2.Water == currentWater)
				{
					LeaveCurrentWater();
				}
			}
		}

		[ContextMenu("Refresh Probe")]
		private void ScanWaters()
		{
			Vector3 position = base.transform.position;
			List<Water> waters = WaterGlobals.Instance.Waters;
			int count = waters.Count;
			for (int i = 0; i < count; i++)
			{
				if (waters[i].Volume.IsPointInside(position, exclusions, size))
				{
					EnterWater(waters[i]);
					return;
				}
			}
			LeaveCurrentWater();
		}

		private void ScanBoundlessWaters()
		{
			Vector3 position = base.transform.position;
			List<Water> boundlessWaters = WaterGlobals.Instance.BoundlessWaters;
			int count = boundlessWaters.Count;
			for (int i = 0; i < count; i++)
			{
				Water water = boundlessWaters[i];
				if (water.Volume.IsPointInsideMainVolume(position) && water.Volume.IsPointInside(position, exclusions, size))
				{
					EnterWater(water);
					break;
				}
			}
		}

		private void EnterWater(Water water)
		{
			if (!(currentWater == water))
			{
				if (currentWater != null)
				{
					LeaveCurrentWater();
				}
				currentWater = water;
				if (enter != null)
				{
					enter.Invoke();
				}
			}
		}

		private void LeaveCurrentWater()
		{
			if (currentWater != null)
			{
				if (leave != null)
				{
					leave.Invoke();
				}
				currentWater = null;
			}
		}

		public static WaterVolumeProbe CreateProbe(Transform target, float size = 0f)
		{
			GameObject gameObject = new GameObject("Water Volume Probe");
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			gameObject.transform.position = target.position;
			gameObject.layer = WaterProjectSettings.Instance.WaterCollidersLayer;
			SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius = size;
			sphereCollider.isTrigger = true;
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			rigidbody.mass = 1E-07f;
			WaterVolumeProbe waterVolumeProbe = gameObject.AddComponent<WaterVolumeProbe>();
			waterVolumeProbe.target = target;
			waterVolumeProbe.targetted = true;
			waterVolumeProbe.size = size;
			waterVolumeProbe.exclusions = target.GetComponentsInChildren<WaterVolumeSubtract>(true);
			return waterVolumeProbe;
		}
	}
}
