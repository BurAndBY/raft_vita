using UnityEngine;

[AddComponentMenu("azure[Sky]/Cloud Animation")]
[ExecuteInEditMode]
public class AzureSkyCloudAnimation : MonoBehaviour
{
	public Texture2D[] clouds;

	private Texture2D c1;

	private Texture2D c2;

	public int iniCloud;

	private int currentCloud;

	public float animationSpeed;

	private float lerp;

	private AzureSky_Controller skyController;

	private void Start()
	{
		skyController = GetComponent<AzureSky_Controller>();
		if (skyController != null)
		{
			currentCloud = iniCloud;
			if (clouds.Length > 1)
			{
				skyController.Sky_Material.SetTexture("_Cloud1", clouds[currentCloud]);
				skyController.Sky_Material.SetTexture("_Cloud2", clouds[currentCloud + 1]);
			}
		}
	}

	private void Update()
	{
		if (!(skyController != null) || clouds.Length != 120)
		{
			return;
		}
		lerp += animationSpeed * Time.deltaTime;
		if (lerp >= 1f)
		{
			if (currentCloud < 119)
			{
				currentCloud++;
			}
			else
			{
				currentCloud = 0;
			}
			if (currentCloud <= 119)
			{
				skyController.Sky_Material.SetTexture("_Cloud1", clouds[currentCloud]);
			}
			else
			{
				skyController.Sky_Material.SetTexture("_Cloud1", clouds[0]);
			}
			if (currentCloud <= 118)
			{
				skyController.Sky_Material.SetTexture("_Cloud2", clouds[currentCloud + 1]);
			}
			else
			{
				skyController.Sky_Material.SetTexture("_Cloud2", clouds[0]);
			}
			lerp = 0f;
		}
		skyController.Sky_Material.SetFloat("_CloudLerp", lerp);
	}

	public void setCloudSpeed(float speed)
	{
		animationSpeed = speed;
	}
}
