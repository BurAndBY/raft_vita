using UnityEngine;

public class BobTransform : MonoBehaviour
{
	public bool randomizeTimeOffset;

	[Header("Up and down settings")]
	[Space(10f)]
	public float frequency = 20f;

	public float magnitude = 0.5f;

	public Vector3 waterOffset;

	[Header("Rotate settings")]
	public float frequencyRotation = 20f;

	public float magnitudeRotation = 0.5f;

	public Vector3 rotationOffset;

	public Transform rotatePoint;

	public float angle;

	private Vector3 startPos;

	public float timeOffset;

	public Vector3 targetEuler;

	public float sin;

	private void Start()
	{
		startPos = base.transform.position;
		targetEuler = base.transform.eulerAngles;
		if (randomizeTimeOffset)
		{
			timeOffset = Random.Range(-10000, 10000);
		}
	}

	private void Update()
	{
		startPos.x = base.transform.position.x;
		startPos.z = base.transform.position.z;
		base.transform.position = startPos + waterOffset + Vector3.up * Mathf.Sin((Time.time + timeOffset) * frequency) * magnitude;
	}
}
