using System;
using UnityEngine;

[Serializable]
public class SerializableTransform
{
	public float posX;

	public float posY;

	public float posZ;

	public float rotX;

	public float rotY;

	public float rotZ;

	public float scaleX;

	public float scaleY;

	public float scaleZ;

	public SerializableTransform(Transform transform)
	{
		posX = transform.position.x;
		posY = transform.position.y;
		posZ = transform.position.z;
		rotX = transform.eulerAngles.x;
		rotY = transform.eulerAngles.y;
		rotZ = transform.eulerAngles.z;
		scaleX = transform.localScale.x;
		scaleY = transform.localScale.y;
		scaleZ = transform.localScale.z;
	}

	public void SetTransform(Transform transform)
	{
		transform.position = new Vector3(posX, posY, posZ);
		transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
		transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
	}
}
