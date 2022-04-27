using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveAndLoad : SingletonGeneric<SaveAndLoad>
{
	[HideInInspector]
	public string Path;

	[HideInInspector]
	public string[] SavedGameNames;

	[HideInInspector]
	public Scene startScene;

	private void Awake()
	{
		if (UnityEngine.Object.FindObjectOfType<SaveAndLoad>() != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		startScene = SceneManager.GetActiveScene();
		Path = Application.persistentDataPath + "/SavedGames/";
		try
		{
			if (!Directory.Exists(Path))
			{
				Directory.CreateDirectory(Path);
				Debug.Log("Created new directory at: " + Path);
			}
			else
			{
				Debug.Log("Successfully found directory at: " + Path);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString() + " _|_ " + ex.Message);
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Save()
	{
		RGD_Game graph = CreateRGDGame();

		string jsontext = JsonUtility.ToJson(graph);
		PlayerPrefs.SetString(GameManager.CurrentGameFileName, jsontext);

		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Create(Path + GameManager.CurrentGameFileName);
		binaryFormatter.Serialize(fileStream, graph);
		fileStream.Close();
		Debug.Log("Saved Map: Succes");
	}

	public void Load()
	{
		if(PlayerPrefs.HasKey(GameManager.CurrentGameFileName))
        {
			RGD_Game rGD_Game = null;
			rGD_Game = JsonUtility.FromJson<RGD_Game>(PlayerPrefs.GetString(GameManager.CurrentGameFileName));

			if (rGD_Game != null)
			{
				RestoreRGDGame(rGD_Game);
				Debug.Log("Load Map: Success");
			}
		}

		//if (File.Exists(Path + GameManager.CurrentGameFileName))
		//{
		//	BinaryFormatter binaryFormatter = new BinaryFormatter();
		//	FileStream fileStream = File.Open(Path + GameManager.CurrentGameFileName, FileMode.Open);
		//	RGD_Game rGD_Game = null;
		//	try
		//	{
		//		rGD_Game = binaryFormatter.Deserialize(fileStream) as RGD_Game;
		//	}
		//	catch (Exception message)
		//	{
		//		Debug.Log(message);
		//		fileStream.Close();
		//		CorruptFile(Path + GameManager.CurrentGameFileName, "OldVersion-");
		//		Helper.SetCursorVisibleAndLockState(true, CursorLockMode.None);
		//		SceneManager.LoadScene("MainMenuScene");
		//		return;
		//	}
		//	if (rGD_Game != null)
		//	{
		//		RestoreRGDGame(rGD_Game);
		//		Debug.Log("Load Map: Success");
		//	}
		//	fileStream.Close();
		//}
	}

	public void CorruptFile(string path, string textOnFile)
	{
		if (File.Exists(path))
		{
			File.Copy(path, Path + textOnFile + GameManager.CurrentGameFileName, true);
			File.Delete(path);
			Debug.Log("Corrupted file: " + path + "\nWith message: " + textOnFile);
		}
	}

	private RGD_Game CreateRGDGame()
	{
		RGD_Game rGD_Game = new RGD_Game();
		rGD_Game.globalRaftTimeOffset = GameManager.singleton.globalRaftParent.GetComponent<BobTransform>().timeOffset;
		rGD_Game.globalRaftSerializableTransform = new SerializableTransform(GameManager.singleton.globalRaftParent);
		rGD_Game.sky = new RGD_Sky(GameManager.singleton.skyController);
		List<Block> allPlacedBlocks = UnityEngine.Object.FindObjectOfType<BlockPlacer>().allPlacedBlocks;
		for (int i = 0; i < allPlacedBlocks.Count; i++)
		{
			RGD_Block rGD_Block = new RGD_Block();
			rGD_Block.serializableTransform = new SerializableTransform(allPlacedBlocks[i].transform);
			rGD_Block.type = allPlacedBlocks[i].type;
			rGD_Block.health = allPlacedBlocks[i].health;
			rGD_Block.blockIndex = allPlacedBlocks[i].blockIndex;
			rGD_Game.blocks.Add(rGD_Block);
		}
		PlayerStats playerStats = UnityEngine.Object.FindObjectOfType<PlayerStats>();
		RGD_Player player = rGD_Game.player;
		player.serializableTransform = new SerializableTransform(playerStats.transform);
		player.hunger = playerStats.Hunger;
		player.thirst = playerStats.Thirst;
		player.health = playerStats.Health;
		player.fatigue = playerStats.Fatigue;
		RGD_Inventory item = new RGD_Inventory(-1, PlayerInventory.Singleton);
		rGD_Game.inventories.Insert(0, item);
		Chest[] array = UnityEngine.Object.FindObjectsOfType<Chest>();
		if (array.Length > 0)
		{
			for (int j = 0; j < array.Length; j++)
			{
				Inventory inventoryReference = array[j].GetInventoryReference();
				Block component = array[j].GetComponent<Block>();
				if (!(inventoryReference == null) && !(component == null))
				{
					RGD_Inventory item2 = new RGD_Inventory(component.blockIndex, inventoryReference);
					rGD_Game.inventories.Add(item2);
				}
			}
		}
		CookingStand[] array2 = UnityEngine.Object.FindObjectsOfType<CookingStand>();
		if (array2.Length > 0)
		{
			foreach (CookingStand stand in array2)
			{
				RGD_CookingStand item3 = new RGD_CookingStand(stand);
				rGD_Game.cookingStands.Add(item3);
			}
		}
		Cropplot[] array3 = UnityEngine.Object.FindObjectsOfType<Cropplot>();
		if (array3.Length > 0)
		{
			foreach (Cropplot plot in array3)
			{
				RGD_Cropplot item4 = new RGD_Cropplot(plot);
				rGD_Game.cropplots.Add(item4);
			}
		}
		Net[] array4 = UnityEngine.Object.FindObjectsOfType<Net>();
		if (array4.Length > 0)
		{
			foreach (Net net in array4)
			{
				RGD_ItemNet item5 = new RGD_ItemNet(net);
				rGD_Game.itemNets.Add(item5);
			}
		}
		return rGD_Game;
	}

	private void RestoreRGDGame(RGD_Game game)
	{
		if (game.sky != null)
		{
			game.sky.RestoreSky(GameManager.singleton.skyController);
		}
		PlayerStats playerStats = GameManager.singleton.playerStats;
		if (playerStats != null)
		{
			RGD_Player player = game.player;
			player.RestorePlayer(playerStats);
		}
		game.inventories[0].RestoreInventory(PlayerInventory.Singleton);
		PlayerInventory.Singleton.hotbar.SelectHotslot(PlayerInventory.Singleton.GetSlot(game.inventories[0].hotslotIndex));
		PlayerInventory.Singleton.hotbar.SetSelectedSlotIndex(game.inventories[0].hotslotIndex);
		GameManager.singleton.globalRaftParent.GetComponent<BobTransform>().timeOffset = game.globalRaftTimeOffset;
		game.globalRaftSerializableTransform.SetTransform(GameManager.singleton.globalRaftParent);
		Block.GlobalIndex = 0;
		BlockPlacer blockPlacer = UnityEngine.Object.FindObjectOfType<BlockPlacer>();
		if (!(blockPlacer != null))
		{
			return;
		}
		List<Block> list = new List<Block>();
		for (int i = 0; i < game.blocks.Count; i++)
		{
			RGD_Block rGD_Block = game.blocks[i];
			Block blockFromType = blockPlacer.GetBlockFromType(rGD_Block.type);
			Transform transform = UnityEngine.Object.Instantiate(blockFromType.prefab, Vector3.zero, blockFromType.prefab.transform.rotation).transform;
			transform.transform.SetParent(GameManager.singleton.globalRaftParent);
			rGD_Block.serializableTransform.SetTransform(transform);
			Block component = transform.GetComponent<Block>();
			component.health = rGD_Block.health;
			component.blockIndex = rGD_Block.blockIndex;
			if (component.blockIndex > Block.GlobalIndex)
			{
				Block.GlobalIndex = component.blockIndex;
			}
			component.RefreshOverlapps();
			blockPlacer.allPlacedBlocks.Add(component);
			component.overlappingComponent.LookForOverlapps(false);
			if (component.specialySaved)
			{
				list.Add(component);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Block specialBlock = list[j];
			if (RestoreCookingStand(game, specialBlock) || RestoreCropplot(game, specialBlock) || RestoreItemNet(game, specialBlock) || RestoreChest(game, specialBlock))
			{
			}
		}
	}

	private bool RestoreCookingStand(RGD_Game game, Block specialBlock)
	{
		if (game == null || game.cookingStands == null)
		{
			return false;
		}
		for (int i = 0; i < game.cookingStands.Count; i++)
		{
			RGD_CookingStand rGD_CookingStand = game.cookingStands[i];
			if (rGD_CookingStand.blockIndex == specialBlock.blockIndex)
			{
				CookingStand component = specialBlock.GetComponent<CookingStand>();
				if (component != null)
				{
					rGD_CookingStand.RestoreStand(component);
					return true;
				}
				Debug.Log("Could not find cookingstand component on " + specialBlock.transform.name + "... May be wrong indexing on blocks");
			}
		}
		return false;
	}

	private bool RestoreCropplot(RGD_Game game, Block specialBlock)
	{
		if (game == null || game.cropplots == null)
		{
			return false;
		}
		for (int i = 0; i < game.cropplots.Count; i++)
		{
			RGD_Cropplot rGD_Cropplot = game.cropplots[i];
			if (rGD_Cropplot.blockIndex == specialBlock.blockIndex)
			{
				Cropplot component = specialBlock.GetComponent<Cropplot>();
				if (component != null)
				{
					rGD_Cropplot.RestorePlot(component);
					return true;
				}
				Debug.Log("Could not find cropplot component on " + specialBlock.transform.name + "... May be wrong indexing on blocks");
			}
		}
		return false;
	}

	private bool RestoreChest(RGD_Game game, Block specialBlock)
	{
		if (game == null || game.inventories == null)
		{
			return false;
		}
		for (int i = 1; i < game.inventories.Count; i++)
		{
			RGD_Inventory rGD_Inventory = game.inventories[i];
			if (rGD_Inventory.blockIndex != specialBlock.blockIndex)
			{
				continue;
			}
			Chest component = specialBlock.GetComponent<Chest>();
			if (component != null)
			{
				Inventory inventoryReference = component.GetInventoryReference();
				if (inventoryReference != null)
				{
					rGD_Inventory.RestoreInventory(inventoryReference);
					return true;
				}
			}
			else
			{
				Debug.Log("Could not find chest component on " + specialBlock.transform.name + "... May be wrong indexing on blocks");
			}
		}
		return false;
	}

	private bool RestoreItemNet(RGD_Game game, Block specialBlock)
	{
		if (game == null || game.itemNets == null)
		{
			return false;
		}
		for (int i = 0; i < game.itemNets.Count; i++)
		{
			RGD_ItemNet rGD_ItemNet = game.itemNets[i];
			if (rGD_ItemNet.blockIndex == specialBlock.blockIndex)
			{
				Net component = specialBlock.GetComponent<Net>();
				if (component != null)
				{
					rGD_ItemNet.RestoreItemNet(component);
					return true;
				}
				Debug.Log("Could not find Net component on " + specialBlock.transform.name + "... May be wrong indexing on blocks");
			}
		}
		return false;
	}
}
