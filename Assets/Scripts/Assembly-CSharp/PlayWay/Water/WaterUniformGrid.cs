using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterUniformGrid : WaterPrimitiveBase
	{
		protected override Mesh[] CreateMeshes(int vertexCount, bool volume)
		{
			int num = Mathf.RoundToInt(Mathf.Sqrt(vertexCount));
			List<Mesh> list = new List<Mesh>();
			List<Vector3> list2 = new List<Vector3>();
			List<int> list3 = new List<int>();
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				float z = (float)i / (float)(num - 1) * 2f - 1f;
				for (int j = 0; j < num; j++)
				{
					float x = (float)j / (float)(num - 1) * 2f - 1f;
					if (volume && (j == 0 || i == 0 || j == num - 1 || i == num - 1))
					{
						list2.Add(new Vector3(0f, -0.2f, 0f));
					}
					else
					{
						list2.Add(new Vector3(x, 0f, z));
					}
					if (j != 0 && i != 0 && num2 > num)
					{
						list3.Add(num2);
						list3.Add(num2 - num);
						list3.Add(num2 - num - 1);
						list3.Add(num2 - 1);
					}
					num2++;
					if (num2 == 65000)
					{
						Mesh item = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Uniform Grid {0}x{1} - {2}", num, num, num3.ToString("00")));
						list.Add(item);
						j--;
						i--;
						z = (float)i / (float)(num - 1) * 2f - 1f;
						num2 = 0;
						list2.Clear();
						list3.Clear();
						num3++;
					}
				}
			}
			if (num2 != 0)
			{
				Mesh item2 = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Uniform Grid {0}x{1} - {2}", num, num, num3.ToString("00")));
				list.Add(item2);
			}
			return list.ToArray();
		}

		protected override Matrix4x4 GetMatrix(Camera camera)
		{
			Vector3 pos;
			if (camera.orthographic)
			{
				Vector3 position = camera.transform.position;
				Vector3 forward = camera.transform.forward;
				float num = (water.transform.position.y - position.y) / forward.y;
				if (num > 0f)
				{
					pos = position + forward * num;
				}
				else
				{
					pos = position;
					pos.y = water.transform.position.y;
				}
			}
			else
			{
				pos = camera.transform.position;
				pos.y = water.transform.position.y;
			}
			Vector3 s = ((!camera.orthographic) ? new Vector3(camera.farClipPlane * Mathf.Tan(camera.fieldOfView * ((float)Math.PI / 180f)), camera.farClipPlane, camera.farClipPlane) : new Vector3(camera.orthographicSize * 2f + water.MaxHorizontalDisplacement, camera.orthographicSize * 2f + water.MaxHorizontalDisplacement, camera.orthographicSize * 2f + water.MaxHorizontalDisplacement));
			return Matrix4x4.TRS(pos, (!camera.orthographic) ? Quaternion.AngleAxis(camera.transform.eulerAngles.y, Vector3.up) : Quaternion.identity, s);
		}
	}
}
