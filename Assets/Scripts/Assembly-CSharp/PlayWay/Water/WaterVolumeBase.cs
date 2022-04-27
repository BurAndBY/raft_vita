using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlayWay.Water
{
	[ExecuteInEditMode]
	public abstract class WaterVolumeBase : MonoBehaviour
	{
		public enum WaterVolumeMode
		{
			Physics,
			PhysicsAndSurfaceRendering,
			PhysicsAndFullRendering
		}

		[SerializeField]
		private Water water;

		[SerializeField]
		private WaterVolumeMode mode = WaterVolumeMode.PhysicsAndSurfaceRendering;

		private Collider[] colliders;

		private MeshRenderer[] volumeRenderers;

		private float radius;

		public Water Water
		{
			get
			{
				return water;
			}
		}

		public WaterVolumeMode Mode
		{
			get
			{
				return mode;
			}
		}

		public MeshRenderer[] VolumeRenderers
		{
			get
			{
				return volumeRenderers;
			}
		}

		protected virtual CullMode CullMode
		{
			get
			{
				return CullMode.Back;
			}
		}

		private void OnEnable()
		{
			colliders = GetComponents<Collider>();
			base.gameObject.layer = WaterProjectSettings.Instance.WaterCollidersLayer;
			Register(water);
			if (mode >= WaterVolumeMode.PhysicsAndSurfaceRendering && water != null && Application.isPlaying)
			{
				CreateRenderers();
			}
		}

		private void OnDisable()
		{
			DisposeRenderers();
			Unregister(water);
		}

		protected abstract void Register(Water water);

		protected abstract void Unregister(Water water);

		private void OnValidate()
		{
			colliders = GetComponents<Collider>();
			for (int i = 0; i < colliders.Length; i++)
			{
				Collider collider = colliders[i];
				if (!collider.isTrigger)
				{
					collider.isTrigger = true;
				}
			}
			PerformUpdate();
		}

		public void AssignTo(Water water)
		{
			if (!(this.water == water))
			{
				DisposeRenderers();
				Unregister(water);
				this.water = water;
				Register(water);
				if (mode >= WaterVolumeMode.PhysicsAndSurfaceRendering && water != null && Application.isPlaying)
				{
					CreateRenderers();
				}
			}
		}

		public void EnableRenderers(bool forBorderRendering)
		{
			if (volumeRenderers != null)
			{
				bool flag = !forBorderRendering || mode == WaterVolumeMode.PhysicsAndFullRendering;
				for (int i = 0; i < volumeRenderers.Length; i++)
				{
					volumeRenderers[i].enabled = flag;
				}
			}
		}

		public void DisableRenderers()
		{
			if (volumeRenderers != null)
			{
				for (int i = 0; i < volumeRenderers.Length; i++)
				{
					volumeRenderers[i].enabled = false;
				}
			}
		}

		internal void SetLayer(int layer)
		{
			if (volumeRenderers != null)
			{
				for (int i = 0; i < volumeRenderers.Length; i++)
				{
					volumeRenderers[i].gameObject.layer = layer;
				}
			}
		}

		public bool IsPointInside(Vector3 point)
		{
			Collider[] array = colliders;
			foreach (Collider convex in array)
			{
				if (convex.IsPointInside(point))
				{
					return true;
				}
			}
			return false;
		}

		private void DisposeRenderers()
		{
			if (volumeRenderers == null)
			{
				return;
			}
			MeshRenderer[] array = volumeRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer != null)
				{
					UnityEngine.Object.Destroy(meshRenderer.gameObject);
				}
			}
			volumeRenderers = null;
		}

		protected virtual void CreateRenderers()
		{
			int num = colliders.Length;
			volumeRenderers = new MeshRenderer[num];
			Material sharedMaterial = ((CullMode != CullMode.Back) ? water.WaterVolumeBackMaterial : water.WaterVolumeMaterial);
			for (int i = 0; i < num; i++)
			{
				Collider collider = colliders[i];
				GameObject gameObject;
				if (collider is BoxCollider)
				{
					gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					gameObject.transform.localScale = (collider as BoxCollider).size;
				}
				else if (collider is MeshCollider)
				{
					gameObject = new GameObject();
					gameObject.hideFlags = HideFlags.DontSave;
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = (collider as MeshCollider).sharedMesh;
					gameObject.AddComponent<MeshRenderer>();
				}
				else if (collider is SphereCollider)
				{
					float num2 = (collider as SphereCollider).radius * 2f;
					gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					gameObject.transform.localScale = new Vector3(num2, num2, num2);
				}
				else
				{
					if (!(collider is CapsuleCollider))
					{
						throw new InvalidOperationException("Unsupported collider type.");
					}
					CapsuleCollider capsuleCollider = collider as CapsuleCollider;
					float num3 = capsuleCollider.height * 0.5f;
					float num4 = capsuleCollider.radius * 2f;
					gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
					switch (capsuleCollider.direction)
					{
					case 0:
						gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						gameObject.transform.localScale = new Vector3(num3, num4, num4);
						break;
					case 1:
						gameObject.transform.localScale = new Vector3(num4, num3, num4);
						break;
					case 2:
						gameObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
						gameObject.transform.localScale = new Vector3(num4, num4, num3);
						break;
					}
				}
				gameObject.hideFlags = HideFlags.DontSave;
				gameObject.name = "Volume Renderer";
				gameObject.layer = WaterProjectSettings.Instance.WaterLayer;
				gameObject.transform.SetParent(base.transform, false);
				UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
				MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
				component.sharedMaterial = sharedMaterial;
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
				volumeRenderers[i] = component;
			}
		}

		private void PerformUpdate()
		{
		}
	}
}
