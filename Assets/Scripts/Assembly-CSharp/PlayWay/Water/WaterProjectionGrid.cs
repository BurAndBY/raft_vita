using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	[ExecuteInEditMode]
	public class WaterProjectionGrid : WaterPrimitiveBase
	{
		private float previousVerticesPerPixel;

		internal override void OnEnable(Water water)
		{
			base.OnEnable(water);
			base.water = water;
		}

		internal override void OnDisable()
		{
			base.OnDisable();
		}

		internal override void AddToMaterial(Water water)
		{
			water.SetKeyword("_PROJECTION_GRID", true);
		}

		internal override void RemoveFromMaterial(Water water)
		{
			water.SetKeyword("_PROJECTION_GRID", false);
		}

		public override Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, int vertexCount, bool volume)
		{
			int pixelWidth = camera.pixelWidth;
			int pixelHeight = camera.pixelHeight;
			int key = pixelHeight | (pixelWidth << 16);
			Vector3 position = camera.transform.position;
			matrix = Matrix4x4.identity;
			matrix.m03 = position.x;
			matrix.m13 = 0f;
			matrix.m23 = position.z;
			float num = (float)vertexCount / (float)(pixelWidth * pixelHeight);
			water.WaterMaterial.SetMatrix("_InvViewMatrix", camera.cameraToWorldMatrix);
			water.WaterBackMaterial.SetMatrix("_InvViewMatrix", camera.cameraToWorldMatrix);
			CachedMeshSet value;
			if (!cache.TryGetValue(key, out value))
			{
				value = (cache[key] = new CachedMeshSet(CreateMeshes(Mathf.RoundToInt((float)pixelWidth * num), Mathf.RoundToInt((float)pixelHeight * num))));
			}
			return value.meshes;
		}

		protected override Mesh[] CreateMeshes(int vertexCount, bool volume)
		{
			throw new InvalidOperationException();
		}

		private Mesh[] CreateMeshes(int verticesX, int verticesY)
		{
			List<Mesh> list = new List<Mesh>();
			List<Vector3> list2 = new List<Vector3>();
			List<int> list3 = new List<int>();
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < verticesY; i++)
			{
				float y = (float)i / (float)(verticesY - 1);
				for (int j = 0; j < verticesX; j++)
				{
					float x = (float)j / (float)(verticesX - 1);
					list2.Add(new Vector3(x, y, 0f));
					if (j != 0 && i != 0 && num > verticesX)
					{
						list3.Add(num);
						list3.Add(num - 1);
						list3.Add(num - verticesX - 1);
						list3.Add(num - verticesX);
					}
					num++;
					if (num == 65000)
					{
						Mesh mesh = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Projection Grid {0}x{1} - {2}", verticesX, verticesY, num2.ToString("00")));
						mesh.bounds = new Bounds(Vector3.zero, new Vector3(100000f, 0.2f, 100000f));
						list.Add(mesh);
						j--;
						i--;
						y = (float)i / (float)(verticesY - 1);
						num = 0;
						list2.Clear();
						list3.Clear();
						num2++;
					}
				}
			}
			if (num != 0)
			{
				Mesh mesh2 = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Projection Grid {0}x{1} - {2}", verticesX, verticesY, num2.ToString("00")));
				mesh2.bounds = new Bounds(Vector3.zero, new Vector3(100000f, 0.2f, 100000f));
				list.Add(mesh2);
			}
			return list.ToArray();
		}

		protected override Matrix4x4 GetMatrix(Camera camera)
		{
			throw new NotImplementedException();
		}
	}
}
