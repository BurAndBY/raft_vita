using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.WaterSamples
{
	public class FpsCounter : MonoBehaviour
	{
		private Text label;

		private int frameCount;

		private float timeSum;

		private void Awake()
		{
			label = GetComponent<Text>();
		}

		private void Update()
		{
			frameCount++;
			timeSum += Time.unscaledDeltaTime;
			if (frameCount > 10)
			{
				label.text = ((float)frameCount / timeSum).ToString("0.0");
				frameCount = 0;
				timeSum = 0f;
			}
		}
	}
}
