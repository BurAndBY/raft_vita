using System;
using UnityEngine;

namespace PlayWay.Water
{
	public sealed class WaterSample
	{
		public enum DisplacementMode
		{
			Height,
			Displacement,
			HeightAndForces
		}

		public enum ComputationsMode
		{
			Normal = 0,
			[Obsolete]
			Stabilized = 0,
			ForceCompletion = 2
		}

		private readonly Water water;

		private float x;

		private float z;

		private float time;

		private Vector3 displaced;

		private Vector3 previousResult;

		private Vector3 forces;

		private Vector3 previousForces;

		private bool finished;

		private bool enqueued;

		private readonly float precision;

		private readonly float horizontalThreshold;

		private readonly DisplacementMode displacementMode;

		private static readonly float[] weights = new float[14]
		{
			0.85f, 0.75f, 0.83f, 0.77f, 0.85f, 0.75f, 0.85f, 0.75f, 0.83f, 0.77f,
			0.85f, 0.75f, 0.85f, 0.75f
		};

		public bool Finished
		{
			get
			{
				return finished;
			}
		}

		public Vector2 Position
		{
			get
			{
				return new Vector2(x, z);
			}
		}

		public WaterSample(Water water, DisplacementMode displacementMode = DisplacementMode.Height, float precision = 1f)
		{
			if (water == null)
			{
				throw new ArgumentException("Argument 'water' is null.");
			}
			if (precision <= 0f || precision > 1f)
			{
				throw new ArgumentException("Precision has to be between 0.0 and 1.0.");
			}
			this.precision = precision;
			horizontalThreshold = 0.045f / (precision * precision * precision);
			this.water = water;
			this.displacementMode = displacementMode;
			previousResult.x = float.NaN;
		}

		public void Start(Vector3 origin)
		{
			previousResult = origin;
			finished = true;
			GetAndReset(origin.x, origin.z);
		}

		public void Start(float x, float z)
		{
			previousResult = new Vector3(x, water.transform.position.y, z);
			finished = true;
			GetAndReset(x, z);
		}

		public Vector3 GetAndReset(Vector3 origin, ComputationsMode mode = ComputationsMode.Normal)
		{
			return GetAndReset(origin.x, origin.z, mode);
		}

		public Vector3 GetAndReset(float x, float z, ComputationsMode mode = ComputationsMode.Normal)
		{
			Vector3 vector;
			return GetAndReset(x, z, mode, out vector);
		}

		public Vector3 GetAndReset(float x, float z, ComputationsMode mode, out Vector3 forces)
		{
			switch (mode)
			{
			case ComputationsMode.ForceCompletion:
				if (!finished)
				{
					finished = true;
					ComputationStep(true);
				}
				break;
			case ComputationsMode.Normal:
				if (!finished && !float.IsNaN(previousResult.x))
				{
					forces = previousForces;
					return previousResult;
				}
				previousResult = displaced;
				previousForces = this.forces;
				break;
			}
			finished = true;
			if (!enqueued)
			{
				WaterAsynchronousTasks.Instance.AddWaterSampleComputations(this);
				enqueued = true;
				water.OnSamplingStarted();
			}
			Vector3 result = displaced;
			result.y += water.transform.position.y;
			forces = this.forces;
			this.x = x;
			this.z = z;
			displaced.x = x;
			displaced.y = 0f;
			displaced.z = z;
			this.forces.x = 0f;
			this.forces.y = 0f;
			this.forces.z = 0f;
			time = water.Time;
			finished = false;
			return result;
		}

		public void GetAndResetFast(float x, float z, float time, ref Vector3 result, ref Vector3 forces)
		{
			if (!finished)
			{
				forces = previousForces;
				result = previousResult;
				return;
			}
			previousResult = displaced;
			previousForces = this.forces;
			result = displaced;
			result.y += water.transform.position.y;
			forces = this.forces;
			this.x = x;
			this.z = z;
			displaced.x = x;
			displaced.y = 0f;
			displaced.z = z;
			this.forces.x = 0f;
			this.forces.y = 0f;
			this.forces.z = 0f;
			this.time = time;
			finished = false;
		}

		public Vector3 Stop()
		{
			if (enqueued)
			{
				if (WaterAsynchronousTasks.HasInstance)
				{
					WaterAsynchronousTasks.Instance.RemoveWaterSampleComputations(this);
				}
				enqueued = false;
				if (water != null)
				{
					water.OnSamplingStopped();
				}
			}
			return displaced;
		}

		internal void ComputationStep(bool ignoreFinishedFlag = false)
		{
			if (finished && !ignoreFinishedFlag)
			{
				return;
			}
			if (displacementMode == DisplacementMode.Height || displacementMode == DisplacementMode.HeightAndForces)
			{
				CompensateHorizontalDisplacement();
				if (displacementMode == DisplacementMode.Height)
				{
					float heightAt = water.GetHeightAt(x, z, 0f, precision, time);
					displaced.y += heightAt;
				}
				else
				{
					Vector4 heightAndForcesAt = water.GetHeightAndForcesAt(x, z, 0f, precision, time);
					displaced.y += heightAndForcesAt.w;
					forces.x += heightAndForcesAt.x;
					forces.y += heightAndForcesAt.y;
					forces.z += heightAndForcesAt.z;
				}
			}
			else
			{
				Vector3 vector = ((water.WaterId == -1) ? default(Vector3) : water.GetDisplacementAt(x, z, 0f, precision, time));
				displaced += vector;
			}
			finished = true;
		}

		private void CompensateHorizontalDisplacement()
		{
			Vector2 horizontalDisplacementAt = water.GetHorizontalDisplacementAt(x, z, 0f, precision * 0.5f, time);
			x -= horizontalDisplacementAt.x;
			z -= horizontalDisplacementAt.y;
			if (!(horizontalDisplacementAt.x > horizontalThreshold) && !(horizontalDisplacementAt.y > horizontalThreshold) && !(horizontalDisplacementAt.x < 0f - horizontalThreshold) && !(horizontalDisplacementAt.y < 0f - horizontalThreshold))
			{
				return;
			}
			for (int i = 0; i < 14; i++)
			{
				horizontalDisplacementAt = water.GetHorizontalDisplacementAt(x, z, 0f, precision * 0.5f, time);
				float num = displaced.x - (x + horizontalDisplacementAt.x);
				float num2 = displaced.z - (z + horizontalDisplacementAt.y);
				x += num * weights[i];
				z += num2 * weights[i];
				if (num < horizontalThreshold && num2 < horizontalThreshold && num > 0f - horizontalThreshold && num2 > 0f - horizontalThreshold)
				{
					break;
				}
			}
		}
	}
}
