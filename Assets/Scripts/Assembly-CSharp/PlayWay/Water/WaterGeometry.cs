using System;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterGeometry
	{
		public enum Type
		{
			RadialGrid,
			ProjectionGrid,
			UniformGrid,
			CustomMeshes
		}

		[SerializeField]
		[Tooltip("Geometry type used for display.")]
		private Type type;

		[SerializeField]
		[Tooltip("Water geometry vertex count at 1920x1080.")]
		private int baseVertexCount = 500000;

		[Tooltip("Water geometry vertex count at 1920x1080 on systems with tesselation support. Set it a bit lower as tesselation will place additional, better distributed vertices in shader.")]
		[SerializeField]
		private int tesselatedBaseVertexCount = 16000;

		[SerializeField]
		private bool adaptToResolution = true;

		[SerializeField]
		private WaterRadialGrid radialGrid;

		[SerializeField]
		private WaterProjectionGrid projectionGrid;

		[SerializeField]
		private WaterUniformGrid uniformGrid;

		[SerializeField]
		private WaterCustomSurfaceMeshes customSurfaceMeshes;

		[Obsolete]
		[SerializeField]
		private Mesh[] customMeshes;

		private Type previousType;

		private int previousTargetVertexCount;

		private int thisSystemVertexCount;

		private int frameCount;

		public Type GeometryType
		{
			get
			{
				return type;
			}
		}

		public int VertexCount
		{
			get
			{
				return baseVertexCount;
			}
		}

		public int TesselatedBaseVertexCount
		{
			get
			{
				return tesselatedBaseVertexCount;
			}
		}

		public bool AdaptToResolution
		{
			get
			{
				return adaptToResolution;
			}
		}

		public bool Triangular
		{
			get
			{
				if (type == Type.CustomMeshes)
				{
					return customSurfaceMeshes.Triangular;
				}
				return false;
			}
		}

		public WaterCustomSurfaceMeshes CustomSurfaceMeshes
		{
			get
			{
				return customSurfaceMeshes;
			}
		}

		internal void OnEnable(Water water)
		{
			if (customMeshes != null && customMeshes.Length != 0)
			{
				customSurfaceMeshes.Meshes = customMeshes;
				customMeshes = null;
			}
			OnValidate(water);
			UpdateVertexCount();
			radialGrid.OnEnable(water);
			projectionGrid.OnEnable(water);
			uniformGrid.OnEnable(water);
			customSurfaceMeshes.OnEnable(water);
		}

		internal void OnDisable()
		{
			if (radialGrid != null)
			{
				radialGrid.OnDisable();
			}
			if (projectionGrid != null)
			{
				projectionGrid.OnDisable();
			}
			if (uniformGrid != null)
			{
				uniformGrid.OnDisable();
			}
			if (customSurfaceMeshes != null)
			{
				customSurfaceMeshes.OnDisable();
			}
		}

		[Obsolete("Use WaterGeometry.CustomMeshes.Meshes")]
		public Mesh[] GetCustomMeshesDirect()
		{
			return customSurfaceMeshes.Meshes;
		}

		[Obsolete("Use WaterGeometry.CustomMeshes.Meshes")]
		public void SetCustomMeshes(Mesh[] meshes)
		{
			customSurfaceMeshes.Meshes = meshes;
		}

		internal void OnValidate(Water water)
		{
			if (radialGrid == null)
			{
				radialGrid = new WaterRadialGrid();
			}
			if (projectionGrid == null)
			{
				projectionGrid = new WaterProjectionGrid();
			}
			if (uniformGrid == null)
			{
				uniformGrid = new WaterUniformGrid();
			}
			if (customSurfaceMeshes == null)
			{
				customSurfaceMeshes = new WaterCustomSurfaceMeshes();
			}
			if (previousType != type)
			{
				if (previousType == Type.RadialGrid)
				{
					radialGrid.RemoveFromMaterial(water);
				}
				if (previousType == Type.ProjectionGrid)
				{
					projectionGrid.RemoveFromMaterial(water);
				}
				if (previousType == Type.UniformGrid)
				{
					uniformGrid.RemoveFromMaterial(water);
				}
				if (type == Type.RadialGrid)
				{
					radialGrid.AddToMaterial(water);
				}
				if (type == Type.ProjectionGrid)
				{
					projectionGrid.AddToMaterial(water);
				}
				if (type == Type.UniformGrid)
				{
					uniformGrid.AddToMaterial(water);
				}
				previousType = type;
			}
			UpdateVertexCount();
			if (previousTargetVertexCount != thisSystemVertexCount)
			{
				radialGrid.Dispose();
				uniformGrid.Dispose();
				projectionGrid.Dispose();
				previousTargetVertexCount = thisSystemVertexCount;
			}
		}

		internal void Update()
		{
			if (++frameCount > 8)
			{
				frameCount = 0;
			}
			switch (frameCount)
			{
			case 0:
				radialGrid.Update();
				break;
			case 3:
				projectionGrid.Update();
				break;
			case 6:
				uniformGrid.Update();
				break;
			}
		}

		public Mesh[] GetMeshes(WaterGeometryType geometryType, int vertexCount, bool volume)
		{
			if (geometryType == WaterGeometryType.ProjectionGrid)
			{
				throw new InvalidOperationException("Projection grid needs camera to be retrieved. Use GetTransformedMeshes instead.");
			}
			Matrix4x4 matrix;
			switch (geometryType)
			{
			case WaterGeometryType.Auto:
				switch (type)
				{
				case Type.RadialGrid:
					return radialGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
				case Type.ProjectionGrid:
					return projectionGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
				case Type.UniformGrid:
					return uniformGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
				case Type.CustomMeshes:
					return customSurfaceMeshes.GetTransformedMeshes(null, out matrix, volume);
				default:
					throw new InvalidOperationException("Unknown water geometry type.");
				}
			case WaterGeometryType.RadialGrid:
				return radialGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
			case WaterGeometryType.ProjectionGrid:
				return projectionGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
			case WaterGeometryType.UniformGrid:
				return uniformGrid.GetTransformedMeshes(null, out matrix, vertexCount, volume);
			default:
				throw new InvalidOperationException("Unknown water geometry type.");
			}
		}

		public Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, WaterGeometryType geometryType, bool volume, int vertexCount = 0)
		{
			if (vertexCount == 0)
			{
				vertexCount = ((!adaptToResolution) ? thisSystemVertexCount : Mathf.RoundToInt((float)thisSystemVertexCount * ((float)(camera.pixelWidth * camera.pixelHeight) / 2073600f)));
			}
			switch (geometryType)
			{
			case WaterGeometryType.Auto:
				switch (type)
				{
				case Type.RadialGrid:
					return radialGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case Type.ProjectionGrid:
					return projectionGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case Type.UniformGrid:
					return uniformGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
				case Type.CustomMeshes:
					return customSurfaceMeshes.GetTransformedMeshes(null, out matrix, volume);
				default:
					throw new InvalidOperationException("Unknown water geometry type.");
				}
			case WaterGeometryType.RadialGrid:
				return radialGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			case WaterGeometryType.ProjectionGrid:
				return projectionGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			case WaterGeometryType.UniformGrid:
				return uniformGrid.GetTransformedMeshes(camera, out matrix, vertexCount, volume);
			default:
				throw new InvalidOperationException("Unknown water geometry type.");
			}
		}

		private void UpdateVertexCount()
		{
			thisSystemVertexCount = ((!SystemInfo.supportsComputeShaders) ? Mathf.Min(baseVertexCount, WaterQualitySettings.Instance.MaxVertexCount) : Mathf.Min(tesselatedBaseVertexCount, WaterQualitySettings.Instance.MaxTesselatedVertexCount));
		}
	}
}
