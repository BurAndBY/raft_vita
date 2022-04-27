using UnityEngine;
using UnityEngine.UI;

namespace PlayWay.WaterSamples
{
	public class CubesSample : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private Text cubesCountLabel;

		[SerializeField]
		private Text fpsLabel;

		private int cubesCount = 1;

		private float nextFpsUpdate;

		private int lastFrameCount;

		private float lastTime;

		private float nextCubeSpawnTime;

		private void Update()
		{
			if (Input.GetKey(KeyCode.Space) && Time.time >= nextCubeSpawnTime)
			{
				GameObject gameObject = Object.Instantiate(prefab);
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				component.AddForce(Random.Range(-15f, 15f), Random.Range(-25f, 15f), Random.Range(-15f, 15f), ForceMode.Impulse);
				component.AddTorque(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
				cubesCount++;
				cubesCountLabel.text = "Cubes: " + cubesCount;
				nextCubeSpawnTime = Time.time + 0.05f;
			}
			if (Input.GetKey(KeyCode.A))
			{
				Camera.main.transform.RotateAround(base.transform.position, Vector3.up, Time.deltaTime * 20f);
			}
			if (Input.GetKey(KeyCode.D))
			{
				Camera.main.transform.RotateAround(base.transform.position, Vector3.up, (0f - Time.deltaTime) * 20f);
			}
			if (Time.time >= nextFpsUpdate)
			{
				int frameCount = Time.frameCount;
				fpsLabel.text = "FPS: " + ((float)(frameCount - lastFrameCount) / (Time.time - lastTime)).ToString("0.0");
				nextFpsUpdate = Time.time + 0.25f;
				lastFrameCount = frameCount;
				lastTime = Time.time;
			}
		}
	}
}
