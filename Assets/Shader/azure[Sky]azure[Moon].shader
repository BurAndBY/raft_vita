Shader "azure[Sky]/azure[Moon]" {
	Properties {
		_MoonTexture ("Moon Texture", 2D) = "white" {}
		_Saturation ("Saturation", Range(0.5, 2)) = 0.5
		_Penunbra ("Penunbra", Range(0, 4)) = 2
		_Shadow ("Shadow", Range(0, 0.1)) = 0
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