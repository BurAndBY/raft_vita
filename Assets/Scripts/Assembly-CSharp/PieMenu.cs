using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieMenu : MonoBehaviour
{
	[Header("Pie settings")]
	public Image piePrefab;

	[Header("ColorSettings")]
	public Color normalColor = Color.white;

	public Color highlightColor = Color.gray;

	private List<PieSlice> slices = new List<PieSlice>();

	private float anglePerButton;

	private Vector2 fakeBoxPos;

	private Vector2 prevMousepos;

	private int selectedIndex;

	private int previousIndex;

	private void Update()
	{
		GetItemInMenu();
	}

	public void Create(int pieCount)
	{
		anglePerButton = 360f / (float)pieCount;
		for (int i = 0; i < pieCount; i++)
		{
			GameObject gameObject = Object.Instantiate(piePrefab.gameObject, base.transform);
			gameObject.transform.localScale = piePrefab.transform.localScale;
			PieSlice component = gameObject.GetComponent<PieSlice>();
			Image component2 = gameObject.GetComponent<Image>();
			component2.type = Image.Type.Filled;
			component2.fillMethod = Image.FillMethod.Radial360;
			component2.fillOrigin = 1;
			component2.fillAmount = anglePerButton / 360f;
			component2.fillClockwise = false;
			RectTransform component3 = component2.GetComponent<RectTransform>();
			component3.localPosition = Vector2.zero;
			component3.localEulerAngles = new Vector3(0f, 0f, (0f - anglePerButton) * (float)i);
			Vector2 vector2 = (component3.offsetMin = (component3.offsetMax = Vector2.zero));
			slices.Add(component);
			component.Initialize();
		}
		selectedIndex = (previousIndex = 0);
	}

	public PieSlice GetSlice(int index)
	{
		if (index >= 0 && index < slices.Count)
		{
			return slices[index];
		}
		return null;
	}

	private void GetItemInMenu()
	{
		Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
		Vector2 vector2 = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		Vector2 vector3 = vector2 - prevMousepos;
		vector3.Normalize();
		fakeBoxPos += vector3 * 20f;
		float num = Vector2.Distance(fakeBoxPos, vector);
		if (num > 100f)
		{
			fakeBoxPos = vector + (fakeBoxPos - vector).normalized * 100f;
		}
		else if (num < 20f)
		{
			fakeBoxPos = vector + (fakeBoxPos - vector).normalized * 20f;
		}
		Vector2 vector4 = fakeBoxPos - vector;
		vector4.Normalize();
		float num2 = 360f - Mathf.Atan2(vector4.y, vector4.x) * 57.29578f - 270f;
		if (num2 < 0f)
		{
			num2 += 360f;
		}
		int num3 = (int)(num2 / anglePerButton);
		if (num3 != selectedIndex && num3 >= 0 && num3 < slices.Count)
		{
			previousIndex = selectedIndex;
			selectedIndex = num3;
			Image radialImage = slices[num3].radialImage;
			Image radialImage2 = slices[previousIndex].radialImage;
			radialImage2.color = normalColor;
			radialImage.color = highlightColor;
		}
		prevMousepos = vector2;
	}

	public int GetPressButtonIndex()
	{
		return selectedIndex;
	}
}
