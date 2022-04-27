using System;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
	public PersonController controller;

	public float bobbingSpeed = 0.18f;

	public float bobbingSpeedSprint = 0.18f;

	public float bobbingAmount = 0.2f;

	public float midpoint = 2f;

	private float timer;

	private void Update()
	{
		if (controller.inWater || PauseMenu.IsOpen || GameManager.IsInMenu)
		{
			return;
		}
		float num = 0f;
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		float num2 = ((!controller.sprinting) ? bobbingSpeed : bobbingSpeedSprint);
		Vector3 localPosition = base.transform.localPosition;
		if (Mathf.Abs(axis) == 0f && Mathf.Abs(axis2) == 0f)
		{
			timer = 0f;
		}
		else
		{
			num = Mathf.Sin(timer);
			timer += num2;
			if (timer > (float)Math.PI * 2f)
			{
				timer -= (float)Math.PI * 2f;
			}
		}
		if (num != 0f)
		{
			float num3 = num * bobbingAmount;
			float value = Mathf.Abs(axis) + Mathf.Abs(axis2);
			value = Mathf.Clamp(value, 0f, 1f);
			num3 = value * num3;
			localPosition.y = midpoint + num3;
		}
		else
		{
			localPosition.y = midpoint;
		}
		base.transform.localPosition = localPosition;
	}
}
