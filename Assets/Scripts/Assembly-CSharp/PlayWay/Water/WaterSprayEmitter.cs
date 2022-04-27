using UnityEngine;

namespace PlayWay.Water
{
	public class WaterSprayEmitter : MonoBehaviour
	{
		[SerializeField]
		private WaterSpray water;

		[SerializeField]
		private float emissionRate = 5f;

		[SerializeField]
		private float startIntensity = 1f;

		[SerializeField]
		private float startVelocity = 1f;

		[SerializeField]
		private float lifetime = 4f;

		private float totalTime;

		private float timeStep;

		private WaterSpray.Particle[] particles;

		private void Start()
		{
			OnValidate();
			particles = new WaterSpray.Particle[Mathf.Max(1, (int)emissionRate)];
		}

		private void Update()
		{
			int num = 0;
			totalTime += Time.deltaTime;
			while (totalTime >= timeStep)
			{
				totalTime -= timeStep;
				particles[num].lifetime = new Vector2(lifetime, lifetime);
				particles[num].maxIntensity = startIntensity;
				particles[num].position = base.transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
				particles[num].velocity = base.transform.forward * startVelocity;
				particles[num++].offset = Random.Range(0f, 10f);
			}
			if (num != 0)
			{
				water.SpawnCustomParticles(particles, num);
			}
		}

		private void OnValidate()
		{
			timeStep = 1f / emissionRate;
		}
	}
}
