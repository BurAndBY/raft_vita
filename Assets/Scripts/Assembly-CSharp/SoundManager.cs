using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SingletonGeneric<SoundManager>
{
	private Dictionary<string, AudioSource> audioConnections = new Dictionary<string, AudioSource>();

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		if (audioConnections.Count == 0)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				audioConnections.Add(child.name, child.GetComponent<AudioSource>());
			}
		}
	}

	public AudioSource PlaySound(string nameOfSound, float minPitch = 0.8f, float maxPitch = 1.2f)
	{
		if (!audioConnections.ContainsKey(nameOfSound))
		{
			return null;
		}
		AudioSource audioSource = audioConnections[nameOfSound];
		audioSource.transform.position = Vector3.zero;
		audioSource.pitch = Random.Range(minPitch, maxPitch);
		audioSource.Play();
		return audioSource;
	}

	public AudioSource PlaySound(string nameOfSound, Vector3 position, float minPitch = 0.8f, float maxPitch = 1.2f)
	{
		if (!audioConnections.ContainsKey(nameOfSound))
		{
			return null;
		}
		AudioSource audioSource = audioConnections[nameOfSound];
		audioSource.transform.position = position;
		audioSource.pitch = Random.Range(minPitch, maxPitch);
		audioSource.Play();
		return audioSource;
	}

	public AudioSource PlaySoundCopy(string nameOfSound, Vector3 position, bool destroyWhenComplete, float minPitch = 0.8f, float maxPitch = 1.2f)
	{
		if (!audioConnections.ContainsKey(nameOfSound))
		{
			return null;
		}
		AudioSource original = audioConnections[nameOfSound];
		GameObject gameObject = new GameObject("SoundCopy - " + nameOfSound);
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		CopyAudioSource(original, audioSource);
		gameObject.transform.position = position;
		audioSource.pitch = Random.Range(minPitch, maxPitch);
		audioSource.Play();
		if (destroyWhenComplete)
		{
			Object.Destroy(gameObject, audioSource.clip.length);
		}
		return audioSource;
	}

	private void CopyAudioSource(AudioSource original, AudioSource target)
	{
		target.clip = original.clip;
		target.volume = original.volume;
		target.loop = original.loop;
		target.spatialBlend = original.spatialBlend;
		target.outputAudioMixerGroup = original.outputAudioMixerGroup;
	}
}
