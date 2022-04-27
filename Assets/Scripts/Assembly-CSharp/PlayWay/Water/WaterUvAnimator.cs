using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public sealed class WaterUvAnimator
	{
		private NormalMapAnimation normalMapAnimation1 = new NormalMapAnimation(1f, -10f, 1f, new Vector2(1f, 1f));

		private NormalMapAnimation normalMapAnimation2 = new NormalMapAnimation(-0.55f, 20f, 0.74f, new Vector2(1.5f, 1.5f));

		private float windOffset1x;

		private float windOffset1y;

		private float windOffset2x;

		private float windOffset2y;

		private Vector2 windSpeed1;

		private Vector2 windSpeed2;

		private Vector2 windSpeed;

		private Water water;

		private WindWaves windWaves;

		private bool hasWindWaves;

		private int bumpMapST;

		private int detailAlbedoMapST;

		private Vector4 uvTransform1;

		private Vector4 uvTransform2;

		private bool windVectorsDirty = true;

		private float lastTime;

		public Vector2 WindOffset
		{
			get
			{
				return new Vector2(windOffset1x, windOffset1y);
			}
		}

		public NormalMapAnimation NormalMapAnimation1
		{
			get
			{
				return normalMapAnimation1;
			}
			set
			{
				normalMapAnimation1 = value;
				windVectorsDirty = true;
				uvTransform1.x = normalMapAnimation1.Tiling.x;
				uvTransform1.y = normalMapAnimation1.Tiling.y;
			}
		}

		public NormalMapAnimation NormalMapAnimation2
		{
			get
			{
				return normalMapAnimation2;
			}
			set
			{
				normalMapAnimation2 = value;
				windVectorsDirty = true;
				uvTransform2.x = normalMapAnimation2.Tiling.x;
				uvTransform2.y = normalMapAnimation2.Tiling.y;
			}
		}

		public void Start(Water water)
		{
			this.water = water;
			windWaves = water.GetComponent<WindWaves>();
			hasWindWaves = windWaves != null;
			bumpMapST = Shader.PropertyToID("_BumpMap_ST");
			detailAlbedoMapST = Shader.PropertyToID("_DetailAlbedoMap_ST");
		}

		public void Update()
		{
			float time = water.Time;
			float num = time - lastTime;
			lastTime = time;
			if (windVectorsDirty || HasWindSpeedChanged())
			{
				PrecomputeWindVectors();
				windVectorsDirty = false;
			}
			windOffset1x += windSpeed1.x * num;
			windOffset1y += windSpeed1.y * num;
			windOffset2x += windSpeed2.x * num;
			windOffset2y += windSpeed2.y * num;
			uvTransform1.z = (0f - windOffset1x) * uvTransform1.x;
			uvTransform1.w = (0f - windOffset1y) * uvTransform1.y;
			uvTransform2.z = (0f - windOffset2x) * uvTransform2.x;
			uvTransform2.w = (0f - windOffset2x) * uvTransform2.y;
			Material waterMaterial = water.WaterMaterial;
			waterMaterial.SetVector(bumpMapST, uvTransform1);
			waterMaterial.SetVector(detailAlbedoMapST, uvTransform2);
		}

		private void PrecomputeWindVectors()
		{
			windSpeed = GetWindSpeed();
			windSpeed1 = FastMath.Rotate(windSpeed, normalMapAnimation1.Deviation * ((float)Math.PI / 180f)) * (normalMapAnimation1.Speed * 0.001365f);
			windSpeed2 = FastMath.Rotate(windSpeed, normalMapAnimation2.Deviation * ((float)Math.PI / 180f)) * (normalMapAnimation2.Speed * 0.00084f);
		}

		private Vector2 GetWindSpeed()
		{
			if (hasWindWaves)
			{
				return windWaves.WindSpeed;
			}
			return new Vector2(1f, 0f);
		}

		private bool HasWindSpeedChanged()
		{
			if (hasWindWaves)
			{
				return windWaves.WindSpeedChanged;
			}
			return false;
		}
	}
}
