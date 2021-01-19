Shader "MTA13532/SAND" {
	Properties {
		_Color ("Diffuse Color", Color) = (1,1,1,1) 
		_MainTex ("Texturezl", 2D) = "white" {}
		_Ramp ("Ramp", 2D) = "gray" {} 
		_ScrollSpeed ("Shininess", Range(0,5)) = 1
		_Gloss ("Glossynessess", Range(-5,5)) = 1
		_Shininess ("Shiny", Range(0,25)) = 1
		_SpecMap ("Specular map", 2D) = "black" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
CGPROGRAM
#pragma surface surf ToonRamp

	sampler2D _Ramp;
	sampler2D _SpecMap;
	uniform float _ScrollSpeed;
	uniform float _Gloss;
	uniform float _Shininess;

#pragma lighting ToonRamp exclude_path:prepass

inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
	#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
	#endif
	

	half3 h = normalize (lightDir + viewDir);
	half reflectAngle = max (0, dot (s.Normal, h));

	half d = dot (s.Normal, lightDir)*0.5 + 0.5;
	half3 ramp = tex2D(_Ramp, float2(d,d)).rgb;
	half4 c;
	float3 spec = 0;

 	//if(reflectAngle > _ScrollSpeed){
		float specBase = saturate(dot(s.Normal, h));
    	spec = s.Specular * pow(specBase, s.Gloss * 128) * _LightColor0.rgb;
	//}
	half refl = pow(max(0.0, dot( reflect(-lightDir, s.Normal), viewDir)), _Shininess);

	half ran = frac(sin(dot(s.Normal.xy ,half2(12.9898,78.233))) * (43758.5453+ _Time.y));

    c.rgb = ((s.Albedo * _LightColor0.rgb * ramp) * (atten * 2)) + (refl*spec*_Gloss*ran);
	//float hax = frac( cos( fmod( 123456789., 1e-7 * dot(23.1406926327792690,2.6651441426902251) ) ) );
    //half3 rampDiffuse = tex2D(_SpecMap, float2(d*2, d)).rgb;


	c.a = 0;
	return c;
}
 

	sampler2D _MainTex;
	float4 _Color;

	struct Input {
		float2 uv_MainTex : TEXCOORD0;
  		float2 uv_SpecMap;
  		float3 worldPos;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed2 scrolledUV = IN.uv_MainTex;
        fixed xScrollValue = 1;
        fixed yScrollValue = _ScrollSpeed * _Time;

        scrolledUV += fixed2 (xScrollValue, yScrollValue);

		half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb;
		//o.uv = IN.texcoord.xy * uv_MainTex.xy + uv_MainTex.zw;
		o.Specular = tex2D(_SpecMap, scrolledUV).rgb;
		o.Alpha = c.a;
	}
ENDCG

	} 

	Fallback "VertexLit"
}
