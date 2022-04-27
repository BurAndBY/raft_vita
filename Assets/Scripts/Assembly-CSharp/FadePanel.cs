using System.Collections;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public IEnumerator FadeToAlpha(float targetAlpha, float speed)
	{
		float diff = Mathf.Abs(canvasGroup.alpha - targetAlpha);
		while (diff > 0.1f)
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * speed);
			diff = Mathf.Abs(canvasGroup.alpha - targetAlpha);
			yield return null;
		}
		canvasGroup.alpha = targetAlpha;
	}

	public void SetAlpha(float alpha)
	{
		canvasGroup.alpha = alpha;
	}
}
