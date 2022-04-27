Shader "PlayWay Water/Spectrum/Water Spectrum" {
	Properties {
		_MainTex ("", 2D) = "" {}
		_TileSizeLookup ("", 2D) = "" {}
		_Gravity ("", Float) = 9.81
		_PlaneSizeInv ("", Float) = 0.01
		_TargetResolution ("", Vector) = (256,256,256,256)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
}