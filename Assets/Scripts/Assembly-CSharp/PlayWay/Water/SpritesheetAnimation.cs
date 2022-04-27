using UnityEngine;

namespace PlayWay.Water
{
	public class SpritesheetAnimation : MonoBehaviour
	{
		[SerializeField]
		private int horizontal = 2;

		[SerializeField]
		private int vertical = 2;

		[SerializeField]
		private float timeStep = 0.06f;

		[SerializeField]
		private bool loop;

		[SerializeField]
		private bool destroyGo;

		private Material material;

		private float nextChangeTime;

		private int x;

		private int y;

		private void Start()
		{
			Renderer component = GetComponent<Renderer>();
			material = component.material;
			material.mainTextureScale = new Vector2(1f / (float)horizontal, 1f / (float)vertical);
			material.mainTextureOffset = new Vector2(0f, 0f);
			nextChangeTime = Time.time + timeStep;
		}

		private void Update()
		{
			if (!(Time.time >= nextChangeTime))
			{
				return;
			}
			nextChangeTime += timeStep;
			if (x == horizontal - 1 && y == vertical - 1)
			{
				if (!loop)
				{
					if (destroyGo)
					{
						Object.Destroy(base.gameObject);
					}
					else
					{
						base.enabled = false;
					}
					return;
				}
				x = 0;
				y = 0;
			}
			else
			{
				x++;
				if (x >= horizontal)
				{
					x = 0;
					y++;
				}
			}
			material.mainTextureOffset = new Vector2((float)x / (float)horizontal, 1f - (float)(y + 1) / (float)vertical);
		}
	}
}
