Shader "azure[Sky]/azure[Sky]_ProceduralClouds" {
	Properties {
		[HideInInspector] _WispyCovarage ("Covarage", Range(0, 5)) = 1
		[HideInInspector] _WispyDarkness ("Darkness", Vector) = (1,1,1,1)
		[HideInInspector] _WispyBright ("Bright", Vector) = (1,1,1,1)
		[HideInInspector] _WispyColor ("Wispy Color", Vector) = (1,1,1,1)
		_ProceduralCloudAltitude ("Cloud Altitude", Range(10, 100)) = 50
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
}