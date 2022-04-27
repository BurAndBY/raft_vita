using PlayWay.Water;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Settings : SingletonGeneric<Settings>
{
	private float mouseSensitivity;

	private bool invertYAxis;

	private float masterVolume;

	private float ambienceVolume;

	private float sfxVolume;

	private float uiVolume;

	private float musicVolume;

	private int waterQuality;

	private bool ambientOcclusion;

	private bool sunShafts;

	private bool cameraBob;

	private bool antialiasing;

	private OptionsMenu optionsMenu;

	public float MouseSensitivty
	{
		get
		{
			return mouseSensitivity;
		}
		private set
		{
			mouseSensitivity = value;
		}
	}

	public bool InvertYAxis
	{
		get
		{
			return invertYAxis;
		}
		private set
		{
			invertYAxis = value;
		}
	}

	public float MasterVolume
	{
		get
		{
			return masterVolume;
		}
		private set
		{
			masterVolume = value;
		}
	}

	public float AmbienceVolume
	{
		get
		{
			return ambienceVolume;
		}
		private set
		{
			ambienceVolume = value;
		}
	}

	public float SFXVolume
	{
		get
		{
			return sfxVolume;
		}
		private set
		{
			sfxVolume = value;
		}
	}

	public float UIVolume
	{
		get
		{
			return uiVolume;
		}
		private set
		{
			uiVolume = value;
		}
	}

	public float MusicVolume
	{
		get
		{
			return musicVolume;
		}
		private set
		{
			musicVolume = value;
		}
	}

	public int WaterQuality
	{
		get
		{
			return waterQuality;
		}
		private set
		{
			waterQuality = value;
		}
	}

	public bool AmbientOcclusion
	{
		get
		{
			return ambientOcclusion;
		}
		private set
		{
			ambientOcclusion = value;
		}
	}

	public bool SunShafts
	{
		get
		{
			return sunShafts;
		}
		private set
		{
			sunShafts = value;
		}
	}

	public bool CameraBob
	{
		get
		{
			return cameraBob;
		}
		private set
		{
			cameraBob = value;
		}
	}

	public bool Antialiasing
	{
		get
		{
			return antialiasing;
		}
		private set
		{
			antialiasing = value;
		}
	}

	public OptionsMenu OptionsMenu
	{
		get
		{
			if (optionsMenu == null)
			{
				optionsMenu = Object.FindObjectOfType<OptionsMenu>();
				return optionsMenu;
			}
			return optionsMenu;
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		LoadSettings();
	}

	public void SetMouseSensitivty(Slider slider)
	{
		MouseSensitivty = slider.value;
	}

	public void SetInvertYAxis(Toggle toggle)
	{
		InvertYAxis = toggle.isOn;
	}

	public void SetMasterVolume(Slider slider)
	{
		MasterVolume = slider.value;
		OptionsMenu.mixer.SetFloat("VolumeMaster", MasterVolume);
	}

	public void SetAmbienceVolume(Slider slider)
	{
		AmbienceVolume = slider.value;
		OptionsMenu.mixer.SetFloat("VolumeAmbience", AmbienceVolume);
	}

	public void SetSFXVolume(Slider slider)
	{
		SFXVolume = slider.value;
		OptionsMenu.mixer.SetFloat("VolumeSFX", SFXVolume);
	}

	public void SetMusicVolume(Slider slider)
	{
		MusicVolume = slider.value;
		OptionsMenu.mixer.SetFloat("VolumeMusic", MusicVolume);
	}

	public void SetUIVolume(Slider slider)
	{
		UIVolume = slider.value;
		OptionsMenu.mixer.SetFloat("VolumeUI", UIVolume);
	}

	public void SetWaterQuality(Slider slider)
	{
		WaterQuality = (int)slider.value;
		WaterQualitySettings.Instance.SetQualityLevel(WaterQuality);
	}

	public void SetAmbientOcclusion(Toggle toggle)
	{
		AmbientOcclusion = toggle.isOn;
		SetBehaviourOnCamera<ScreenSpaceAmbientOcclusion>(toggle.isOn);
	}

	public void SetSunshaft(Toggle toggle)
	{
		SunShafts = toggle.isOn;
		SetBehaviourOnCamera<SunShafts>(toggle.isOn);
	}

	public void SetCameraBob(Toggle toggle)
	{
		CameraBob = toggle.isOn;
		SetBehaviourOnCamera<CameraBob>(toggle.isOn);
	}

	public void SetAntialiasing(Toggle toggle)
	{
		Antialiasing = toggle.isOn;
		SetBehaviourOnCamera<Antialiasing>(toggle.isOn);
	}

	public void SaveSettings()
	{
		PlayerPrefs.SetFloat("MouseSensitivity", MouseSensitivty);
		PlayerPrefs.SetInt("InvertYAxis", InvertYAxis ? 1 : 0);
		PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
		PlayerPrefs.SetFloat("AmbienceVolume", AmbienceVolume);
		PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
		PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
		PlayerPrefs.SetFloat("UIVolume", UIVolume);
		PlayerPrefs.SetInt("WaterQuality", WaterQuality);
		PlayerPrefs.SetInt("AmbientOcclusion", AmbientOcclusion ? 1 : 0);
		PlayerPrefs.SetInt("SunShaft", SunShafts ? 1 : 0);
		PlayerPrefs.SetInt("CameraBob", CameraBob ? 1 : 0);
		PlayerPrefs.SetInt("Antialiasing", Antialiasing ? 1 : 0);
	}

	public void LoadSettings()
	{
		MouseSensitivty = PlayerPrefs.GetFloat("MouseSensitivity", 2.25f);
		InvertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1;
		OptionsMenu.MouseSensitivitySlider.value = MouseSensitivty;
		OptionsMenu.InvertYAxisToggle.isOn = InvertYAxis;
		MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0f);
		AmbienceVolume = PlayerPrefs.GetFloat("AmbienceVolume", 0f);
		SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);
		MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
		UIVolume = PlayerPrefs.GetFloat("UIVolume", 0f);
		OptionsMenu.masterVolumeSlider.value = MasterVolume;
		OptionsMenu.ambienceVolumeSlider.value = AmbienceVolume;
		OptionsMenu.sfxVolumeSlider.value = SFXVolume;
		OptionsMenu.musicVolumeSlider.value = MusicVolume;
		OptionsMenu.uiVolumeSlider.value = UIVolume;
		Invoke("SetMixerParameters", 0.001f);
		WaterQuality = PlayerPrefs.GetInt("WaterQuality", 5);
		WaterQualitySettings.Instance.SetQualityLevel(WaterQuality);
		OptionsMenu.waterQualitySlider.value = WaterQuality;
		AmbientOcclusion = PlayerPrefs.GetInt("AmbientOcclusion", 1) == 1;
		SunShafts = PlayerPrefs.GetInt("SunShaft", 1) == 1;
		CameraBob = PlayerPrefs.GetInt("CameraBob", 1) == 1;
		Antialiasing = PlayerPrefs.GetInt("Antialiasing", 1) == 1;
		SetBehaviourOnCamera<ScreenSpaceAmbientOcclusion>(AmbientOcclusion);
		SetBehaviourOnCamera<SunShafts>(SunShafts);
		SetBehaviourOnCamera<CameraBob>(CameraBob);
		SetBehaviourOnCamera<Antialiasing>(Antialiasing);
		OptionsMenu.ambientOcclusionToggle.isOn = AmbientOcclusion;
		OptionsMenu.sunshaftToggle.isOn = SunShafts;
		OptionsMenu.cameraBobToggle.isOn = CameraBob;
		OptionsMenu.antialisingToggle.isOn = Antialiasing;
	}

	private void SetMixerParameters()
	{
		OptionsMenu.mixer.SetFloat("VolumeMaster", MasterVolume);
		OptionsMenu.mixer.SetFloat("VolumeAmbience", AmbienceVolume);
		OptionsMenu.mixer.SetFloat("VolumeSFX", SFXVolume);
		OptionsMenu.mixer.SetFloat("VolumeMusic", MusicVolume);
		OptionsMenu.mixer.SetFloat("VolumeUI", UIVolume);
	}

	private void SetBehaviourOnCamera<T>(bool enabled) where T : Behaviour
	{
		T component = Camera.main.GetComponent<T>();
		if ((Object)component != (Object)null)
		{
			component.enabled = enabled;
		}
	}
}
