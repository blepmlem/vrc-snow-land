Shader "Custom/SnowShader" {
	Properties {
		_TesselationMinDistance("MinTess Distance", Float) = 0
		_TesselationMaxDistance("Max Tess Distance", Float) = 50
		_Tess("Tessellation", Range(1,128)) = 4
		_SnowTex ("Snow (RGB)", 2D) = "white" {}
		_Splatmap("Splatmap", 2D) = "black" {}
		_Displacement("Displacement", Range(0, 1.0)) = 0.3
		[HDR] _TopColor ("Top Color", Color) = (1,1,1,1)
		_TopTex ("Top (RGB)", 2D) = "white" {}
		[HDR] _BotColor("Bottom Color", Color) = (1,1,1,1)
		_BotTex("Bottom (RGB)", 2D) = "white" {}
//		_Glossiness ("Smoothness", Range(0,1)) = 0.5
//		_Metallic ("Metallic", Range(0,1)) = 0
//		_Noise1 ("Noise 1 (RGB)", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range(0, 1.0)) = 0.3
		_GlitterTex("GlitterTex", 2D) = "white" {}
		_GlitterThreshold("GlitterThreshold", Float) = 0
		[HDR] _GlitterColor("GlitterColor", Color) = (1,1,1,1)
		_OceanSpecularPower("OceanSpecularPower", Float) = 0
		_OceanSpecularStrength("OceanSpecularStrength", Float) = 0
		[HDR] _OceanSpecularColor("OceanSpecularColor", Color) = (1,1,1,1)
		_TerrainRimPower("TerrainRimPower", Float) = 0
		_TerrainRimStrength("TerrainRimStrength", Float) = 0
		[HDR] _TerrainRimColor("TerrainRimColor", Color) = (1,1,1,1)
		[HDR] _TerrainColor("TerrainColor", Color) = (1,1,1,1)
		_ShadowColor("ShadowColor", Color) = (1,1,1,1)
		_TopCamData ("Top Cam Debug", Vector) = (0,0,0,0)		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM 
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Snow fullforwardshadows vertex:disp tessellate:tessDistance addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6


		#include "Tessellation.cginc"
        #include "UnityPBSLighting.cginc"
		
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



		sampler2D _TopTex;
		fixed4 _TopColor;
		sampler2D _BotTex;
		fixed4 _BotColor;

		struct Input {
			float2 uv_TopTex;
			float2 uv_BotTex;
			float2 uv_Splatmap;
			float2 uv_SnowTex;
			float2 uv_GlitterTex;
			float3 worldPos;
			float3 viewDir;
		};
		
		struct SurfaceOutputSnow
		{
		    fixed3 Albedo;      // base (diffuse or specular) color
		    float3 Normal;      // tangent space normal, if written
		    half3 Emission;
		    half Metallic;      // 0=non-metal, 1=metal
		    // Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
		    // Everywhere in the code you meet smoothness it is perceptual smoothness
		    half Smoothness;    // 0=rough, 1=smooth
		    half Occlusion;     // occlusion (default 1)
		    fixed Alpha;        // alpha for transparencies
			float2 uv_glitter;
			float snowAmount;
		};
		
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		sampler2D _Noise1;
		float _NormalStrength;
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float mipmapLevel(float2 textureCoordinate)
		{
		  float2 dx = ddx(textureCoordinate);
		  float2 dy = ddy(textureCoordinate);
		  float1 deltaMaxSqr = max(dot(dx, dx), dot(dy, dy));
		  return 0.5f * log2(deltaMaxSqr);
		}
		
		// uvParams.xy are the uv coordinates and uvParams.zw contain the texture size in pixels
		float4 tex2Dfold(sampler2D s, float4 uvParams)
		{
		  float mipGrad = mipmapLevel(uvParams.xy * uvParams.zw);
		  float mip = floor(mipGrad);
		  float mipLerp = frac(mipGrad);
		  fixed4 col1 = tex2Dlod(s, float4(uvParams.xy / (pow(2,mip)  ), 0, 0));
		  fixed4 col2 = tex2Dlod(s, float4(uvParams.xy / (pow(2,mip) * 2), 0, 0));
		  return lerp(col1, col2, mipLerp);
		}
		
		sampler2D _SnowTex;
		sampler2D _GlitterTex;
		float4 _GlitterTex_TexelSize;
		float _GlitterThreshold;
		float3 _GlitterColor;
		float _OceanSpecularPower;
		float _OceanSpecularStrength;
		float3 _OceanSpecularColor;
		float _TerrainRimPower;
		float _TerrainRimStrength;
		float3 _TerrainRimColor;
		float3 _TerrainColor;
		float3 _ShadowColor;
		
		float3 GlitterSpecular (float2 uv, float3 N, float3 L, float3 V)
		{
		    // Random glitter direction
			float4 tex = tex2Dfold(_GlitterTex, float4(uv, _GlitterTex_TexelSize.zw));
		    float3 G = normalize(tex.rgb * 2 - 1); // [0,1]->[-1,+1]
		 
		    // Light that reflects on the glitter and hits the eye
		    float3 R = reflect(L, G);
		    float RdotV = dot(R, V);
		 
		    // Only the strong ones (= small RdotV)
		    if (RdotV > _GlitterThreshold)
		        return 0;
		 
		    return (1 - RdotV) * _GlitterColor;
		}


		float3 OceanSpecular (float3 N, float3 L, float3 V)
		{
		    float3 H = normalize(V + L); // Half direction
		    float NdotH = max(0, dot(N, H));
		    float specular = pow(NdotH, _OceanSpecularPower) * _OceanSpecularStrength;
		    return specular * _OceanSpecularColor;
		}
		

		float3 RimLighting(float3 N, float3 V)
		{
		    float rim = 1.0 - saturate(dot(N, V));
		    rim = saturate(pow(rim, _TerrainRimPower) * _TerrainRimStrength);
		    rim = max(rim, 0); // Never negative
		    return rim * _TerrainRimColor;
		}
		
		float3 DiffuseColor(float3 N, float3 L)
		{
		    N.y *= 0.3;
		    float NdotL = saturate(4 * dot(N, L));
		 
		    float3 color = lerp(_ShadowColor, _TerrainColor, NdotL);
		    return color;
		}
				
		float3 nlerp(float3 n1, float3 n2, float t)
		{
		    return normalize(lerp(n1, n2, t));
		}
		
		float3 SandNormal (float2 uv, float3 N)
		{
		    float3 random = tex2D(_SnowTex, uv).rgb;
		    float3 S = normalize(random * 2 - 1);
		    float3 Ns = nlerp(N, S, _NormalStrength);
		    return Ns;
		}
			

		void surf (Input IN, inout SurfaceOutputSnow o) {
			
			half camDistance = distance(_TopCamData.xz, IN.worldPos.xz);
			camDistance = smoothstep(25, 20, camDistance);
			
			half amount = GetTopCamMapLod(IN.worldPos.xz, _TopCamData, _Splatmap).r * camDistance;
			// Albedo comes from a texture tinted by color
			fixed4 snow = tex2D(_TopTex, IN.uv_TopTex) * _TopColor;
			fixed4 ground = tex2D(_BotTex, IN.uv_BotTex) * _BotColor;

			o.snowAmount = amount;
			o.uv_glitter = IN.uv_GlitterTex;
			fixed4 c = lerp(snow, ground, amount);
			float3 N = float3(0, 0, 1);
			N = SandNormal(IN.uv_SnowTex.xy, N);
			o.Albedo = c.rgb;
			o.Normal = N;
			// Metallic and smoothness come from slider variables
			// o.Metallic = _Metallic;
			// o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		inline void LightingSnow_GI(SurfaceOutputSnow s, UnityGIInput data, inout UnityGI gi)
        {
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
#else
    Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
    gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif
        }
		
		float4 LightingSnow (SurfaceOutputSnow s, fixed3 viewDir, UnityGI gi)
		{
		   // Lighting properties
		    float3 L = gi.light.dir;
		    float3 N = s.Normal;
		    float3 V = viewDir;
		 
		    // Lighting calculation
		    float3 diffuseColor = DiffuseColor  (N, L);
		    float3 rimColor     = RimLighting   (N, V);
		    float3 oceanColor   = OceanSpecular (N, L, V);
			float3 glitterColor = GlitterSpecular (s.uv_glitter, N, L, V);
 
		    // Combining
		    float3 specularColor = saturate(max(rimColor, oceanColor));
		    float3 color = diffuseColor + specularColor + glitterColor;
		 
		    // Final color
		    return float4(color * s.Albedo, 1);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
