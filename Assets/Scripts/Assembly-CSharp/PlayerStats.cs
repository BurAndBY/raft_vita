using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	[Header("Components")]
	public PersonController personController;

	[Header("Mask settings")]
	public float hungerBottomY;

	public float thirstBottomY;

	public float healthBottomY;

	[Tooltip("Per seconds")]
	public float decreaseAmount = 1f;

	public float fatigueDecreaseAmount = 1f;

	[Header("Sounds")]
	public AudioSource hungrySound;

	public AudioSource thirstySound;

	public AudioSource fatigueSound;

	public AudioSource injuredSound;

	private AudioSource playerScreamSource;

	private float hunger;

	private float thirst;

	private float health;

	private float fatigue;

	public float Hunger
	{
		get
		{
			return hunger;
		}
		set
		{
			hunger = value;
			hunger = Mathf.Clamp(hunger, 0f, 100f);
		}
	}

	public float Thirst
	{
		get
		{
			return thirst;
		}
		set
		{
			thirst = value;
			thirst = Mathf.Clamp(thirst, 0f, 100f);
		}
	}

	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
			health = Mathf.Clamp(health, 0f, 100f);
		}
	}

	public float Fatigue
	{
		get
		{
			return fatigue;
		}
		set
		{
			fatigue = value;
			fatigue = Mathf.Clamp(fatigue, 0f, 100f);
		}
	}

	private void Start()
	{
		Hunger = 100f;
		Thirst = 100f;
		Health = 100f;
		Fatigue = 100f;
		hungrySound.gameObject.SetActive(false);
		thirstySound.gameObject.SetActive(false);
		fatigueSound.gameObject.SetActive(false);
		injuredSound.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (personController != null)
		{
			if (personController.inWater)
			{
				Fatigue -= fatigueDecreaseAmount * Time.deltaTime;
			}
			else
			{
				Fatigue += fatigueDecreaseAmount * 3f * Time.deltaTime;
			}
		}
		float num = 1f - Fatigue / 100f;
		CanvasHelper.singleton.SetFatigueSlider(num);
		CanvasHelper.singleton.SetFatigueSlider(num > 0f);
		fatigueSound.gameObject.SetActive(Fatigue / 100f <= 0.25f);
		Hunger -= decreaseAmount * 0.33f * Time.deltaTime;
		Thirst -= decreaseAmount * 0.75f * Time.deltaTime;
		if (Hunger <= 0f || Thirst <= 0f || Fatigue <= 0f)
		{
			Health -= decreaseAmount * 10f * Time.deltaTime;
		}
		if (Health <= 0f)
		{
			Object.Destroy(Object.FindObjectOfType<PersonController>());
			SingletonGeneric<DeathMenu>.Singleton.SetGameOver();
		}
		float num2 = Mathf.Abs(Hunger / 100f - 1f);
		float hungerImagePosition = num2 * hungerBottomY;
		CanvasHelper.singleton.SetHungerImagePosition(hungerImagePosition);
		CanvasHelper.singleton.SetHungerGlow(Hunger / 100f);
		hungrySound.gameObject.SetActive(Hunger / 100f <= 0.25f);
		num2 = Mathf.Abs(Thirst / 100f - 1f);
		hungerImagePosition = num2 * thirstBottomY;
		CanvasHelper.singleton.SetThirstImagePosition(hungerImagePosition);
		CanvasHelper.singleton.SetThirstGlow(Thirst / 100f);
		thirstySound.gameObject.SetActive(Thirst / 100f <= 0.25f);
		num2 = Mathf.Abs(Health / 100f - 1f);
		hungerImagePosition = num2 * healthBottomY;
		CanvasHelper.singleton.SetHealthImagePosition(hungerImagePosition);
		CanvasHelper.singleton.SetHealthGlow(Health / 100f);
		injuredSound.gameObject.SetActive(Health / 100f <= 0.25f);
	}

	public void AddHunger(float amount)
	{
		Hunger += amount;
	}

	public void AddThirst(float amount)
	{
		Thirst += amount;
	}

	public void AddHealth(float amount)
	{
		Health += amount;
		if (amount < 0f)
		{
			if (playerScreamSource == null)
			{
				playerScreamSource = SingletonGeneric<SoundManager>.Singleton.PlaySound("PlayerDamage");
			}
			else if (!playerScreamSource.isPlaying)
			{
				SingletonGeneric<SoundManager>.Singleton.PlaySound("PlayerDamage");
			}
		}
	}
}
