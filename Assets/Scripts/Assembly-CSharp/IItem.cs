using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Item", order = 1)]
public class IItem : ScriptableObject
{
	[Header("Item settings")]
	public bool setAnimationIdle;

	public ItemIndex index;

	public GameObject prefab;

	public Sprite sprite;

	public int stackSize;

	public string displayName;

	[TextArea]
	public string displayDescription;

	public bool IsStackable()
	{
		return stackSize > 0;
	}
}
