using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/UsableItem", order = 1)]
public class IUsableItem : IItem
{
	[Header("Usable settings")]
	public string useButtonName = "LeftClick";
}
