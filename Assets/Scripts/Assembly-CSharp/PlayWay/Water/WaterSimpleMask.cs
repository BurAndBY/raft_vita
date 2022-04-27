using System;
using UnityEngine;

namespace PlayWay.Water
{
	[RequireComponent(typeof(Renderer))]
	public class WaterSimpleMask : MonoBehaviour
	{
		[SerializeField]
		private Water water;

		public Water Water
		{
			get
			{
				return water;
			}
			set
			{
				if (!(water == value))
				{
					base.enabled = false;
					water = value;
					base.enabled = true;
				}
			}
		}

		private void OnEnable()
		{
			Renderer component = GetComponent<Renderer>();
			component.material.SetFloat("_WaterId", 1 << water.WaterId);
			base.gameObject.layer = WaterProjectSettings.Instance.WaterTempLayer;
			if (component == null)
			{
				throw new InvalidOperationException("WaterSimpleMask is attached to an object without any renderer.");
			}
			water.Renderer.AddMask(component);
		}

		private void OnDisable()
		{
			Renderer component = GetComponent<Renderer>();
			if (component == null)
			{
				throw new InvalidOperationException("WaterSimpleMask is attached to an object without any renderer.");
			}
			water.Renderer.RemoveMask(component);
		}

		private void OnValidate()
		{
			base.gameObject.layer = WaterProjectSettings.Instance.WaterTempLayer;
		}
	}
}
