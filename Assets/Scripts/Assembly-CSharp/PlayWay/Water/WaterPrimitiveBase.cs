using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public abstract class WaterPrimitiveBase
	{
		protected class CachedMeshSet
		{
			public Mesh[] meshes;

			public int lastFrameUsed;

			public CachedMeshSet(Mesh[] meshes)
			{
				this.meshes = meshes;
				Update();
			}

			public void Update()
			{
				lastFrameUsed = Time.frameCount;
			}
		}

		protected Water water;

		protected Dictionary<int, CachedMeshSet> cache = new Dictionary<int, CachedMeshSet>(Int32EqualityComparer.Default);

		private List<int> keysToRemove;

		public void Dispose()
		{
			foreach (CachedMeshSet value in cache.Values)
			{
				Mesh[] meshes = value.meshes;
				foreach (Mesh obj in meshes)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(obj);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
				}
			}
			cache.Clear();
		}

		internal virtual void OnEnable(Water water)
		{
			this.water = water;
		}

		internal virtual void OnDisable()
		{
			Dispose();
		}

		internal virtual void AddToMaterial(Water water)
		{
		}

		internal virtual void RemoveFromMaterial(Water water)
		{
		}

		public virtual Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, int vertexCount, bool volume)
		{
			if (camera != null)
			{
				matrix = GetMatrix(camera);
			}
			else
			{
				matrix = Matrix4x4.identity;
			}
			int num = vertexCount;
			if (volume)
			{
				num = -num;
			}
			CachedMeshSet value;
			if (!cache.TryGetValue(num, out value))
			{
				value = (cache[num] = new CachedMeshSet(CreateMeshes(vertexCount, volume)));
			}
			else
			{
				value.Update();
			}
			return value.meshes;
		}

		internal void Update()
		{
			int frameCount = Time.frameCount;
			if (keysToRemove == null)
			{
				keysToRemove = new List<int>();
			}
			Dictionary<int, CachedMeshSet>.Enumerator enumerator = cache.GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, CachedMeshSet> current = enumerator.Current;
				if (frameCount - current.Value.lastFrameUsed <= 27)
				{
					continue;
				}
				keysToRemove.Add(current.Key);
				Mesh[] meshes = current.Value.meshes;
				foreach (Mesh obj in meshes)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(obj);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(obj);
					}
				}
			}
			for (int j = 0; j < keysToRemove.Count; j++)
			{
				cache.Remove(keysToRemove[j]);
			}
			keysToRemove.Clear();
		}

		protected abstract Matrix4x4 GetMatrix(Camera camera);

		protected abstract Mesh[] CreateMeshes(int vertexCount, bool volume);

		protected Mesh CreateMesh(Vector3[] vertices, int[] indices, string name, bool triangular = false)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			mesh.name = name;
			mesh.vertices = vertices;
			mesh.SetIndices(indices, (!triangular) ? MeshTopology.Quads : MeshTopology.Triangles, 0);
			mesh.RecalculateBounds();
			mesh.UploadMeshData(true);
			return mesh;
		}
	}
}
