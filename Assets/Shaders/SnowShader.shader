Shader "Custom/SnowShader" {
	Properties {
		_TesselationMinDistance("MinTess Distance", Float) = 0
		_TesselationMaxDistance("Max Tess Distance", Float) = 50
		_Tess("Tessellation", Range(1,512)) = 4
		_Splatmap("Splatmap", 2D) = "black" {}
		_Displacement("Displacement", Range(0, 1.0)) = 0.3
		_SnowColor ("Snow Color", Color) = (1,1,1,1)
		_SnowTex ("Snow (RGB)", 2D) = "white" {}
		_GroundColor("Ground Color", Color) = (1,1,1,1)
		_GroundTex("Ground (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_TopCamData ("Top Cam Debug", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6


		#include "Tessellation.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
		};

		float _Tess, _TesselationMinDistance, _TesselationMaxDistance;

		float4 tessDistance(appdata v0, appdata v1, appdata v2) {
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _TesselationMinDistance, _TesselationMaxDistance, _Tess);
		}

		sampler2D _Splatmap;
		float _Displacement;
		float4 _TopCamData;
		
		float4 GetTopCamMapLod(float2 wPos, float4 topCam, sampler2D  camTex)
	    {
	        return tex2Dlod(camTex, (float4(wPos,0,0) + float4(-topCam.x,-topCam.z,0,0)) * topCam.w * .5 + float4(.5,.5,0,0));
	    }
		
		void disp(inout appdata v)
		{
			float4 pos = mul(unity_ObjectToWorld,v.vertex);
			half camDistance = distance(_TopCamData.xz, pos.xz);
			camDistance = smoothstep(25, 20, camDistance);
			float4 d = GetTopCamMapLod(pos.xz, _TopCamData, _Splatmap).r * _Displacement * camDistance;
			v.vertex.xyz -= v.normal * d;
			v.vertex.xyz += v.normal * _Displacement;
		}



		sampler2D _SnowTex;
		fixed4 _SnowColor;
		sampler2D _GroundTex;
		fixed4 _GroundColor;

		struct Input {
			float2 uv_SnowTex;
			float2 uv_GroundTex;
			float2 uv_Splatmap;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float3 viewDirection = normalize( o. );
			
			half camDistance = distance(_TopCamData.xz, IN.worldPos.xz);
			camDistance = smoothstep(25, 20, camDistance);
			
			half amount = GetTopCamMapLod(IN.worldPos.xz, _TopCamData, _Splatmap).r * camDistance;
			// Albedo comes from a texture tinted by color
			fixed4 snow = tex2D(_SnowTex, IN.uv_SnowTex) * _SnowColor;
			fixed4 ground = tex2D(_GroundTex, IN.uv_GroundTex) * _GroundColor;

			fixed4 c = lerp(snow, ground, amount);
			
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
