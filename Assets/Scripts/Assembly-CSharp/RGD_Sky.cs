using System;
using System.Runtime.Serialization;

[Serializable]
public class RGD_Sky
{
	[OptionalField(VersionAdded = 3)]
	public float timeOfDay;

	public RGD_Sky(AzureSky_Controller skyController)
	{
		timeOfDay = skyController.TIME_of_DAY;
	}

	public void RestoreSky(AzureSky_Controller skyController)
	{
		skyController.SetTime(timeOfDay, skyController.DAY_CYCLE);
	}
}
