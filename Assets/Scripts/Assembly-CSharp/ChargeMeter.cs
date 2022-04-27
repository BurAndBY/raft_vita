using UnityEngine;

public class ChargeMeter : MonoBehaviour
{
	public float chargeSpeed = 100f;

	[Range(0f, 1f)]
	public float minimumChargeRate = 0.3f;

	[Range(1f, 10f)]
	public int difficultyLevel = 2;

	private float maxCharge = 100f;

	private float minCharge;

	private float currentCharge;

	private int chargeDirection = 1;

	public float ChargeNormal
	{
		get
		{
			return Mathf.Clamp(currentCharge / maxCharge, minCharge, maxCharge);
		}
	}

	public void Charge()
	{
		float num = Mathf.Clamp(currentCharge / maxCharge, minimumChargeRate, 1f);
		currentCharge += (float)chargeDirection * chargeSpeed * Time.deltaTime * (float)difficultyLevel;
		currentCharge = Mathf.Clamp(currentCharge, minCharge, maxCharge);
	}

	public void Reset()
	{
		currentCharge = 0f;
		chargeDirection = 1;
	}
}
