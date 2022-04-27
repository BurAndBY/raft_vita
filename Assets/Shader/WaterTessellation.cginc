#include "Tessellation.cginc"
#include "Lighting.cginc"

float _TesselationFactor;

struct TesselatorVertexInput
{
    float4 vertex		: INTERNALTESSPOS;
	float tessFactor	: TEXCOORD0;
	half3 normal		: NORMAL;
    #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		half2 uv2		: TEXCOORD1;
    #endif
};

float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess)
{
	float dist = distance(vertex, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return f;
}

#if defined(_PROJECTION_GRID) || !defined(_TRIANGLES)

	/*
	 * QUAD TESSELATION
	 */
	[UNITY_domain("quad")]
	[UNITY_partitioning("integer")]
	[UNITY_outputtopology("triangle_cw")]
	[UNITY_patchconstantfunc("hsconst")]
	[UNITY_outputcontrolpoints(4)]
	TesselatorVertexInput hs_surf (InputPatch<TesselatorVertexInput, 4> v, uint id : SV_OutputControlPointID)
	{
		return v[id];
	}

	struct HS_CONSTANT_OUTPUT
	{
		float edge[4]  : SV_TessFactor;
		float inside[2] : SV_InsideTessFactor;
	};

	HS_CONSTANT_OUTPUT hsconst (InputPatch<TesselatorVertexInput,4> v)
	{
		HS_CONSTANT_OUTPUT o;

		float4 tess = float4(v[0].tessFactor, v[1].tessFactor, v[2].tessFactor, v[3].tessFactor);
		o.edge[0] = 0.5 * (tess.x + tess.w);
		o.edge[1] = 0.5 * (tess.x + tess.y);
		o.edge[2] = 0.5 * (tess.y + tess.z);
		o.edge[3] = 0.5 * (tess.z + tess.w);
		o.inside[0] = o.inside[1] = (o.edge[0] + o.edge[1] + o.edge[2] + o.edge[3]) * 0.25;

		return o;
	}

	#ifndef BASIC_INPUTS
	[UNITY_domain("quad")]
	TESS_OUTPUT ds_surf (HS_CONSTANT_OUTPUT tessFactors, const OutputPatch<TesselatorVertexInput, 4> patch, float2 UV : SV_DomainLocation)
	{
		VertexInput v;

		v.vertex = lerp(
			lerp(patch[0].vertex, patch[1].vertex, UV.x),
			lerp(patch[3].vertex, patch[2].vertex, UV.x),
			UV.y
		);

		v.normal = lerp(
			lerp(patch[0].normal, patch[1].normal, UV.x),
			lerp(patch[3].normal, patch[2].normal, UV.x),
			UV.y
		);

	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		v.uv2 = lerp(
			lerp(patch[0].uv2, patch[1].uv2, UV.x),
			lerp(patch[3].uv2, patch[2].uv2, UV.x),
			UV.y
		);
	#endif

		TESS_OUTPUT o = POST_TESS_VERT (v);
		return o;
	}
	#else
	[UNITY_domain("quad")]
	TESS_OUTPUT ds_surf(HS_CONSTANT_OUTPUT tessFactors, const OutputPatch<TesselatorVertexInput, 4> patch, float2 UV : SV_DomainLocation)
	{
		VertexInput2 v;

		v.vertex = lerp(
			lerp(patch[0].vertex, patch[1].vertex, UV.x),
			lerp(patch[3].vertex, patch[2].vertex, UV.x),
			UV.y
			);

		TESS_OUTPUT o = POST_TESS_VERT(v);
		return o;
	}
	#endif

#else

	/*
	* TRIANGLE TESSELATION
	*/
	[UNITY_domain("tri")]
	[UNITY_partitioning("fractional_odd")]
	[UNITY_outputtopology("triangle_cw")]
	[UNITY_patchconstantfunc("hsconst")]
	[UNITY_outputcontrolpoints(3)]
	TesselatorVertexInput hs_surf(InputPatch<TesselatorVertexInput, 3> v, uint id : SV_OutputControlPointID)
	{
		return v[id];
	}

	UnityTessellationFactors hsconst(InputPatch<TesselatorVertexInput, 3> v)
	{
		UnityTessellationFactors o;
		float4 tf = UnityCalcTriEdgeTessFactors(float3(v[0].tessFactor, v[1].tessFactor, v[2].tessFactor));
		o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
		return o;
	}

	#ifndef BASIC_INPUTS
	[UNITY_domain("tri")]
	TESS_OUTPUT ds_surf(UnityTessellationFactors tessFactors, const OutputPatch<TesselatorVertexInput, 3> vi, float3 bary : SV_DomainLocation) {
		VertexInput v;
		v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
		v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;

	#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
		v.uv2 = vi[0].uv2*bary.x + vi[1].uv2*bary.y + vi[2].uv2*bary.z;
	#endif

		TESS_OUTPUT o = POST_TESS_VERT(v);
		return o;
	}
	#else
	[UNITY_domain("tri")]
	TESS_OUTPUT ds_surf(UnityTessellationFactors tessFactors, const OutputPatch<TesselatorVertexInput, 3> vi, float3 bary : SV_DomainLocation) {
		VertexInput2 v;
		v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;

		TESS_OUTPUT o = POST_TESS_VERT(v);
		return o;
	}
	#endif

#endif

TesselatorVertexInput tessvert_surf (VertexInput v)
{
	TesselatorVertexInput o;
#if _PROJECTION_GRID
	o.vertex = v.vertex;
	o.vertex = float4(GetProjectedPosition(o.vertex.xy), 1);
	o.tessFactor = round(_TesselationFactor);			// use flat tesselation rate as it doesn't have much sense to support tesselation for projection grid
#else
	o.vertex = mul(_OBJECT2WORLD, v.vertex);
	o.tessFactor = CalcDistanceTessFactor(o.vertex, _ProjectionParams.y, _ProjectionParams.z, _TesselationFactor);
#endif
	o.normal = v.normal;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	o.uv2 = v.uv2;
#endif
	return o;
}
