using UnityEngine;

public class Player : MonoBehaviour
{
	public static float UseDistance = 2.5f;

	public MouseLook mouseLookXScript;

	public MouseLook mouseLookYScript;

	private new static Transform transform;

	private void Start()
	{
		transform = base.gameObject.transform;
		if (GameManager.IsInNewGame)
		{
			PlayerInventory.Singleton.AddItem(ItemIndex.Hook, 1);
		}
	}

	private void Update()
	{
	}

	public static void LockPlayerControls()
	{
		PersonController personController = Object.FindObjectOfType<PersonController>();
		personController.enabled = !personController.enabled;
	}

	public static bool IsWithingDistance(Vector3 position)
	{
		float num = Vector3.Distance(transform.position, position);
		return num <= UseDistance;
	}

	public static bool IsWithingDistance(Vector3 position, float requiredDistance)
	{
		float num = Vector3.Distance(transform.position, position);
		return num <= requiredDistance;
	}

	public bool MouseLookIsActive()
	{
		return mouseLookXScript.enabled && mouseLookYScript.enabled;
	}

	public void SetMouseLookScripts(bool canLook)
	{
		MouseLook mouseLook = mouseLookXScript;
		bool flag = canLook;
		mouseLookYScript.enabled = flag;
		mouseLook.enabled = flag;
	}
}
