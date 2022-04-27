using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	private delegate void ExitMethod(bool forceExit);

	public static bool IsOpen;

	public GameObject pauseMenuParent;

	public GameObject yesNoSaveLayout;

	public Button saveButton;

	public Text saveButtonText;

	private bool hasSavedGame;

	private ExitMethod exitMethod;

	private void Start()
	{
		Time.timeScale = 1f;
		CloseMenu();
	}

	private void Update()
	{
		if (IsOpen && !GameManager.IsInMenu)
		{
			GameManager.IsInMenu = true;
		}
		if (Input.GetKeyDown(KeyCode.JoystickButton7))
		{
			if (!IsOpen)
			{
				OpenPauseMenu();
			}
			else if (!OptionsMenu.IsOpen)
			{
				CloseMenu();
			}
		}
		saveButton.interactable = !hasSavedGame;
		saveButtonText.text = ((!saveButton.interactable) ? "Game is saved" : "Save game");
	}

	public void ContinuePlaying()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		CloseMenu();
	}

	public void SaveGame()
	{
		if (!hasSavedGame)
		{
			hasSavedGame = true;
			SingletonGeneric<SaveAndLoad>.Singleton.Save();
		}
	}

	public void ExitToMainMenu(bool forceExit = false)
	{
		if (!hasSavedGame && !forceExit)
		{
			SetYesNoSaveLayout(true);
			exitMethod = ExitToMainMenu;
		}
		else
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
			SceneManager.LoadScene("MainMenuScene");
			Time.timeScale = 1f;
		}
	}

	public void ExitApplication(bool forceExit = false)
	{
		if (!hasSavedGame && !forceExit)
		{
			SetYesNoSaveLayout(true);
			exitMethod = ExitApplication;
		}
		else
		{
			SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
			Application.Quit();
		}
	}

	public void ExitWithSetMethod(bool forceExit)
	{
		if (exitMethod != null)
		{
			exitMethod(forceExit);
		}
	}

	public void SetYesNoSaveLayout(bool state)
	{
		yesNoSaveLayout.gameObject.SetActive(state);
		if (!state)
		{
			exitMethod = null;
		}
	}

	public void OpenPauseMenu()
	{
		if (!GameManager.IsInMenu)
		{
			IsOpen = true;
			Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
			Time.timeScale = 0f;
			pauseMenuParent.SetActive(true);
		}
	}

	public void CloseMenu()
	{
		Time.timeScale = 1f;
		IsOpen = false;
		GameManager.IsInMenu = false;
		Helper.SetCursorVisibleAndLockState(false, CursorLockMode.Locked);
		exitMethod = null;
		pauseMenuParent.SetActive(false);
		SetYesNoSaveLayout(false);
		hasSavedGame = false;
	}

	public void OpenOptionMenu()
	{
		pauseMenuParent.SetActive(false);
		SingletonGeneric<Settings>.Singleton.OptionsMenu.Enter();
	}
}
