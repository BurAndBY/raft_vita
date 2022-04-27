Shader "PlayWay Water/Variations/Water Volume _ALPHABLEND_ON _CUBEMAP_REFLECTIONS _VOLUMETRIC_LIGHTING _WATER_REFRACTION _WAVES_FFT" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_DepthColor ("Depth Color", Vector) = (0,0.012,0.05,1)
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_SpecColor ("Specular", Vector) = (0.2,0.2,0.2,1)
		_BumpScale ("Bump Scale", Vector) = (1,1,0,0)
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_DisplacementNormalsIntensity ("Displacement Normals Intensity", Float) = 1.4
		_GlobalNormalMap ("", 2D) = "black" {}
		_GlobalNormalMap1 ("", 2D) = "black" {}
		_GlobalNormalMap2 ("", 2D) = "black" {}
		_GlobalNormalMap3 ("", 2D) = "black" {}
		_GlobalDisplacementMap ("", 2D) = "black" {}
		_GlobalDisplacementMap1 ("", 2D) = "black" {}
		_GlobalDisplacementMap2 ("", 2D) = "black" {}
		_GlobalDisplacementMap3 ("", 2D) = "black" {}
		_DisplacementsScale ("Horizontal Displacement Scale", Float) = 1
		_OcclusionStrength ("Strength", Range(0, 1)) = 1
		_OcclusionMap ("Occlusion", 2D) = "white" {}
		_EmissionColor ("Color", Vector) = (0,0,0,1)
		_EmissionMap ("Emission", 2D) = "white" {}
		_SubtractiveMask ("", 2D) = "black" {}
		_AdditiveMask ("", 2D) = "black" {}
		_DetailAlbedoMap ("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale ("Scale", Float) = 1
		_DetailNormalMap ("Normal Map", 2D) = "bump" {}
		_DetailFadeFactor ("", Float) = 2
		_PlanarReflectionTex ("Planar Reflection", 2D) = "black" {}
		_PlanarReflectionPack ("Planar reflection (distortion, intensity, offset Y, unused)", Vector) = (1,0.45,-0.3,0)
		_LightSmoothnessMul ("Light Smoothness Multiplier", Float) = 1
		_WrapSubsurfaceScatteringPack ("Wrap SSS", Vector) = (0.2,0.833333,0.5,0.66666)
		_SpecularFresnelBias ("Specular Fresnel Bias", Float) = 0.02041
		_RefractionDistortion ("Refraction Distortion", Float) = 0.55
		_WaterTileSize ("Heightmap Tile Size", Vector) = (180,18,600,1800)
		_WaterTileSizeScales ("", Vector) = (0.63241,0.113151,3.165131,10.265131)
		_Foam ("Foam (intensity, cutoff)", Vector) = (0.1,0.375,0,0)
		_FoamTex ("Foam texture ", 2D) = "black" {}
		_FoamNormalMap ("Foam Normal Map", 2D) = "bump" {}
		_FoamNormalScale ("Foam Normal Scale", Float) = 2.2
		_FoamTiling ("Foam Tiling", Vector) = (5.4,5.4,1,1)
		_FoamSpecularColor ("Foam Specular Color", Vector) = (1,1,1,1)
		_EdgeBlendFactorInv ("Edge Blend Factor", Float) = 0.3333
		_FoamMapWS ("", 2D) = "black" {}
		_AbsorptionColor ("", Vector) = (0.35,0.04,0.001,1)
		_ReflectionColor ("", Vector) = (1,1,1,1)
		_LocalDisplacementMap ("", 2D) = "black" {}
		_LocalSlopeMap ("", 2D) = "black" {}
		_DisplacedHeightMaps ("", 2D) = "black" {}
		_SubsurfaceScatteringPack ("Subsurface Scattering", Vector) = (1,0.15,1.65,0)
		_SlopeVariance ("", 3D) = "black" {}
		_TesselationFactor ("Tesselation Factor", Float) = 14
		_MaxDisplacement ("", Float) = 10
		_SurfaceOffset ("", Vector) = (0,0,0,0)
		_WaterId ("", Vector) = (128,256,0,0)
		_WaterStencilId ("", Float) = 0
		_WaterStencilIdInv ("", Float) = 0
		[HideInInspector] _Cull ("_Cull", Float) = 2
		[HideInInspector] _Mode ("__mode", Float) = 0
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 0
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
}