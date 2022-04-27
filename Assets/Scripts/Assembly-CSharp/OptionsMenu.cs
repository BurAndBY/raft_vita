using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	public static bool IsOpen;

	public GameObject optionParent;

	public GameObject[] categoryParents;

	public Text categoryLabel;

	[Header("Audio components")]
	public AudioMixer mixer;

	public Slider masterVolumeSlider;

	public Slider ambienceVolumeSlider;

	public Slider sfxVolumeSlider;

	public Slider musicVolumeSlider;

	public Slider uiVolumeSlider;

	[Header("Controls components")]
	public Slider MouseSensitivitySlider;

	public Toggle InvertYAxisToggle;

	[Header("Video components")]
	public Slider waterQualitySlider;

	public Toggle antialisingToggle;

	public Toggle cameraBobToggle;

	public Toggle sunshaftToggle;

	public Toggle ambientOcclusionToggle;

	private GameObject currentCategory;

	private void Awake()
	{
		optionParent.SetActive(false);
	}

	private void Update()
	{
		if (IsOpen && Input.GetKeyDown(KeyCode.JoystickButton7))
		{
			BackButton();
			Exit();
		}
	}

	public void OpenCategory(GameObject category)
	{
		if (category == null)
		{
			return;
		}
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIButtonClick");
		GameObject[] array = categoryParents;
		foreach (GameObject gameObject in array)
		{
			bool flag = gameObject == category;
			gameObject.SetActive(flag);
			if (flag)
			{
				currentCategory = gameObject;
				categoryLabel.text = currentCategory.name;
			}
		}
		SingletonGeneric<Settings>.Singleton.SaveSettings();
	}

	public void BackButton()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIButtonClick");
		if (SceneManager.GetActiveScene().name == "MainMenuScene")
		{
			StartMenuScreen startMenuScreen = Object.FindObjectOfType<StartMenuScreen>();
			if (startMenuScreen != null)
			{
				startMenuScreen.standardLayout.SetActive(true);
				startMenuScreen.newGameLayout.SetActive(false);
				Exit();
			}
		}
		else
		{
			PauseMenu pauseMenu = Object.FindObjectOfType<PauseMenu>();
			if (pauseMenu != null)
			{
				pauseMenu.pauseMenuParent.SetActive(true);
			}
		}
	}

	public void Exit()
	{
		IsOpen = false;
		SingletonGeneric<Settings>.Singleton.SaveSettings();
		optionParent.SetActive(false);
	}

	public void Enter()
	{
		IsOpen = true;
		optionParent.SetActive(true);
		SingletonGeneric<Settings>.Singleton.LoadSettings();
		if (currentCategory == null)
		{
			currentCategory = categoryParents[0];
		}
		OpenCategory(currentCategory);
	}
}
