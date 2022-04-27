using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayWay.Water
{
	[Serializable]
	public class WaterRadialGrid : WaterPrimitiveBase
	{
		protected override Mesh[] CreateMeshes(int vertexCount, bool volume)
		{
			int num = Mathf.RoundToInt(Mathf.Sqrt(vertexCount));
			int num2 = Mathf.RoundToInt((float)num * 0.78f);
			int num3 = Mathf.RoundToInt((float)vertexCount / (float)num2);
			int num4 = num3;
			if (volume)
			{
				num4 >>= 1;
			}
			List<Mesh> list = new List<Mesh>();
			List<Vector3> list2 = new List<Vector3>();
			List<int> list3 = new List<int>();
			int num5 = 0;
			int num6 = 0;
			Vector2[] array = new Vector2[num2];
			for (int i = 0; i < num2; i++)
			{
				float num7 = (float)i / (float)(num2 - 1) * 2f - 1f;
				num7 *= (float)Math.PI / 4f;
				array[i] = new Vector2(Mathf.Sin(num7), Mathf.Cos(num7)).normalized;
			}
			for (int j = 0; j < num4; j++)
			{
				float num8 = (float)j / (float)(num3 - 1);
				num8 = 1f - Mathf.Cos(num8 * (float)Math.PI * 0.5f);
				for (int k = 0; k < num2; k++)
				{
					Vector2 vector = array[k] * num8;
					if (j < num4 - 2 || !volume)
					{
						list2.Add(new Vector3(vector.x, 0f, vector.y));
					}
					else if (j == num4 - 2)
					{
						list2.Add(new Vector3(vector.x * 10f, -0.9f, vector.y) * 0.5f);
					}
					else
					{
						list2.Add(new Vector3(vector.x * 10f, -0.9f, vector.y * -10f) * 0.5f);
					}
					if (k != 0 && j != 0 && num5 > num2)
					{
						list3.Add(num5);
						list3.Add(num5 - num2);
						list3.Add(num5 - num2 - 1);
						list3.Add(num5 - 1);
					}
					num5++;
					if (num5 == 65000)
					{
						Mesh item = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Radial Grid {0}x{1} - {2}", num2, num3, num6.ToString("00")));
						list.Add(item);
						k--;
						j--;
						num8 = (float)j / (float)(num3 - 1);
						num8 = 1f - Mathf.Cos(num8 * (float)Math.PI * 0.5f);
						num5 = 0;
						list2.Clear();
						list3.Clear();
						num6++;
					}
				}
			}
			if (num5 != 0)
			{
				Mesh item2 = CreateMesh(list2.ToArray(), list3.ToArray(), string.Format("Radial Grid {0}x{1} - {2}", num2, num3, num6.ToString("00")));
				list.Add(item2);
			}
			return list.ToArray();
		}

		protected override Matrix4x4 GetMatrix(Camera camera)
		{
			Vector3 vector = WaterUtilities.ViewportWaterPerpendicular(camera);
			Vector3 vector2 = WaterUtilities.ViewportWaterRight(camera);
			float y = water.transform.position.y;
			Vector3 vector3 = WaterUtilities.RaycastPlane(camera, y, vector - vector2);
			Vector3 vector4 = WaterUtilities.RaycastPlane(camera, y, vector + vector2);
			float farClipPlane = camera.farClipPlane;
			float num = Mathf.Tan(camera.fieldOfView * 0.5f * ((float)Math.PI / 180f));
			Vector3 position = camera.transform.position;
			Vector3 s = ((!camera.orthographic) ? new Vector3(farClipPlane * num * camera.aspect + water.MaxHorizontalDisplacement * 2f, farClipPlane, farClipPlane) : new Vector3(farClipPlane, farClipPlane, farClipPlane));
			float num2 = vector4.x - vector3.x;
			if (num2 < 0f)
			{
				num2 = 0f - num2;
			}
			float num3 = ((!(vector3.z < vector4.z)) ? vector4.z : vector3.z) - (num2 + water.MaxHorizontalDisplacement * 2f) * s.z / s.x;
			if (camera.orthographic)
			{
				num3 -= camera.orthographicSize * 3.2f;
			}
			float y2 = camera.transform.forward.y;
			Vector3 vector5;
			if (y2 < -0.98f || y2 > 0.98f)
			{
				vector5 = -camera.transform.up;
				float num4 = Mathf.Sqrt(vector5.x * vector5.x + vector5.z * vector5.z);
				vector5.x /= num4;
				vector5.z /= num4;
				if (!camera.orthographic)
				{
					num3 = (0f - position.y) * 4f * num;
				}
			}
			else
			{
				vector5 = camera.transform.forward;
				float num5 = Mathf.Sqrt(vector5.x * vector5.x + vector5.z * vector5.z);
				vector5.x /= num5;
				vector5.z /= num5;
			}
			if (!camera.orthographic)
			{
				s.z -= num3;
			}
			return Matrix4x4.TRS(new Vector3(position.x + vector5.x * num3, y, position.z + vector5.z * num3), Quaternion.AngleAxis(Mathf.Atan2(vector5.x, vector5.z) * 57.29578f, Vector3.up), s);
		}
	}
}
