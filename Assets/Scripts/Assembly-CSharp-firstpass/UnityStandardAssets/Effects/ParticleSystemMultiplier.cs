using UnityEngine;

namespace UnityStandardAssets.Effects
{
	public class ParticleSystemMultiplier : MonoBehaviour
	{
		public float multiplier = 1f;

		private void Start()
		{
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.startSize *= multiplier;
				particleSystem.startSpeed *= multiplier;
				particleSystem.startLifetime *= Mathf.Lerp(multiplier, 1f, 0.5f);
				particleSystem.Clear();
				particleSystem.Play();
			}
		}
	}
}
