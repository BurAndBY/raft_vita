using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterVolume
	{
		[SerializeField]
		[Tooltip("Makes water volume be infinite in horizontal directions and infinitely deep. It is still reduced by substractive colliders tho. Check that if this is an ocean, sea or if this water spans through most of the scene. If you will uncheck this, you will need to add some child colliders to define where water should display.")]
		private bool boundless = true;

		private Water water;

		private List<WaterVolumeAdd> volumes = new List<WaterVolumeAdd>();

		private List<WaterVolumeSubtract> subtractors = new List<WaterVolumeSubtract>();

		private Camera volumesCamera;

		private bool collidersAdded;

		public bool Boundless
		{
			get
			{
				return boundless;
			}
		}

		public bool HasRenderableAdditiveVolumes
		{
			get
			{
				for (int i = 0; i < volumes.Count; i++)
				{
					if (volumes[i].Mode >= WaterVolumeBase.WaterVolumeMode.PhysicsAndSurfaceRendering)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<WaterVolumeAdd> GetVolumesDirect()
		{
			return volumes;
		}

		public List<WaterVolumeSubtract> GetSubtractiveVolumesDirect()
		{
			return subtractors;
		}

		public void Dispose()
		{
			if (volumesCamera != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(volumesCamera.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(volumesCamera.gameObject);
				}
				volumesCamera = null;
			}
		}

		internal void OnEnable(Water water)
		{
			this.water = water;
			if (collidersAdded || !Application.isPlaying)
			{
				return;
			}
			Collider[] componentsInChildren = water.GetComponentsInChildren<Collider>(true);
			Collider[] array = componentsInChildren;
			foreach (Collider collider in array)
			{
				WaterVolumeSubtract component = collider.GetComponent<WaterVolumeSubtract>();
				if (component == null)
				{
					WaterVolumeAdd waterVolumeAdd = collider.GetComponent<WaterVolumeAdd>();
					if (waterVolumeAdd == null)
					{
						waterVolumeAdd = collider.gameObject.AddComponent<WaterVolumeAdd>();
					}
					AddVolume(waterVolumeAdd);
				}
			}
			collidersAdded = true;
		}

		internal void OnDisable()
		{
			Dispose();
		}

		internal void AddVolume(WaterVolumeAdd volume)
		{
			volumes.Add(volume);
			volume.AssignTo(water);
		}

		internal void RemoveVolume(WaterVolumeAdd volume)
		{
			volumes.Remove(volume);
		}

		internal void AddSubtractor(WaterVolumeSubtract volume)
		{
			subtractors.Add(volume);
			volume.AssignTo(water);
		}

		internal void RemoveSubtractor(WaterVolumeSubtract volume)
		{
			subtractors.Remove(volume);
		}

		public bool IsPointInside(Vector3 point, WaterVolumeSubtract[] exclusions, float radius = 0f)
		{
			foreach (WaterVolumeSubtract subtractor in subtractors)
			{
				if (subtractor.IsPointInside(point) && !Contains(exclusions, subtractor))
				{
					return false;
				}
			}
			if (boundless)
			{
				return point.y - radius <= water.transform.position.y + water.MaxVerticalDisplacement;
			}
			foreach (WaterVolumeAdd volume in volumes)
			{
				if (volume.IsPointInside(point))
				{
					return true;
				}
			}
			return false;
		}

		private bool Contains(WaterVolumeSubtract[] array, WaterVolumeSubtract element)
		{
			if (array == null)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == element)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsPointInsideMainVolume(Vector3 point, float radius = 0f)
		{
			if (boundless)
			{
				return point.y - radius <= water.transform.position.y + water.MaxVerticalDisplacement;
			}
			return false;
		}

		private void CreateVolumesCamera()
		{
			GameObject gameObject = new GameObject();
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			volumesCamera = gameObject.AddComponent<Camera>();
			volumesCamera.enabled = false;
		}
	}
}
