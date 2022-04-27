using UnityEngine;

public class DeathMenu : SingletonGeneric<DeathMenu>
{
	public GameObject deathMenuParent;

	public GameObject canvasGame;

	public FadePanel blackFade;

	private void Start()
	{
		deathMenuParent.gameObject.SetActive(false);
	}

	public void SetGameOver()
	{
		GameManager.GameOver = true;
		string path = SingletonGeneric<SaveAndLoad>.Singleton.Path + GameManager.CurrentGameFileName;
		SingletonGeneric<SaveAndLoad>.Singleton.CorruptFile(path, "DEAD-");
		OpenMenu();
	}

	public void OpenMenu()
	{
		blackFade.SetAlpha(1f);
		PauseMenu.IsOpen = true;
		Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
		canvasGame.SetActive(false);
		deathMenuParent.SetActive(true);
	}
}
