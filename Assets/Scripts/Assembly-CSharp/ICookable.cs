using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Item/Cookable", order = 1)]
public class ICookable : IEdible
{
	[Header("Cookable settings")]
	public float cooktime;

	public IEdible result;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
