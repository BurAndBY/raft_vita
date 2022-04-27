using UnityEngine;
using UnityEngine.UI;

public class SavedGameButton : MonoBehaviour
{
	public Text textComponent;

	[HideInInspector]
	public string savedGameName;

	public Button button;

	public bool dead;

	private static StartMenuScreen startMenuScreen;

	private void Start()
	{
		if (startMenuScreen == null)
		{
			startMenuScreen = Object.FindObjectOfType<StartMenuScreen>();
		}
	}

	public void SetText(string text)
	{
		textComponent.text = text;
	}

	public void Click()
	{
		if (startMenuScreen != null)
		{
			startMenuScreen.Select(this);
		}
	}

	public void SetDead()
	{
		dead = true;
		SpriteState spriteState = button.spriteState;
		spriteState.highlightedSprite = spriteState.disabledSprite;
		spriteState.pressedSprite = spriteState.disabledSprite;
		button.image.sprite = spriteState.disabledSprite;
		button.spriteState = spriteState;
	}
}
