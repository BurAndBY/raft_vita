Shader "PlayWay Water/Volumes/Front Simple" {
	Properties {
		_WaterId ("", Vector) = (2,1,0,0)
		_WaterStencilId ("", Float) = 0
		_WaterStencilIdInv ("", Float) = 0
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