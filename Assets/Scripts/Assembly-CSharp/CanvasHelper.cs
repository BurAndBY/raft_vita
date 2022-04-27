using System;
using UnityEngine;
using UnityEngine.UI;

public class CanvasHelper : MonoBehaviour
{
	public delegate void OnMenuClose();

	private OnMenuClose menuCloseCallstack;

	public static CanvasHelper singleton;

	[Header("Components")]
	public BlockMenu blockmenu;

	[Header("Close/open on INVENTORY button")]
	public GameObject[] menus;

	[Header("Hunger")]
	public RectTransform healthTransform;

	public RectTransform hungerTransform;

	public RectTransform thirstTransform;

	public Image healthGlow;

	public Image hungerGlow;

	public Image thirstGlow;

	public float healthGlowValue;

	public float hungerGlowValue;

	public float thirstGlowValue;

	[Header("Fatigue")]
	public GameObject fatigueParent;

	public Image fatigueRadial;

	public Material fatigueShader;

	[Header("Remove Block Slider")]
	public Image removeBlockRadialImage;

	[Header("Others")]
	public GameObject EButton;

	public Text displayText;

	public GameObject gameOver;

	public GameObject aimCursor;

	private void Awake()
	{
		if (singleton == null)
		{
			singleton = this;
			Helper.aimCursor = aimCursor;
			SetRemoveBlock(false);
			SetEButton(false);
			SetDisplayText(false);
			SetFatigueSlider(false);
			Invoke("DelayedStart", 0.1f);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void DelayedStart()
	{
		CloseMenus();
	}

	private void Update()
	{
		if (!PlayerItemManager.IsBusy && !GameManager.IsInBuildMenu && !PauseMenu.IsOpen && !OptionsMenu.IsOpen)
		{
			bool buttonDown = Input.GetButtonDown("Inventory");
			bool flag = Input.GetButtonDown("Inventory") || Input.GetKeyDown(KeyCode.Escape);
			if (GameManager.IsInMenu && flag)
			{
				CloseMenus();
			}
			else if (buttonDown)
			{
				OpenMenus();
			}
		}
		Color color = healthGlow.color;
		color.a = ((!(healthGlowValue >= 0.25f)) ? ((Mathf.Sin(Time.time * (1f - healthGlowValue) * 5f) + 1f) / 2f) : 0f);
		healthGlow.color = color;
		color = thirstGlow.color;
		color.a = ((!(thirstGlowValue >= 0.25f)) ? ((Mathf.Sin(Time.time * (1f - thirstGlowValue) * 5f) + 1f) / 2f) : 0f);
		thirstGlow.color = color;
		color = hungerGlow.color;
		color.a = ((!(hungerGlowValue >= 0.25f)) ? ((Mathf.Sin(Time.time * (1f - hungerGlowValue) * 5f) + 1f) / 2f) : 0f);
		hungerGlow.color = color;
	}

	public void SubscribeToMenuClose(OnMenuClose closeMethod)
	{
		menuCloseCallstack = (OnMenuClose)Delegate.Combine(menuCloseCallstack, closeMethod);
	}

	public void CloseMenus()
	{
		GameManager.IsInMenu = false;
		GameObject[] array = menus;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(false);
		}
		Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
		if (menuCloseCallstack != null)
		{
			menuCloseCallstack();
		}
	}

	public void OpenMenus()
	{
		GameManager.IsInMenu = true;
		GameObject[] array = menus;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(true);
		}
		Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
	}

	public void SetHealthGlow(float normal)
	{
		healthGlowValue = normal;
	}

	public void SetHungerGlow(float normal)
	{
		hungerGlowValue = normal;
	}

	public void SetThirstGlow(float normal)
	{
		thirstGlowValue = normal;
	}

	public void SetHealthImagePosition(float yPos)
	{
		healthTransform.localPosition = new Vector3(0f, yPos);
	}

	public void SetHungerImagePosition(float yPos)
	{
		hungerTransform.localPosition = new Vector3(0f, yPos);
	}

	public void SetThirstImagePosition(float yPos)
	{
		thirstTransform.localPosition = new Vector3(0f, yPos);
	}

	public void SetFatigueSlider(float value)
	{
		fatigueRadial.fillAmount = value;
		fatigueShader.SetFloat("_FatigueValue", value);
	}

	public void SetFatigueSlider(bool state)
	{
		fatigueParent.gameObject.SetActive(state);
	}

	public void SetRemoveBlock(bool state)
	{
		if (removeBlockRadialImage != null)
		{
			removeBlockRadialImage.gameObject.SetActive(state);
		}
	}

	public void SetRemoveBlock(float value)
	{
		if (removeBlockRadialImage != null)
		{
			removeBlockRadialImage.fillAmount = value;
		}
	}

	public void SetEButton(bool state)
	{
		EButton.SetActive(state);
	}

	public void SetDisplayText(bool state)
	{
		if (displayText != null)
		{
			displayText.gameObject.SetActive(state);
		}
		if ((bool)EButton)
		{
			EButton.SetActive(state);
		}
	}

	public void SetDisplayText(string message, bool showEButton = false)
	{
		if (displayText != null)
		{
			displayText.gameObject.SetActive(true);
			displayText.text = message;
		}
		if (EButton != null)
		{
			EButton.SetActive(showEButton);
		}
	}

	public void SetGameOver(bool state)
	{
		gameOver.SetActive(state);
	}
}
