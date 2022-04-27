using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("azure[Sky]/Sky Controller")]
public class AzureSky_Controller : MonoBehaviour
{
	public bool showTimeOfDay = true;

	public bool showObj_and_Mat;

	public bool showScattering;

	public bool showSkySettings;

	public bool showFogSettings;

	public bool showCloudSettings;

	public bool showAmbient;

	public bool showLighting;

	public bool showTextures;

	public bool showOptions;

	public bool showOutput;

	public int DAY_of_WEEK;

	public int NUMBER_of_DAYS = 7;

	public float TIME_of_DAY = 6f;

	public float TIME_of_DAY_by_CURVE = 6f;

	public int UTC;

	public float Longitude;

	public float DAY_CYCLE = 3f;

	public float PassTime;

	private Vector3 sun_v3 = Vector3.zero;

	public bool SetTime_By_Curve;

	public AnimationCurve DayNightLengthCurve = AnimationCurve.Linear(0f, 0f, 24f, 24f);

	public GameObject Sun_DirectionalLight;

	public GameObject Moon_DirectionalLight;

	public Material Sky_Material;

	public Material Fog_Material;

	public Material Moon_Material;

	public ReflectionProbe AzureReflectionProbe;

	public float RayCoeff = 1.5f;

	public float MieCoeff = 1f;

	public float Turbidity = 1f;

	public float g = 0.75f;

	public float SkyCoeff = 2000f;

	public float SunIntensity = 100f;

	public float MoonIntensity = 0.25f;

	public float Kr = 8.4f;

	public float Km = 1.25f;

	public float Altitude = 0.05f;

