using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
	public bool startEnabled;

	public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	public Transform particleParent;

	private void Awake()
	{
		if (startEnabled)
		{
			PlayParticles();
		}
		else
		{
			StopParticles();
		}
	}

	public void PlayParticles()
	{
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.Play();
		}
	}

	public void StopParticles()
	{
		foreach (ParticleSystem particleSystem in particleSystems)
		{
			particleSystem.Stop();
		}
	}

	public void SetPosition(Vector3 pos)
	{
		if (particleParent != null)
		{
			particleParent.position = pos;
		}
	}

	public void SetLookRotation(Vector3 dir)
	{
		if (particleParent != null)
		{
			particleParent.rotation = Quaternion.LookRotation(dir);
		}
	}
}
