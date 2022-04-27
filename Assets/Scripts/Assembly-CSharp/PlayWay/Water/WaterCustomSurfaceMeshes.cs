using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterCustomSurfaceMeshes
	{
		private struct Edge
		{
			public int vertexIndex0;

			public int vertexIndex1;

			public int faceIndex0;

			public int faceIndex1;
		}

		[SerializeField]
		private Mesh[] customMeshes;

		private Water water;

		private Mesh[] usedMeshes;

		private Mesh[] volumeMeshes;

		public Mesh[] Meshes
		{
			get
			{
				return customMeshes;
			}
			set
			{
				customMeshes = value;
				usedMeshes = null;
				volumeMeshes = null;
			}
		}

		private Mesh[] UsedMeshes
		{
			get
			{
				if (usedMeshes == null)
				{
					List<Mesh> list = new List<Mesh>();
					Mesh[] array = customMeshes;
					foreach (Mesh mesh in array)
					{
						if (mesh != null)
						{
							list.Add(mesh);
						}
					}
					usedMeshes = list.ToArray();
				}
				return usedMeshes;
			}
		}

		public Mesh[] VolumeMeshes
		{
			get
			{
				if (volumeMeshes == null)
				{
					Mesh[] array = UsedMeshes;
					List<Mesh> list = new List<Mesh>();
					Mesh[] array2 = array;
					foreach (Mesh mesh in array2)
					{
						list.Add(mesh);
						list.Add(CreateBoundaryMesh(mesh));
					}
					volumeMeshes = list.ToArray();
				}
				return volumeMeshes;
			}
		}

		public bool Triangular
		{
			get
			{
				return customMeshes == null || UsedMeshes.Length == 0 || UsedMeshes[0].GetTopology(0) == MeshTopology.Triangles;
			}
		}

		internal virtual void OnEnable(Water water)
		{
			this.water = water;
		}

		internal virtual void OnDisable()
		{
			Dispose();
		}

		public Mesh[] GetTransformedMeshes(Camera camera, out Matrix4x4 matrix, bool volume)
		{
			matrix = water.transform.localToWorldMatrix;
			if (volume)
			{
				return VolumeMeshes;
			}
			return UsedMeshes;
		}

		public void Dispose()
		{
			if (volumeMeshes != null)
			{
				for (int i = 1; i < volumeMeshes.Length; i += 2)
				{
					volumeMeshes[i].Destroy();
				}
				volumeMeshes = null;
			}
			usedMeshes = null;
		}

		private Mesh CreateBoundaryMesh(Mesh sourceMesh)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			Vector3[] vertices = sourceMesh.vertices;
			List<Vector3> list = new List<Vector3>();
			List<int> list2 = new List<int>();
			Edge[] array = BuildManifoldEdges(sourceMesh);
			Vector3 item = default(Vector3);
			int item2 = array.Length * 4;
			for (int i = 0; i < array.Length; i++)
			{
				int count = list.Count;
				Vector3 vector = vertices[array[i].vertexIndex0];
				Vector3 vector2 = vertices[array[i].vertexIndex1];
				list.Add(vector);
				list.Add(vector2);
				vector.y -= 1000f;
				vector2.y -= 1000f;
				list.Add(vector);
				list.Add(vector2);
				list2.Add(count + 3);
				list2.Add(count + 2);
				list2.Add(count);
				list2.Add(count + 3);
				list2.Add(count);
				list2.Add(count + 1);
				list2.Add(count + 3);
				list2.Add(count + 2);
				list2.Add(item2);
				item += vector;
				item += vector2;
			}
			item /= (float)(list.Count / 2);
			list.Add(item);
			mesh.vertices = list.ToArray();
			mesh.SetIndices(list2.ToArray(), MeshTopology.Triangles, 0);
			return mesh;
		}

		private static Edge[] BuildManifoldEdges(Mesh mesh)
		{
			Edge[] array = BuildEdges(mesh.vertexCount, mesh.triangles);
			List<Edge> list = new List<Edge>();
			Edge[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Edge item = array2[i];
				if (item.faceIndex0 == item.faceIndex1)
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		private static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
		{
			int num = triangleArray.Length;
			int[] array = new int[vertexCount + num];
			int num2 = triangleArray.Length / 3;
			for (int i = 0; i < vertexCount; i++)
			{
				array[i] = -1;
			}
			Edge[] array2 = new Edge[num];
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				int num4 = triangleArray[j * 3 + 2];
				for (int k = 0; k < 3; k++)
				{
					int num5 = triangleArray[j * 3 + k];
					if (num4 < num5)
					{
						Edge edge = default(Edge);
						edge.vertexIndex0 = num4;
						edge.vertexIndex1 = num5;
						edge.faceIndex0 = j;
						edge.faceIndex1 = j;
						array2[num3] = edge;
						int num6 = array[num4];
						if (num6 == -1)
						{
							array[num4] = num3;
						}
						else
						{
							while (true)
							{
								int num7 = array[vertexCount + num6];
								if (num7 == -1)
								{
									break;
								}
								num6 = num7;
							}
							array[vertexCount + num6] = num3;
						}
						array[vertexCount + num3] = -1;
						num3++;
					}
					num4 = num5;
				}
			}
			for (int l = 0; l < num2; l++)
			{
				int num8 = triangleArray[l * 3 + 2];
				for (int m = 0; m < 3; m++)
				{
					int num9 = triangleArray[l * 3 + m];
					if (num8 > num9)
					{
						bool flag = false;
						for (int num10 = array[num9]; num10 != -1; num10 = array[vertexCount + num10])
						{
							Edge edge2 = array2[num10];
							if (edge2.vertexIndex1 == num8 && edge2.faceIndex0 == edge2.faceIndex1)
							{
								array2[num10].faceIndex1 = l;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							Edge edge3 = default(Edge);
							edge3.vertexIndex0 = num8;
							edge3.vertexIndex1 = num9;
							edge3.faceIndex0 = l;
							edge3.faceIndex1 = l;
							array2[num3] = edge3;
							num3++;
						}
					}
					num8 = num9;
				}
			}
			Edge[] array3 = new Edge[num3];
			for (int n = 0; n < num3; n++)
			{
				array3[n] = array2[n];
			}
			return array3;
		}
	}
}
