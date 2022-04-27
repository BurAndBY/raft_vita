using UnityEngine;

namespace PlayWay.Water
{
	public static class WaterUtilities
	{
		public static Vector3 RaycastPlane(Camera camera, float planeHeight, Vector3 pos)
		{
			Ray ray = camera.ViewportPointToRay(pos);
			if (camera.transform.position.y > planeHeight)
			{
				if (ray.direction.y > -0.01f)
				{
					ray.direction = new Vector3(ray.direction.x, 0f - ray.direction.y - 0.02f, ray.direction.z);
				}
			}
			else if (ray.direction.y < 0.01f)
			{
				ray.direction = new Vector3(ray.direction.x, 0f - ray.direction.y + 0.02f, ray.direction.z);
			}
			float num = (0f - (ray.origin.y - planeHeight)) / ray.direction.y;
			Vector3 vector = ray.direction * num;
			return Quaternion.AngleAxis(0f - camera.transform.eulerAngles.y, Vector3.up) * vector;
		}

		public static Vector3 ViewportWaterPerpendicular(Camera camera)
		{
			Vector3 result = camera.worldToCameraMatrix.MultiplyVector(new Vector3(0f, -1f, 0f));
			result.z = 0f;
			result.Normalize();
			result *= 0.5f;
			result.x += 0.5f;
			result.y += 0.5f;
			return result;
		}

		public static Vector3 ViewportWaterRight(Camera camera)
		{
			Vector3 vector = camera.worldToCameraMatrix.MultiplyVector(Vector3.Cross(camera.transform.forward, new Vector3(0f, -1f, 0f)));
			vector.z = 0f;
			vector.Normalize();
			return vector * 0.5f;
		}

		public static void Destroy(this Object obj)
		{
			Object.DestroyImmediate(obj);
		}
	}
}
