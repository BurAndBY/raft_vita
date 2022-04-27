using System.Collections.Generic;
using UnityEngine;

public class UseItemController : MonoBehaviour
{
	public List<ItemConnection> allConnections = new List<ItemConnection>();

	private IUsableItem usableItem;

	private Dictionary<ItemIndex, GameObject> connectionDictionary = new Dictionary<ItemIndex, GameObject>();

	private GameObject activeObject;

	private void Start()
	{
		foreach (ItemConnection allConnection in allConnections)
		{
			connectionDictionary.Add(allConnection.index, allConnection.obj);
			allConnection.obj.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if ((bool)usableItem && Input.GetButtonDown(usableItem.useButtonName))
		{
			Use();
		}
	}

	private void OnValidate()
	{
		foreach (ItemConnection allConnection in allConnections)
		{
			allConnection.name = allConnection.index.ToString();
		}
	}

	public void StartUsing(IUsableItem item)
	{
		if (connectionDictionary.ContainsKey(item.index))
		{
			usableItem = item;
			if (item.setAnimationIdle)
			{
				PlayerAnimator.SetAnimation(PlayerAnimation.Index_5_HoldItem, true, false);
			}
			activeObject = connectionDictionary[item.index];
			activeObject.SetActive(true);
			activeObject.SendMessage("OnSelect", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Deselect()
	{
		if (usableItem != null && connectionDictionary.ContainsKey(usableItem.index))
		{
			if (activeObject != null)
			{
				activeObject.SendMessage("OnDeSelect", SendMessageOptions.DontRequireReceiver);
				activeObject = null;
			}
			connectionDictionary[usableItem.index].SetActive(false);
		}
	}

	private void Use()
	{
		if (activeObject != null)
		{
			activeObject.SendMessage("OnUse", SendMessageOptions.DontRequireReceiver);
		}
	}
}
