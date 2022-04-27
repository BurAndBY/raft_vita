Shader "Shader Forge/Fatique2" {
	Properties {
		[PerRendererData] _MainTex ("MainTex", 2D) = "white" {}
		_Color1 ("Color1", Vector) = (1,1,1,1)
		_Color2 ("Color2", Vector) = (1,0,0,1)
		_FatigueValue ("FatigueValue", Range(0, 1)) = 0.8402407
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}