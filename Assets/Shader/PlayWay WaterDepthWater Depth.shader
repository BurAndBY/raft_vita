Shader "PlayWay Water/Depth/Water Depth" {
	Properties {
		_SubtractiveMask ("", 2D) = "black" {}
		_AdditiveMask ("", 2D) = "black" {}
		_DisplacedHeightMaps ("", 2D) = "black" {}
		_WaterTileSize ("Heightmap Tile Size", Vector) = (180,18,600,1800)
		_SurfaceOffset ("", Vector) = (0,0,0,0)
		_WaterId ("", Vector) = (128,256,0,0)
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