	public List<AnimationCurve> LambdaCurveR = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f),
		AnimationCurve.Linear(0f, 650f, 24f, 650f)
	};

	public List<AnimationCurve> LambdaCurveG = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f),
		AnimationCurve.Linear(0f, 570f, 24f, 570f)
	};

	public List<AnimationCurve> LambdaCurveB = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f),
		AnimationCurve.Linear(0f, 475f, 24f, 475f)
	};

	public List<AnimationCurve> RayCoeffCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> MieCoeffCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> TurbidityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> gCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f),
		AnimationCurve.Linear(0f, 0.75f, 24f, 0.75f)
	};

	public List<AnimationCurve> SkyCoeffCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f),
		AnimationCurve.Linear(0f, 2000f, 24f, 2000f)
	};

	public List<AnimationCurve> SunIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f),
		AnimationCurve.Linear(0f, 100f, 24f, 100f)
	};

	public List<AnimationCurve> MoonIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f)
	};

	public List<AnimationCurve> KrCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f),
		AnimationCurve.Linear(0f, 8.4f, 24f, 8.4f)
	};

	public List<AnimationCurve> KmCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f),
		AnimationCurve.Linear(0f, 1.25f, 24f, 1.25f)
	};

	public List<AnimationCurve> AltitudeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f)
	};

	public Vector3 lambda = new Vector3(650f, 570f, 475f);

	private Vector3 K = new Vector3(686f, 678f, 666f);

	private const float n = 1.0003f;

	private const float N = 2.545E+25f;

	private const float pn = 0.035f;

	private const float pi = (float)Math.PI;

	public float SkyLuminance = 1f;

	public float SkyDarkness = 1f;

	public float SunsetPower = 3.5f;

	public float SunDiskSize = 250f;

	public float SunDiskIntensity = 3f;

	public float SunDiskPropagation = -1.5f;

	public float MoonSize = 5f;

	public float StarsIntensity = 5f;

	public float StarsExtinction = 0.5f;

	public float MoonColorPower = 2.15f;

	public float MoonExtinction = 0.5f;

	public float MilkyWayIntensity;

	public float MilkyWayPower = 2.5f;

	public float Exposure = 1.5f;

	public float NightSkyFarColorDistance = 0.5f;

	public float NightSkyFarColorPower = 0.25f;

	public List<AnimationCurve> SkyLuminanceCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> SkyDarknessCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> SunsetPowerCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f),
		AnimationCurve.Linear(0f, 3.5f, 24f, 3.5f)
	};

	public List<AnimationCurve> SunDiskSizeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f),
		AnimationCurve.Linear(0f, 250f, 24f, 250f)
	};

	public List<AnimationCurve> SunDiskIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f)
	};

	public List<AnimationCurve> SunDiskPropagationCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f),
		AnimationCurve.Linear(0f, -1.5f, 24f, -1.5f)
	};

	public List<AnimationCurve> MoonSizeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f)
	};

	public List<AnimationCurve> MoonColorPowerCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f),
		AnimationCurve.Linear(0f, 2.15f, 24f, 2.15f)
	};

	public List<AnimationCurve> StarsIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f)
	};

	public List<AnimationCurve> StarsExtinctionCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f)
	};

	public List<AnimationCurve> MoonExtinctionCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f)
	};

	public List<AnimationCurve> MilkyWayIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f)
	};

	public List<AnimationCurve> MilkyWayPowerCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f),
		AnimationCurve.Linear(0f, 2.5f, 24f, 2.5f)
	};

	public List<AnimationCurve> ExposureCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f),
		AnimationCurve.Linear(0f, 1.5f, 24f, 1.5f)
	};

	public List<AnimationCurve> NightSkyFarColorDistanceCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f),
		AnimationCurve.Linear(0f, 0.5f, 24f, 0.5f)
	};

	public List<AnimationCurve> NightSkyFarColorPowerCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f),
		AnimationCurve.Linear(0f, 0.25f, 24f, 0.25f)
	};

	public List<Gradient> SunsetGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> MoonGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> MoonBrightGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> NightGroundCloseGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> NightGroundFarGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public int MoonEclipseShadow;

	public float Umbra = 0.95f;

	public float UmbraSize = 0.25f;

	public float Penumbra = 3f;

	public float PenumbraSize = 0.5f;

	public Color PenumbraColor = Color.red;

	public float StarsScintillation = 5.5f;

	private float scintRot;

	public Vector3 MilkyWayPos;

	private Matrix4x4 MilkyWayMatrix;

	private Matrix4x4 NoiseRot;

	public bool LinearFog = true;

	public float ScatteringFogDistance = 3f;

	public float FogBlendPoint = 3f;

	public float NormalFogDistance = 10f;

	public float DenseFogIntensity;

	public float DenseFogAltitude = -0.8f;

	public bool UseUnityFog;

	public int UnityFogModeIndex = 1;

	public float UnityFogDensity = 1f;

	public float UnityFogStart;

	public float UnityFogEnd = 300f;

	public List<Gradient> NormalFogGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> GlobalFogGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> DenseFogGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> UnityFogGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<AnimationCurve> ScatteringFogDistanceCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f)
	};

	public List<AnimationCurve> FogBlendPointCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f)
	};

	public List<AnimationCurve> NormalFogDistanceCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f),
		AnimationCurve.Linear(0f, 10f, 24f, 10f)
	};

	public List<AnimationCurve> DenseFogIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f),
		AnimationCurve.Linear(0f, 0.12f, 24f, 0.12f)
	};

	public List<AnimationCurve> DenseFogAltitudeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f),
		AnimationCurve.Linear(0f, -0.8f, 24f, -0.8f)
	};

	public List<AnimationCurve> UnityFogDensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> UnityFogStartCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f)
	};

	public List<AnimationCurve> UnityFogEndCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f),
		AnimationCurve.Linear(0f, 300f, 24f, 300f)
	};

	public int cloudModeIndex;

	private Shader noCloudsShader;

	private Shader preRenderedShader;

	private Shader proceduralCloudShader;

	private float preRenderedCloudLongitude;

	public List<Gradient> EdgeColorGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> DarkColorGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public float CloudExtinction = 3f;

	public float AlphaSaturation = 2f;

	public float CloudDensity = 1f;

	public float MoonBrightIntensity = 3f;

	public float MoonBrightRange = 1f;

	public float PreRenderedCloudAltitude = 0.05f;

	public List<AnimationCurve> PreRenderedCloudAltitudeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f),
		AnimationCurve.Linear(0f, 0.05f, 24f, 0.05f)
	};

	public List<AnimationCurve> CloudExtinctionCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f)
	};

	public List<AnimationCurve> AlphaSaturationCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f),
		AnimationCurve.Linear(0f, 2f, 24f, 2f)
	};

	public List<AnimationCurve> CloudDensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> MoonBrightIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f),
		AnimationCurve.Linear(0f, 3f, 24f, 3f)
	};

	public List<AnimationCurve> MoonBrightRangeCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public Texture2D WispyCloudTexture;

	public List<Gradient> WispyDarknessGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> WispyBrightGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> WispyColorGradientColor = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public float WispyCovarage;

	public float WispyCloudPosition;

	public float WispyCloudSpeed;

	public float WispyCloudDirection;

	public List<AnimationCurve> WispyCovarageCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f),
		AnimationCurve.Linear(0f, 0f, 24f, 0f)
	};

	public List<AnimationCurve> WispyCloudSpeedCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f),
		AnimationCurve.Linear(0f, 5f, 24f, 5f)
	};

	public bool useReflectionProbe = true;

	public int ambientSourceIndex;

	public float AmbientIntensity = 1f;

	public float ReflectionIntensity = 1f;

	public int ReflectionBounces = 1;

	public int ReflectionProbeRefreshMode = 1;

	public int ReflectionProbeTimeSlicing = 2;

	public float ReflectionProbeTimeToUpdate = 1f;

	private float timeSinceLastUpdate;

	public float ReflectionProbeIntensity = 1f;

	public LayerMask ReflectionProbeCullingMask = 0;

	public Vector3 ReflectionProbeSize = new Vector3(10f, 10f, 10f);

	public bool ForceProbeUpdateAtFirstFrame = true;

	public List<AnimationCurve> AmbientIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> ReflectionIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> ReflectionBouncesCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> ReflectionProbeIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<Gradient> AmbientColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> SkyAmbientColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> EquatorAmbientColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> GroundAmbientColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public float SunDirLightIntensity = 1f;

	public float MoonDirLightIntensity = 1f;

	public float SunFlareIntensity = 1f;

	public List<AnimationCurve> SunDirLightIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> MoonDirLightIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<AnimationCurve> SunFlareIntensityCurve = new List<AnimationCurve>
	{
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f),
		AnimationCurve.Linear(0f, 1f, 24f, 1f)
	};

	public List<Gradient> SunDirLightColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public List<Gradient> MoonDirLightColorGradient = new List<Gradient>
	{
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient(),
		new Gradient()
	};

	public RenderTexture MoonTexture;

	public Cubemap StarField;

	public Cubemap StarNoise;

	public Cubemap MilkyWay;

	private float getGradientTime;

	private float getCurveTime;

	public bool skyUpdate = true;

	public bool UseSunLensFlare = true;

	public bool SkyHDR;

	public bool showCurveValue;

	public bool showGradientTime = true;

	public int SpaceColorIndex;

	public Color CurveColorField = Color.yellow;

	public float ColorCorrection = 1f;

	public float WispyColorCorrection = 1f;

	private Light SunLightComponent;

	private Light MoonLightComponent;

	private LensFlare SunFlareComponent;

	public List<AnimationCurve> OutputCurveList = new List<AnimationCurve>();

	public List<Gradient> OutputGradientList = new List<Gradient>();

	private void Start()
	{
		if (UseSunLensFlare)
		{
			SunFlareComponent = Sun_DirectionalLight.GetComponent<LensFlare>();
		}
		SunLightComponent = Sun_DirectionalLight.GetComponent<Light>();
		MoonLightComponent = Moon_DirectionalLight.GetComponent<Light>();
		SkyUpdate();
		SetSunPosition();
		SetTime(TIME_of_DAY, DAY_CYCLE);
		getGradientTime = TIME_of_DAY / 24f;
		getCurveTime = TIME_of_DAY;
		if (SetTime_By_Curve)
		{
			getCurveTime = TIME_of_DAY_by_CURVE;
			getGradientTime = TIME_of_DAY_by_CURVE / 24f;
		}
		if (useReflectionProbe && ReflectionProbeRefreshMode == 2)
		{
			if (ForceProbeUpdateAtFirstFrame)
			{
				AzureReflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame;
				AzureReflectionProbe.RenderProbe(null);
				AzureReflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
			}
			else
			{
				AzureReflectionProbe.RenderProbe(null);
			}
		}
		AzureReflectionProbe.cullingMask = ReflectionProbeCullingMask;
		if (Application.isPlaying)
		{
			ClearList();
		}
	}

	private void Update()
	{
		if (skyUpdate)
		{
			SkyUpdate();
		}
		else
		{
			WispyCloudPosition -= WispyCloudSpeed * (0.001f * Time.deltaTime);
			Sky_Material.SetFloat("_ProceduralCloudSpeed", WispyCloudPosition);
			StarsIntensity = StarsIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			Sky_Material.SetFloat("_StarsIntensity", StarsIntensity);
		}
		Sky_Material.SetVector("_SunDir", -Sun_DirectionalLight.transform.forward);
		Sky_Material.SetVector("_MoonDir", -Moon_DirectionalLight.transform.forward);
		Sky_Material.SetMatrix("_MoonMatrix", Moon_DirectionalLight.transform.worldToLocalMatrix);
		Sky_Material.SetMatrix("_SunMatrix", Sun_DirectionalLight.transform.worldToLocalMatrix);
		Fog_Material.SetVector("_SunDir", -Sun_DirectionalLight.transform.forward);
		Fog_Material.SetVector("_MoonDir", -Moon_DirectionalLight.transform.forward);
		Fog_Material.SetMatrix("_MoonMatrix", Moon_DirectionalLight.transform.worldToLocalMatrix);
		if (cloudModeIndex == 1)
		{
			preRenderedCloudLongitude = 0.0027777778f * (Longitude + 180f);
			Sky_Material.SetFloat("_Longitude", preRenderedCloudLongitude);
			if (TIME_of_DAY >= 12f)
			{
				Sky_Material.SetInt("_Rise_or_Down", 0);
			}
			else
			{
				Sky_Material.SetInt("_Rise_or_Down", 1);
			}
		}
		Moon_Material.SetVector("_SunDir", -Sun_DirectionalLight.transform.forward);
		if (StarsScintillation > 0f)
		{
			scintRot += StarsScintillation * Time.deltaTime;
			Quaternion q = Quaternion.Euler(scintRot, scintRot, scintRot);
			NoiseRot = Matrix4x4.TRS(Vector3.zero, q, new Vector3(1f, 1f, 1f));
			Sky_Material.SetMatrix("_NoiseMatrix", NoiseRot);
		}
		Quaternion q2 = Quaternion.Euler(MilkyWayPos);
		MilkyWayMatrix = Matrix4x4.TRS(Vector3.zero, q2, new Vector3(1f, 1f, 1f));
		Sky_Material.SetMatrix("_MilkyWayMatrix", MilkyWayMatrix);
		sun_v3.x = SetSunPosition();
		sun_v3.y = Longitude;
		if (TIME_of_DAY >= 24f)
		{
			if (DAY_of_WEEK < NUMBER_of_DAYS - 1)
			{
				DAY_of_WEEK++;
			}
			else
			{
				DAY_of_WEEK = 0;
			}
			TIME_of_DAY = 0f;
		}
		if (TIME_of_DAY < 0f)
		{
			if (DAY_of_WEEK > 0)
			{
				DAY_of_WEEK--;
			}
			else
			{
				DAY_of_WEEK = NUMBER_of_DAYS - 1;
			}
			TIME_of_DAY = 24f;
		}
		Sun_DirectionalLight.transform.localEulerAngles = sun_v3;
		if (Application.isPlaying)
		{
			TIME_of_DAY += PassTime * Time.deltaTime;
			if (useReflectionProbe)
			{
				UpdateReflections();
			}
		}
		SunLightIntensity();
		MoonLightIntensity();
		SetAmbient();
		if (UseUnityFog)
		{
			SetUnityFog();
		}
		TIME_of_DAY_by_CURVE = DayNightLengthCurve.Evaluate(TIME_of_DAY);
		getGradientTime = TIME_of_DAY / 24f;
		getCurveTime = TIME_of_DAY;
		if (SetTime_By_Curve)
		{
			getCurveTime = TIME_of_DAY_by_CURVE;
			getGradientTime = TIME_of_DAY_by_CURVE / 24f;
		}
	}

	private Vector3 BetaRay()
	{
		Vector3 vector = lambda * 1E-09f;
		Vector3 result = default(Vector3);
		result.x = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.x, 4f) * 5.755f) * SkyCoeff;
		result.y = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.y, 4f) * 5.755f) * SkyCoeff;
		result.z = 8f * Mathf.Pow((float)Math.PI, 3f) * Mathf.Pow(Mathf.Pow(1.0003f, 2f) - 1f, 2f) * 6.105f / (7.635E+25f * Mathf.Pow(vector.z, 4f) * 5.755f) * SkyCoeff;
		return result;
	}

	private Vector3 BetaMie()
	{
		float num = 0.2f * Turbidity * 10f;
		Vector3 result = default(Vector3);
		result.x = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / lambda.x, 2f) * K.x;
		result.y = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / lambda.y, 2f) * K.y;
		result.z = 434f * num * (float)Math.PI * Mathf.Pow((float)Math.PI * 2f / lambda.z, 2f) * K.z;
		result.x = Mathf.Pow(result.x, -1f);
		result.y = Mathf.Pow(result.y, -1f);
		result.z = Mathf.Pow(result.z, -1f);
		return result;
	}

	private float pi316()
	{
		return 3f / (16f * (float)Math.PI);
	}

	private float pi14()
	{
		return 1f / (4f * (float)Math.PI);
	}

	private Vector3 GetMieG()
	{
		return new Vector3(1f - g * g, 1f + g * g, 2f * g);
	}

	public float SetSunPosition()
	{
		if (SetTime_By_Curve)
		{
			return (TIME_of_DAY_by_CURVE + (float)UTC) * 360f / 24f - 90f;
		}
		return (TIME_of_DAY + (float)UTC) * 360f / 24f - 90f;
	}

	public void SetTime(float hour, float dayDuration)
	{
		TIME_of_DAY = hour;
		DAY_CYCLE = dayDuration;
		if (dayDuration > 0f)
		{
			PassTime = 0.4f / DAY_CYCLE;
		}
		else
		{
			PassTime = 0f;
		}
	}

	public float GetTime()
	{
		float x = Sun_DirectionalLight.transform.localEulerAngles.x;
		return x / 15f;
	}

	private void SunLightIntensity()
	{
		if (!(SunLightComponent != null))
		{
			return;
		}
		SunDirLightIntensity = SunDirLightIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunLightComponent.intensity = SunDirLightIntensity;
		SunLightComponent.color = SunDirLightColorGradient[DAY_of_WEEK].Evaluate(getGradientTime);
		if (SunLightComponent.intensity <= 0f)
		{
			SunLightComponent.enabled = false;
		}
		else
		{
			SunLightComponent.enabled = true;
		}
		if (SunFlareComponent != null)
		{
			if (UseSunLensFlare)
			{
				SunFlareComponent.enabled = true;
				SunFlareIntensity = SunFlareIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
				SunFlareComponent.brightness = SunFlareIntensity;
			}
			else
			{
				SunFlareComponent.enabled = false;
			}
		}
	}

	private void MoonLightIntensity()
	{
		if (MoonLightComponent != null)
		{
			MoonDirLightIntensity = MoonDirLightIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			MoonLightComponent.intensity = MoonDirLightIntensity;
			MoonLightComponent.color = MoonDirLightColorGradient[DAY_of_WEEK].Evaluate(getGradientTime);
			if (MoonLightComponent.intensity <= 0f)
			{
				MoonLightComponent.enabled = false;
			}
			else
			{
				MoonLightComponent.enabled = true;
			}
		}
	}

	private void SetAmbient()
	{
		switch (ambientSourceIndex)
		{
		case 0:
			AmbientIntensity = AmbientIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
			RenderSettings.ambientIntensity = AmbientIntensity;
			break;
		case 1:
			AmbientIntensity = AmbientIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
			RenderSettings.ambientIntensity = AmbientIntensity;
			RenderSettings.ambientSkyColor = SkyAmbientColorGradient[DAY_of_WEEK].Evaluate(TIME_of_DAY / 24f);
			RenderSettings.ambientEquatorColor = EquatorAmbientColorGradient[DAY_of_WEEK].Evaluate(TIME_of_DAY / 24f);
			RenderSettings.ambientGroundColor = GroundAmbientColorGradient[DAY_of_WEEK].Evaluate(TIME_of_DAY / 24f);
			break;
		case 2:
			AmbientIntensity = AmbientIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
			RenderSettings.ambientIntensity = AmbientIntensity;
			RenderSettings.ambientSkyColor = AmbientColorGradient[DAY_of_WEEK].Evaluate(TIME_of_DAY / 24f);
			break;
		}
		ReflectionIntensity = ReflectionIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
		RenderSettings.reflectionIntensity = ReflectionIntensity;
		ReflectionBounces = (int)ReflectionBouncesCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
		RenderSettings.reflectionBounces = ReflectionBounces;
	}

	private void SetUnityFog()
	{
		RenderSettings.fogColor = UnityFogGradientColor[DAY_of_WEEK].Evaluate(getGradientTime);
		UnityFogDensity = UnityFogDensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		UnityFogStart = UnityFogStartCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		UnityFogEnd = UnityFogEndCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		RenderSettings.fogDensity = UnityFogDensity * 0.0001f;
		RenderSettings.fogStartDistance = UnityFogStart;
		RenderSettings.fogEndDistance = UnityFogEnd;
	}

	private void UpdateReflections()
	{
		timeSinceLastUpdate += Time.deltaTime;
		if (ReflectionProbeRefreshMode == 2 && timeSinceLastUpdate >= ReflectionProbeTimeToUpdate)
		{
			AzureReflectionProbe.RenderProbe(null);
			timeSinceLastUpdate = 0f;
		}
		ReflectionProbeIntensity = ReflectionProbeIntensityCurve[DAY_of_WEEK].Evaluate(TIME_of_DAY);
		AzureReflectionProbe.intensity = ReflectionProbeIntensity;
	}

	public void ChangeShader(int shader)
	{
		AzureSkyCloudAnimation component = GetComponent<AzureSkyCloudAnimation>();
		switch (shader)
		{
		case 0:
			noCloudsShader = Shader.Find("azure[Sky]/azure[Sky]_NoClouds");
			if ((bool)component)
			{
				component.enabled = false;
			}
			if (Sky_Material.shader != noCloudsShader)
			{
				Sky_Material.shader = noCloudsShader;
			}
			break;
		case 1:
			preRenderedShader = Shader.Find("azure[Sky]/azure[Sky]_PreRenderedClouds");
			if ((bool)component)
			{
				component.enabled = true;
			}
			if (Sky_Material.shader != preRenderedShader)
			{
				Sky_Material.shader = preRenderedShader;
			}
			break;
		case 2:
			proceduralCloudShader = Shader.Find("azure[Sky]/azure[Sky]_ProceduralClouds");
			if ((bool)component)
			{
				component.enabled = false;
			}
			if (Sky_Material.shader != proceduralCloudShader)
			{
				Sky_Material.shader = proceduralCloudShader;
			}
			break;
		}
	}

	public float AzureSkyGetCurveOutput(int index)
	{
		if (SetTime_By_Curve)
		{
			return OutputCurveList[index].Evaluate(TIME_of_DAY_by_CURVE);
		}
		return OutputCurveList[index].Evaluate(TIME_of_DAY);
	}

	public Color AzureSkyGetGradientOutput(int index)
	{
		float time = ((!SetTime_By_Curve) ? (TIME_of_DAY / 24f) : (TIME_of_DAY_by_CURVE / 24f));
		return OutputGradientList[index].Evaluate(time);
	}

	private void SkyUpdate()
	{
		RayCoeff = RayCoeffCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MieCoeff = MieCoeffCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Turbidity = TurbidityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		g = gCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		lambda.x = LambdaCurveR[DAY_of_WEEK].Evaluate(getCurveTime);
		lambda.y = LambdaCurveG[DAY_of_WEEK].Evaluate(getCurveTime);
		lambda.z = LambdaCurveB[DAY_of_WEEK].Evaluate(getCurveTime);
		SkyCoeff = SkyCoeffCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunIntensity = SunIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MoonIntensity = MoonIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Kr = KrCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Km = KmCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Altitude = AltitudeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SkyLuminance = SkyLuminanceCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SkyDarkness = SkyDarknessCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunsetPower = SunsetPowerCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunDiskSize = SunDiskSizeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunDiskIntensity = SunDiskIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		SunDiskPropagation = SunDiskPropagationCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MoonSize = MoonSizeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		StarsIntensity = StarsIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		StarsExtinction = StarsExtinctionCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MoonExtinction = MoonExtinctionCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MilkyWayIntensity = MilkyWayIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MilkyWayPower = MilkyWayPowerCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Exposure = ExposureCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		MoonColorPower = MoonColorPowerCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		NightSkyFarColorDistance = NightSkyFarColorDistanceCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		NightSkyFarColorPower = NightSkyFarColorPowerCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		ScatteringFogDistance = ScatteringFogDistanceCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		FogBlendPoint = FogBlendPointCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		NormalFogDistance = NormalFogDistanceCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		DenseFogIntensity = DenseFogIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		DenseFogAltitude = DenseFogAltitudeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
		Sky_Material.SetVector("_Br", BetaRay() * RayCoeff);
		Sky_Material.SetVector("_Br2", BetaRay() * 3f);
		Sky_Material.SetVector("_Bm", BetaMie() * MieCoeff);
		Sky_Material.SetVector("_Brm", BetaRay() * RayCoeff + BetaMie() * MieCoeff);
		Sky_Material.SetVector("_mieG", GetMieG());
		Sky_Material.SetFloat("_SunIntensity", SunIntensity);
		Sky_Material.SetFloat("_MoonIntensity", MoonIntensity);
		Sky_Material.SetFloat("_Kr", Kr);
		Sky_Material.SetFloat("_Km", Km);
		Sky_Material.SetFloat("_Altitude", Altitude);
		Sky_Material.SetFloat("_pi316", pi316());
		Sky_Material.SetFloat("_pi14", pi14());
		Sky_Material.SetFloat("_pi", (float)Math.PI);
		Sky_Material.SetFloat("_Exposure", Exposure);
		Sky_Material.SetFloat("_SkyLuminance", SkyLuminance);
		Sky_Material.SetFloat("_SkyDarkness", SkyDarkness);
		Sky_Material.SetFloat("_SunsetPower", SunsetPower);
		Sky_Material.SetFloat("_SunDiskSize", SunDiskSize);
		Sky_Material.SetFloat("_SunDiskIntensity", SunDiskIntensity);
		Sky_Material.SetFloat("_SunDiskPropagation", SunDiskPropagation);
		Sky_Material.SetFloat("_MoonSize", MoonSize);
		Sky_Material.SetFloat("_StarsIntensity", StarsIntensity);
		Sky_Material.SetFloat("_StarsExtinction", StarsExtinction);
		Sky_Material.SetFloat("_MoonExtinction", MoonExtinction);
		Sky_Material.SetFloat("_MilkyWayIntensity", MilkyWayIntensity);
		Sky_Material.SetFloat("_MilkyWayPower", MilkyWayPower);
		Sky_Material.SetFloat("_MoonEclipseShadow", MoonEclipseShadow);
		Sky_Material.SetColor("_SunsetColor", SunsetGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Sky_Material.SetColor("_MoonBrightColor", MoonBrightGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Sky_Material.SetColor("_GroundCloseColor", NightGroundCloseGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Sky_Material.SetColor("_GroundFarColor", NightGroundFarGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Sky_Material.SetFloat("_FarColorDistance", NightSkyFarColorDistance);
		Sky_Material.SetFloat("_FarColorIntensity", NightSkyFarColorPower);
		Sky_Material.SetTexture("_MoonSampler", MoonTexture);
		Sky_Material.SetInt("_MoonEclipseShadow", MoonEclipseShadow);
		Sky_Material.SetFloat("_Umbra", Umbra);
		Sky_Material.SetFloat("_UmbraSize", UmbraSize);
		Sky_Material.SetFloat("_Penumbra", Penumbra);
		Sky_Material.SetFloat("_PenumbraSize", PenumbraSize);
		Sky_Material.SetColor("_PenumbraColor", PenumbraColor);
		Sky_Material.SetTexture("_StarField", StarField);
		Sky_Material.SetTexture("_StarNoise", StarNoise);
		Sky_Material.SetTexture("_MilkyWay", MilkyWay);
		Sky_Material.SetFloat("_ColorCorrection", ColorCorrection);
		Fog_Material.SetVector("_Br", BetaRay() * RayCoeff);
		Fog_Material.SetVector("_Br2", BetaRay() * 3f);
		Fog_Material.SetVector("_Bm", BetaMie() * MieCoeff);
		Fog_Material.SetVector("_Brm", BetaRay() * RayCoeff + BetaMie() * MieCoeff);
		Fog_Material.SetVector("_mieG", GetMieG());
		Fog_Material.SetFloat("_SunIntensity", SunIntensity);
		Fog_Material.SetFloat("_MoonIntensity", MoonIntensity);
		Fog_Material.SetFloat("_Kr", Kr);
		Fog_Material.SetFloat("_Km", Km);
		Fog_Material.SetFloat("_Altitude", Altitude);
		Fog_Material.SetFloat("_pi316", pi316());
		Fog_Material.SetFloat("_pi14", pi14());
		Fog_Material.SetFloat("_pi", (float)Math.PI);
		Fog_Material.SetFloat("_Exposure", Exposure);
		Fog_Material.SetFloat("_SkyLuminance", SkyLuminance);
		Fog_Material.SetFloat("_SkyDarkness", SkyDarkness);
		Fog_Material.SetFloat("_SunsetPower", SunsetPower);
		Fog_Material.SetFloat("_ColorCorrection", ColorCorrection);
		Fog_Material.SetColor("_SunsetColor", SunsetGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetColor("_MoonBrightColor", MoonBrightGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetColor("_GroundCloseColor", NightGroundCloseGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetColor("_GroundFarColor", NightGroundFarGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetFloat("_FarColorDistance", NightSkyFarColorDistance);
		Fog_Material.SetFloat("_FarColorIntensity", NightSkyFarColorPower);
		Fog_Material.SetFloat("_ScatteringFogDistance", ScatteringFogDistance);
		Fog_Material.SetFloat("_BlendFogDistance", FogBlendPoint);
		Fog_Material.SetFloat("_NormalFogDistance", NormalFogDistance);
		Fog_Material.SetFloat("_DenseFogIntensity", DenseFogIntensity);
		Fog_Material.SetFloat("_DenseFogAltitude", DenseFogAltitude);
		Fog_Material.SetColor("_DenseFogColor", DenseFogGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetColor("_NormalFogColor", NormalFogGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Fog_Material.SetColor("_GlobalColor", GlobalFogGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		if (LinearFog)
		{
			Fog_Material.SetFloat("_LinearFog", 0.45f);
		}
		else
		{
			Fog_Material.SetFloat("_LinearFog", 1f);
		}
		switch (cloudModeIndex)
		{
		case 1:
			Sky_Material.SetColor("_EdgeColor", EdgeColorGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
			Sky_Material.SetColor("_DarkColor", DarkColorGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
			CloudExtinction = CloudExtinctionCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			AlphaSaturation = AlphaSaturationCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			CloudDensity = CloudDensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			MoonBrightIntensity = MoonBrightIntensityCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			MoonBrightRange = MoonBrightRangeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			PreRenderedCloudAltitude = PreRenderedCloudAltitudeCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			Sky_Material.SetFloat("_CloudExtinction", CloudExtinction);
			Sky_Material.SetFloat("_AlphaSaturation", AlphaSaturation);
			Sky_Material.SetFloat("_CloudDensity", CloudDensity);
			Sky_Material.SetFloat("_MoonBrightIntensity", MoonBrightIntensity);
			Sky_Material.SetFloat("_MoonBrightRange", MoonBrightRange);
			Sky_Material.SetFloat("_CloudAltitude", PreRenderedCloudAltitude);
			break;
		case 2:
			Sky_Material.SetColor("_WispyDarkness", WispyDarknessGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
			Sky_Material.SetColor("_WispyBright", WispyBrightGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
			Sky_Material.SetColor("_WispyColor", WispyColorGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
			WispyCovarage = WispyCovarageCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			WispyCloudSpeed = WispyCloudSpeedCurve[DAY_of_WEEK].Evaluate(getCurveTime);
			Sky_Material.SetFloat("_WispyCovarage", WispyCovarage);
			Sky_Material.SetTexture("_WispyCloudTexture", WispyCloudTexture);
			WispyCloudPosition -= WispyCloudSpeed * (0.001f * Time.deltaTime);
			Sky_Material.SetFloat("_ProceduralCloudSpeed", WispyCloudPosition);
			Sky_Material.SetFloat("_WispyCloudDirection", WispyCloudDirection);
			Sky_Material.SetFloat("_WispyColorCorrection", WispyColorCorrection);
			break;
		}
		Moon_Material.SetColor("_MoonColor", MoonGradientColor[DAY_of_WEEK].Evaluate(getGradientTime));
		Moon_Material.SetFloat("_LightIntensity", MoonColorPower);
		if (SkyHDR)
		{
			Sky_Material.DisableKeyword("HDR_OFF");
			Sky_Material.EnableKeyword("HDR_ON");
			Fog_Material.DisableKeyword("HDR_OFF");
			Fog_Material.EnableKeyword("HDR_ON");
		}
		else
		{
			Sky_Material.EnableKeyword("HDR_OFF");
			Sky_Material.DisableKeyword("HDR_ON");
			Fog_Material.EnableKeyword("HDR_OFF");
			Fog_Material.DisableKeyword("HDR_ON");
		}
	}

	private void ClearList()
	{
		for (int i = NUMBER_of_DAYS; i < 7; i++)
		{
			LambdaCurveR.RemoveAt(NUMBER_of_DAYS);
			LambdaCurveG.RemoveAt(NUMBER_of_DAYS);
			LambdaCurveB.RemoveAt(NUMBER_of_DAYS);
			RayCoeffCurve.RemoveAt(NUMBER_of_DAYS);
			MieCoeffCurve.RemoveAt(NUMBER_of_DAYS);
			TurbidityCurve.RemoveAt(NUMBER_of_DAYS);
			gCurve.RemoveAt(NUMBER_of_DAYS);
			SkyCoeffCurve.RemoveAt(NUMBER_of_DAYS);
			SunIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			MoonIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			KrCurve.RemoveAt(NUMBER_of_DAYS);
			KmCurve.RemoveAt(NUMBER_of_DAYS);
			AltitudeCurve.RemoveAt(NUMBER_of_DAYS);
			SkyLuminanceCurve.RemoveAt(NUMBER_of_DAYS);
			SkyDarknessCurve.RemoveAt(NUMBER_of_DAYS);
			SunsetPowerCurve.RemoveAt(NUMBER_of_DAYS);
			SunDiskSizeCurve.RemoveAt(NUMBER_of_DAYS);
			SunDiskIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			SunDiskPropagationCurve.RemoveAt(NUMBER_of_DAYS);
			MoonSizeCurve.RemoveAt(NUMBER_of_DAYS);
			MoonColorPowerCurve.RemoveAt(NUMBER_of_DAYS);
			StarsIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			StarsExtinctionCurve.RemoveAt(NUMBER_of_DAYS);
			MilkyWayIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			MilkyWayPowerCurve.RemoveAt(NUMBER_of_DAYS);
			ExposureCurve.RemoveAt(NUMBER_of_DAYS);
			NightSkyFarColorDistanceCurve.RemoveAt(NUMBER_of_DAYS);
			NightSkyFarColorPowerCurve.RemoveAt(NUMBER_of_DAYS);
			SunsetGradientColor.RemoveAt(NUMBER_of_DAYS);
			MoonGradientColor.RemoveAt(NUMBER_of_DAYS);
			MoonBrightGradientColor.RemoveAt(NUMBER_of_DAYS);
			NightGroundCloseGradientColor.RemoveAt(NUMBER_of_DAYS);
			NightGroundFarGradientColor.RemoveAt(NUMBER_of_DAYS);
			NormalFogGradientColor.RemoveAt(NUMBER_of_DAYS);
			GlobalFogGradientColor.RemoveAt(NUMBER_of_DAYS);
			ScatteringFogDistanceCurve.RemoveAt(NUMBER_of_DAYS);
			FogBlendPointCurve.RemoveAt(NUMBER_of_DAYS);
			NormalFogDistanceCurve.RemoveAt(NUMBER_of_DAYS);
			EdgeColorGradientColor.RemoveAt(NUMBER_of_DAYS);
			DarkColorGradientColor.RemoveAt(NUMBER_of_DAYS);
			PreRenderedCloudAltitudeCurve.RemoveAt(NUMBER_of_DAYS);
			CloudExtinctionCurve.RemoveAt(NUMBER_of_DAYS);
			AlphaSaturationCurve.RemoveAt(NUMBER_of_DAYS);
			CloudDensityCurve.RemoveAt(NUMBER_of_DAYS);
			MoonBrightIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			MoonBrightRangeCurve.RemoveAt(NUMBER_of_DAYS);
			WispyCovarageCurve.RemoveAt(NUMBER_of_DAYS);
			WispyCloudSpeedCurve.RemoveAt(NUMBER_of_DAYS);
			WispyColorGradientColor.RemoveAt(NUMBER_of_DAYS);
			WispyBrightGradientColor.RemoveAt(NUMBER_of_DAYS);
			WispyDarknessGradientColor.RemoveAt(NUMBER_of_DAYS);
			AmbientIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			ReflectionIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			ReflectionBouncesCurve.RemoveAt(NUMBER_of_DAYS);
			ReflectionProbeIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			AmbientColorGradient.RemoveAt(NUMBER_of_DAYS);
			SkyAmbientColorGradient.RemoveAt(NUMBER_of_DAYS);
			EquatorAmbientColorGradient.RemoveAt(NUMBER_of_DAYS);
			GroundAmbientColorGradient.RemoveAt(NUMBER_of_DAYS);
			SunDirLightIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			MoonDirLightIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			SunFlareIntensityCurve.RemoveAt(NUMBER_of_DAYS);
			SunDirLightColorGradient.RemoveAt(NUMBER_of_DAYS);
			MoonDirLightColorGradient.RemoveAt(NUMBER_of_DAYS);
		}
	}

	private void OnEnable()
	{
	}
}
