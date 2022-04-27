using UnityEngine;

public class ParticleSystemTimeOffset : MonoBehaviour
{
	private void Start()
	{
		GetComponent<ParticleSystem>().time = Random.Range(0, 5000);
	}

	private void Update()
	{
	}
}
