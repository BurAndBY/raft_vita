using UnityEngine;

public class GameManager : MonoBehaviour
{
	public bool loadCheatGame;

	public static string VersionNumber = "Version 1.05b";

	public static GameManager singleton;

	public static bool IsInNewGame;

	public static bool IsInMenu;

	public static bool IsInBuildMenu;

	public static bool GameOver;

	public static string CurrentGameFileName;

	[HideInInspector]
	public PlayerStats playerStats;

	[HideInInspector]
	public Player player;

	public AzureSky_Controller skyController;

	public Texture2D cursorTexture;

	public Transform globalRaftParent;

	public Transform waterFlowParent;

	public FadePanel blackFade;

	private void Awake()
	{
		Application.targetFrameRate = 120;
		Helper.Initialize();
		Inventory.Parent = GameObject.FindWithTag("InventoryParent").transform;
		singleton = this;
		skyController = Object.FindObjectOfType<AzureSky_Controller>();
		player = Object.FindObjectOfType<Player>();
		playerStats = player.GetComponent<PlayerStats>();
		IsInMenu = false;
		GameOver = false;
	}

	private void Start()
	{
		float time = 2.5f;
		Invoke("DelayedStart", time);
		blackFade.SetAlpha(1f);
		Player.LockPlayerControls();
	}

	private void DelayedStart()
	{
		StartCoroutine(blackFade.FadeToAlpha(0f, 2f));
		SingletonGeneric<BlockPlacer>.Singleton.SetStartingBlockCollision();
		Player.LockPlayerControls();
		if (!IsInNewGame)
		{
			SingletonGeneric<SaveAndLoad>.Singleton.Load();
		}
	}

	private void Update()
	{
		Helper.Update();
		Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
	}
}
