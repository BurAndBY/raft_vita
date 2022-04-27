using UnityEngine;
using UnityEngine.UI;

public class InventoryPickupMenuItem : MonoBehaviour
{
	public Text amountTextComponent;

	public Text nameTextComponent;

	public Image imageComponent;

	[HideInInspector]
	public RectTransform rect;

	[HideInInspector]
	public int index;

	private CanvasGroup canvasGroup;

	[SerializeField]
	private float fadeSpeed;

	private bool fade;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void Update()
	{
		Vector3 localPosition = rect.transform.localPosition;
		localPosition.y = Mathf.Lerp(localPosition.y, (float)index * rect.sizeDelta.y, Time.deltaTime * 4f);
		rect.transform.localPosition = localPosition;
		if (fade)
		{
			canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
			if (canvasGroup.alpha <= 0f)
			{
				base.gameObject.SetActive(false);
				rect.localPosition = new Vector3(0f, -2f * rect.sizeDelta.y, 0f);
			}
		}
	}

	public void SetItem(IItem item, int amount)
	{
		amountTextComponent.text = "+" + amount;
		nameTextComponent.text = item.displayName;
		imageComponent.sprite = item.sprite;
		canvasGroup.alpha = 1f;
		fade = false;
		Invoke("StartFade", 3.5f);
	}

	private void StartFade()
	{
		fade = true;
	}
}
