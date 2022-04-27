using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuScreen : MonoBehaviour
{
	[Header("Components")]
	public InputField newGameInput;

	public RectTransform selection;

	public RectTransform scrollViewContent;

	public Text deleteText;

	public Text feedbackText;

	[Header("Buttons")]
	public Transform buttonParent;

	public SavedGameButton savedGameButtonPrefab;

	public Button createNewButton;

	public Button playButton;

	public Button deleteButton;

	[Header("Layouts")]
	public GameObject newGameLayout;

	public GameObject standardLayout;

	public GameObject yesnoLayout;

	private SavedGameButton selectedButton;

	private void Start()
	{
		Time.timeScale = 1f;
		selection.gameObject.SetActive(false);
		yesnoLayout.SetActive(false);
		NewGameBack();
		LoadInSavedGames();
	}

	private void Update()
	{
		if (selectedButton != null)
		{
			selection.position = selectedButton.GetComponent<RectTransform>().position;
		}
		else
		{
			selection.gameObject.SetActive(false);
		}
		string text = newGameInput.text;
		if (text.Length > 10)
		{
			text = text.Substring(0, 10);
		}
		newGameInput.text = text;
		feedbackText.text = ((text.Length <= 0) ? "Enter a name for your game" : (text + ".rgd"));
		bool flag = File.Exists(SingletonGeneric<SaveAndLoad>.Singleton.Path + feedbackText.text);
		if (flag)
		{
			feedbackText.text = "File name already exists";
		}
		createNewButton.interactable = text.Length > 0 && !flag;
		if (selectedButton != null && !selectedButton.dead)
		{
			playButton.interactable = true;
		}
		else
		{
			playButton.interactable = false;
		}
		if (selectedButton != null)
		{
			deleteButton.interactable = true;
		}
		else
		{
			deleteButton.interactable = false;
		}
	}

	public void NewGame()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		standardLayout.SetActive(false);
		newGameLayout.SetActive(true);
	}

	public void NewGameBack()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		standardLayout.SetActive(true);
		newGameLayout.SetActive(false);
	}

	public void NewGameCreate()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		GameManager.CurrentGameFileName = newGameInput.text + ".rgd";
		GameManager.IsInNewGame = true;
		PlayScene("MainScene");
	}

	public void PlaySelected()
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		if (selectedButton != null)
		{
			GameManager.CurrentGameFileName = selectedButton.savedGameName;
			GameManager.IsInNewGame = false;
			PlayScene("MainScene");
		}
	}

	public void DeleteSelected()
	{
		if (selectedButton != null && File.Exists(SingletonGeneric<SaveAndLoad>.Singleton.Path + selectedButton.savedGameName))
		{
			File.Delete(SingletonGeneric<SaveAndLoad>.Singleton.Path + selectedButton.savedGameName);
			selectedButton = null;
			LoadInSavedGames();
		}
		yesnoLayout.SetActive(false);
		standardLayout.SetActive(true);
	}

	public void Select(SavedGameButton button)
	{
		SingletonGeneric<SoundManager>.Singleton.PlaySound("UIClick");
		if (button != null)
		{
			selectedButton = button;
			selection.gameObject.SetActive(true);
		}
	}

	public void DeleteButton()
	{
		if (!(selectedButton == null))
		{
			yesnoLayout.SetActive(true);
			standardLayout.SetActive(false);
			deleteText.text = "Are you sure that you want to delete\n" + selectedButton.savedGameName;
		}
	}

	public void DeleteNoButton()
	{
		yesnoLayout.SetActive(false);
		standardLayout.SetActive(true);
	}

	public void OptionsButton()
	{
		newGameLayout.SetActive(false);
		standardLayout.SetActive(false);
		yesnoLayout.SetActive(false);
		SingletonGeneric<Settings>.Singleton.OptionsMenu.Enter();
	}

	public void ExitApplication()
	{
		Application.Quit();
	}

	private void PlayScene(string name)
	{
		SceneManager.LoadScene(name);
	}

	private void LoadInSavedGames()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(SingletonGeneric<SaveAndLoad>.Singleton.Path);
		FileInfo[] files = directoryInfo.GetFiles();
		SingletonGeneric<SaveAndLoad>.Singleton.SavedGameNames = new string[files.Length];
		List<GameObject> list = new List<GameObject>();
		IEnumerator enumerator = buttonParent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Transform transform = (Transform)enumerator.Current;
				list.Add(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		list.ForEach(delegate(GameObject child)
		{
			UnityEngine.Object.Destroy(child);
		});
		for (int i = 0; i < files.Length; i++)
		{
			SingletonGeneric<SaveAndLoad>.Singleton.SavedGameNames[i] = files[i].Name;
			SavedGameButton savedGameButton = UnityEngine.Object.Instantiate(savedGameButtonPrefab, buttonParent);
			savedGameButton.transform.localScale = savedGameButtonPrefab.transform.localScale;
			string text = Enumerable.First<string>((IEnumerable<string>)files[i].Name.Split('.'));
			savedGameButton.SetText(text);
			savedGameButton.savedGameName = files[i].Name;
			if (savedGameButton.savedGameName.Contains("DEAD-") || savedGameButton.savedGameName.Contains("OldVersion-"))
			{
				savedGameButton.SetDead();
			}
		}
		int num = ((files.Length > 4) ? (files.Length - 4) : 0);
		scrollViewContent.sizeDelta = new Vector2(scrollViewContent.sizeDelta.x, 170 + 30 * num);
	}
}
