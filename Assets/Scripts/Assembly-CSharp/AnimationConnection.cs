using System;
using UnityEngine;

[Serializable]
public class AnimationConnection
{
	[HideInInspector]
	public string name;

	public string functionName;

	public GameObject objectReciever;

	public AnimationClip clip;

	[Tooltip("In seconds")]
	public float timeOfEvent;
}
