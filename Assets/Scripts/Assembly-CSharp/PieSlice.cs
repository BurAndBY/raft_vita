using System;
using UnityEngine;
using UnityEngine.UI;

public class PieSlice : MonoBehaviour
{
	public Image radialImage;

	public Image centerImage;

	public Vector2 centerImageSize;

	[Range(0f, 1f)]
	public float imageOffsetFromCenter;

	private void OnValidate()
	{
		SetImageAtCenterOfSlice();
	}

	public void Initialize()
	{
		SetImageAtCenterOfSlice();
	}

	private void SetImageAtCenterOfSlice()
	{
		float z = radialImage.transform.eulerAngles.z;
		float num = radialImage.fillAmount * 360f;
		float num2 = num / 2f + z;
		float f = num2 * ((float)Math.PI / 180f);
		Vector3 vector = new Vector3(Mathf.Cos(f), Mathf.Sin(f));
		vector.Normalize();
		if (centerImage != null)
		{
			RectTransform component = radialImage.GetComponent<RectTransform>();
			RectTransform component2 = centerImage.GetComponent<RectTransform>();
			Vector3[] array = new Vector3[4];
			component.GetWorldCorners(array);
			Vector3 vector2 = array[2] - component.position;
			component2.position = component.position + vector2 * imageOffsetFromCenter;
			float num3 = Vector3.Distance(component2.position, component.position);
			component2.position = component.position + vector * num3;
			component2.eulerAngles = Vector3.zero;
			component2.sizeDelta = centerImageSize;
		}
	}
}
