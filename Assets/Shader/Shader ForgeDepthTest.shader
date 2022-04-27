Shader "Shader Forge/DepthTest" {
	Properties {
		_Watercolor ("Water color", Vector) = (0.5019608,0.5019608,0.5019608,1)
		_Watercolor2 ("Water color 2", Vector) = (0.5,0.5,0.5,1)
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Gloss ("Gloss", Range(0, 1)) = 0.8
		_Depth ("Depth", Float) = 0
		_Foamrange ("Foam range", Float) = 0
		_Foampower ("Foam power", Float) = 0
		_Foamcolor ("Foam color", Vector) = (0.5,0.5,0.5,1)
		_Foammask ("Foam mask", 2D) = "white" {}
		_Waterspeed ("Water speed", Range(0, 1)) = 0.1416042
		_Wavenormal ("Wave normal", 2D) = "bump" {}
		_Wavenormal2 ("Wave normal 2", 2D) = "bump" {}
		_Waveheight ("Wave height", Range(0, 10)) = 0
		_Wavefrequency ("Wave frequency", Range(0, 100)) = 0
		_Wavespeed ("Wave speed", Range(0, 1)) = 0
		_Fresnelpower ("Fresnel power", Range(0, 10)) = 0
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}