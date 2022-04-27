Shader "Shader Forge/Palmtree" {
	Properties {
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Color ("Color", Vector) = (0.5019608,0.5019608,0.5019608,1)
		_MainTex ("Base Color", 2D) = "white" {}
		_MettalicRough ("Mettalic/Rough", 2D) = "white" {}
		_Waves ("Waves", Range(0, 10)) = 4
		_Speed ("Speed", Range(0, 10)) = 5.717038
		_Windmask ("Windmask", 2D) = "white" {}
		_Windstrenthg ("Windstrenthg", Range(0, 1)) = 0
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}