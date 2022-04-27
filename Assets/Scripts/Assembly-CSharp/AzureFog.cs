using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("azure[Sky]/Fog Scattering")]
public class AzureFog : MonoBehaviour
{
	public Material fogMaterial;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		if (fogMaterial == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		Camera component = GetComponent<Camera>();
		float nearClipPlane = component.nearClipPlane;
		float farClipPlane = component.farClipPlane;
		float fieldOfView = component.fieldOfView;
		float aspect = component.aspect;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = fieldOfView * 0.5f;
		Vector3 vector = GetComponent<Camera>().transform.right * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f)) * aspect;
		Vector3 vector2 = GetComponent<Camera>().transform.up * nearClipPlane * Mathf.Tan(num * ((float)Math.PI / 180f));
		Vector3 vector3 = GetComponent<Camera>().transform.forward * nearClipPlane - vector + vector2;
		float num2 = vector3.magnitude * farClipPlane / nearClipPlane;
		vector3.Normalize();
		vector3 *= num2;
		Vector3 vector4 = component.transform.forward * nearClipPlane + vector + vector2;
		vector4.Normalize();
		vector4 *= num2;
		Vector3 vector5 = component.transform.forward * nearClipPlane + vector - vector2;
		vector5.Normalize();
		vector5 *= num2;
		Vector3 vector6 = component.transform.forward * nearClipPlane - vector - vector2;
		vector6.Normalize();
		vector6 *= num2;
		identity.SetRow(0, vector3);
		identity.SetRow(1, vector4);
		identity.SetRow(2, vector5);
		identity.SetRow(3, vector6);
		fogMaterial.SetMatrix("_FrustumCorners", identity);
		CustomGraphicsBlit(source, destination, fogMaterial, 0);
	}

	private static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
	{
		RenderTexture.active = dest;
		fxMaterial.SetTexture("_MainTex", source);
		GL.PushMatrix();
		GL.LoadOrtho();
		fxMaterial.SetPass(passNr);
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.Vertex3(0f, 0f, 3f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.Vertex3(1f, 0f, 2f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.Vertex3(1f, 1f, 1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}
}
