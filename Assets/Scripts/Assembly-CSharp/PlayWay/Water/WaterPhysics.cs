using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	public class WaterPhysics : MonoBehaviour
	{
		[SerializeField]
		[Range(1f, 30f)]
		[Tooltip("Controls precision of the simulation. Keep it low (1 - 2) for small and not important objects. Prefer high values (15 - 30) for ships etc.")]
		private int sampleCount = 20;

		[SerializeField]
		[Range(0f, 3f)]
		[Tooltip("Controls drag force. Determined experimentally in wind tunnels. Example values:\n https://en.wikipedia.org/wiki/Drag_coefficient#General")]
		private float dragCoefficient = 0.9f;

		[SerializeField]
		[Tooltip("Determines how many waves will be used in computations. Set it low for big objects, larger than most of the waves. Set it high for smaller objects of size comparable to many waves.")]
		[Range(0.125f, 1f)]
		private float precision = 0.5f;

		[Tooltip("Adjust buoyancy proportionally, if your collider is bigger or smaller than the actual object. Lowering this may fix some weird behaviour of objects with extremely low density like beach balls or baloons.")]
		[SerializeField]
		private float buoyancyIntensity = 1f;

		[Tooltip("Horizontal flow force intensity.")]
		[SerializeField]
		private float flowIntensity = 1f;

		private Vector3[] cachedSamplePositions;

		private int cachedSampleIndex;

		private int cachedSampleCount;

		private Collider localCollider;

		private Rigidbody rigidBody;

		private float volume;

		private float area;

		private WaterSample[] samples;

		private float numSamplesInv;

		private Vector3 buoyancyPart;

		private float dragPart;

		private float flowPart;

		private Water waterOverride;

		private WaterVolumeProbe waterProbe;

		private Ray rayUp;

		private Ray rayDown;

		public Water AffectingWater
		{
			get
			{
				return ((object)waterProbe == null) ? waterOverride : waterProbe.CurrentWater;
			}
			set
			{
				bool flag = waterOverride == null;
				waterOverride = value;
				if (waterOverride == null)
				{
					if (!flag)
					{
						OnWaterLeave();
					}
					CreateWaterProbe();
				}
				else
				{
					DestroyWaterProbe();
					OnWaterLeave();
					OnWaterEnter();
				}
			}
		}

		public float BuoyancyIntensity
		{
			get
			{
				return buoyancyIntensity;
			}
			set
			{
				buoyancyIntensity = value;
				if (AffectingWater != null)
				{
					PrecomputeBuoyancy();
				}
			}
		}

		public float DragCoefficient
		{
			get
			{
				return dragCoefficient;
			}
			set
			{
				dragCoefficient = value;
				if (AffectingWater != null)
				{
					PrecomputeDrag();
				}
			}
		}

		public float FlowIntensity
		{
			get
			{
				return flowIntensity;
			}
			set
			{
				flowIntensity = value;
				if (AffectingWater != null)
				{
					PrecomputeFlow();
				}
			}
		}

		private void Awake()
		{
			localCollider = GetComponent<Collider>();
			rigidBody = GetComponentInParent<Rigidbody>();
			rayUp = new Ray(Vector3.zero, Vector3.up);
			rayDown = new Ray(Vector3.zero, Vector3.down);
			if (localCollider == null || rigidBody == null)
			{
				Debug.LogError("WaterPhysics component is attached to an object without any Collider and/or RigidBody.");
				base.enabled = false;
			}
			else
			{
				OnValidate();
				PrecomputeSamples();
			}
		}

		public float GetTotalBuoyancy(float fluidDensity = 999.8f)
		{
			return Physics.gravity.magnitude * volume * buoyancyIntensity * fluidDensity / rigidBody.mass;
		}

		private bool ValidateForEditor()
		{
			if (localCollider == null)
			{
				localCollider = GetComponent<Collider>();
				rigidBody = GetComponentInParent<Rigidbody>();
				OnValidate();
			}
			if (localCollider == null || rigidBody == null)
			{
				return false;
			}
			return true;
		}

		private void OnEnable()
		{
			if (waterOverride == null)
			{
				CreateWaterProbe();
			}
		}

		private void OnDisable()
		{
			DestroyWaterProbe();
			OnWaterLeave();
		}

		private void OnValidate()
		{
			numSamplesInv = 1f / (float)sampleCount;
			if (localCollider != null)
			{
				volume = localCollider.ComputeVolume();
				area = localCollider.ComputeArea();
			}
			if (flowIntensity < 0f)
			{
				flowIntensity = 0f;
			}
			if (buoyancyIntensity < 0f)
			{
				buoyancyIntensity = 0f;
			}
			if (AffectingWater != null)
			{
				PrecomputeBuoyancy();
				PrecomputeDrag();
				PrecomputeFlow();
			}
		}

		private void FixedUpdate()
		{
			Water affectingWater = AffectingWater;
			if ((object)affectingWater == null)
			{
				return;
			}
			Bounds bounds = localCollider.bounds;
			float y = bounds.min.y;
			float y2 = bounds.max.y;
			Vector3 result = default(Vector3);
			Vector3 forces = default(Vector3);
			float maxDistance = y2 - y + 80f;
			float fixedDeltaTime = Time.fixedDeltaTime;
			float num = fixedDeltaTime * (1f - rigidBody.drag * fixedDeltaTime) / rigidBody.mass;
			float density = affectingWater.Density;
			float time = affectingWater.Time;
			Vector3 force = default(Vector3);
			Vector3 vector2 = default(Vector3);
			Vector3 vector3 = default(Vector3);
			Vector3 vector4 = default(Vector3);
			for (int i = 0; i < sampleCount; i++)
			{
				Vector3 vector = base.transform.TransformPoint(cachedSamplePositions[cachedSampleIndex]);
				samples[i].GetAndResetFast(vector.x, vector.z, time, ref result, ref forces);
				float y3 = result.y;
				result.y = y - 20f;
				rayUp.origin = result;
				RaycastHit hitInfo;
				if (localCollider.Raycast(rayUp, out hitInfo, maxDistance))
				{
					float y4 = hitInfo.point.y;
					Vector3 normal = hitInfo.normal;
					result.y = y2 + 20f;
					rayDown.origin = result;
					localCollider.Raycast(rayDown, out hitInfo, maxDistance);
					float y5 = hitInfo.point.y;
					float num2 = (y3 - y4) / (y5 - y4);
					if (num2 <= 0f)
					{
						continue;
					}
					if (num2 > 1f)
					{
						num2 = 1f;
					}
					float num3 = density * num2;
					force.x = buoyancyPart.x * num3;
					force.y = buoyancyPart.y * num3;
					force.z = buoyancyPart.z * num3;
					num3 = num2 * 0.5f;
					result.y = y4 * (1f - num3) + y5 * num3;
					Vector3 pointVelocity = rigidBody.GetPointVelocity(result);
					vector2.x = pointVelocity.x + force.x * num;
					vector2.y = pointVelocity.y + force.y * num;
					vector2.z = pointVelocity.z + force.z * num;
					vector3.x = vector2.x * vector2.x;
					vector3.y = vector2.y * vector2.y;
					vector3.z = vector2.z * vector2.z;
					if (vector2.x > 0f)
					{
						vector3.x = 0f - vector3.x;
					}
					if (vector2.y > 0f)
					{
						vector3.y = 0f - vector3.y;
					}
					if (vector2.z > 0f)
					{
						vector3.z = 0f - vector3.z;
					}
					num3 = dragPart * density;
					vector4.x = num3 * vector3.x;
					vector4.y = num3 * vector3.y;
					vector4.z = num3 * vector3.z;
					force.x += vector4.x * num2;
					force.y += vector4.y * num2;
					force.z += vector4.z * num2;
					rigidBody.AddForceAtPosition(force, result, ForceMode.Force);
					float num4 = forces.x * forces.x + forces.y * forces.y + forces.z * forces.z;
					if (num4 != 0f)
					{
						num3 = -1f / num4;
						float num5 = (normal.x * forces.x + normal.y * forces.y + normal.z * forces.z) * num3 + 0.5f;
						if (num5 > 0f)
						{
							force = forces * (num5 * flowPart);
							result.y = y4;
							rigidBody.AddForceAtPosition(force, result, ForceMode.Force);
						}
					}
				}
				if (++cachedSampleIndex >= cachedSampleCount)
				{
					cachedSampleIndex = 0;
				}
			}
		}

		private void CreateWaterProbe()
		{
			if (waterProbe == null)
			{
				waterProbe = WaterVolumeProbe.CreateProbe(rigidBody.transform, localCollider.bounds.extents.magnitude);
				waterProbe.Enter.AddListener(OnWaterEnter);
				waterProbe.Leave.AddListener(OnWaterLeave);
			}
		}

		private void DestroyWaterProbe()
		{
			if (waterProbe != null)
			{
				waterProbe.gameObject.Destroy();
				waterProbe = null;
			}
		}

		private void OnWaterEnter()
		{
			CreateWaterSamplers();
			PrecomputeBuoyancy();
			PrecomputeDrag();
			PrecomputeFlow();
		}

		private void OnWaterLeave()
		{
			if (samples != null)
			{
				for (int i = 0; i < sampleCount; i++)
				{
					samples[i].Stop();
				}
				samples = null;
			}
		}

		private void PrecomputeSamples()
		{
			List<Vector3> list = new List<Vector3>();
			float num = 0.5f;
			float num2 = 1f;
			int num3 = sampleCount * 18;
			Transform transform = base.transform;
			Vector3 min;
			Vector3 max;
			ColliderExtensions.GetLocalMinMax(localCollider, out min, out max);
			for (int i = 0; i < 4; i++)
			{
				if (list.Count >= num3)
				{
					break;
				}
				for (float num4 = num; num4 <= 1f; num4 += num2)
				{
					for (float num5 = num; num5 <= 1f; num5 += num2)
					{
						for (float num6 = num; num6 <= 1f; num6 += num2)
						{
							Vector3 vector = new Vector3(Mathf.Lerp(min.x, max.x, num4), Mathf.Lerp(min.y, max.y, num5), Mathf.Lerp(min.z, max.z, num6));
							if (localCollider.IsPointInside(transform.TransformPoint(vector)))
							{
								list.Add(vector);
							}
						}
					}
				}
				num2 = num;
				num *= 0.5f;
			}
			cachedSamplePositions = list.ToArray();
			cachedSampleCount = cachedSamplePositions.Length;
			Shuffle(cachedSamplePositions);
		}

		private void CreateWaterSamplers()
		{
			if (samples == null || samples.Length != sampleCount)
			{
				samples = new WaterSample[sampleCount];
			}
			Water affectingWater = AffectingWater;
			for (int i = 0; i < sampleCount; i++)
			{
				samples[i] = new WaterSample(affectingWater, WaterSample.DisplacementMode.HeightAndForces, precision);
				samples[i].Start(cachedSamplePositions[cachedSampleIndex]);
				if (++cachedSampleIndex >= cachedSampleCount)
				{
					cachedSampleIndex = 0;
				}
			}
		}

		private void PrecomputeBuoyancy()
		{
			buoyancyPart = -Physics.gravity * (numSamplesInv * volume * buoyancyIntensity);
		}

		private void PrecomputeDrag()
		{
			dragPart = 0.5f * dragCoefficient * area * numSamplesInv;
		}

		private void PrecomputeFlow()
		{
			flowPart = flowIntensity * dragCoefficient * area * numSamplesInv * 100f;
		}

		private void Shuffle<T>(T[] array)
		{
			int num = array.Length;
			while (num > 1)
			{
				int num2 = Random.Range(0, num--);
				T val = array[num];
				array[num] = array[num2];
				array[num2] = val;
			}
		}
	}
}
