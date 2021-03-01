#ifndef POI_DECAL
    #define POI_DECAL
    
    #if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalTexture);
    #endif
    
    #if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalMask);
    #endif
    
    float4 _DecalColor;
    fixed _DecalTiled;
    float _DecalBlendType;
    half _DecalRotation;
    half2 _DecalScale;
    half2 _DecalPosition;
    half _DecalRotationSpeed;
    float _DecalEmissionStrength;
    float _DecalBlendAlpha;
    float _DecalHueShiftEnabled;
    float _DecalHueShift;
    float _DecalHueShiftSpeed;
    
    void applyDecal(inout float4 albedo, inout float3 decalEmission)
    {
        float2 uv = poiMesh.uv[_DecalTextureUV];
        float2 decalCenter = _DecalPosition;
        float theta = radians(_DecalRotation + _Time.z * _DecalRotationSpeed);
        float cs = cos(theta);
        float sn = sin(theta);
        uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
        uv = remap(uv, float2(0, 0) - _DecalScale / 2 + _DecalPosition, _DecalScale / 2 + _DecalPosition, float2(0, 0), float2(1, 1));
        
        half decalAlpha = 1;
        //float2 uv = TRANSFORM_TEX(poiMesh.uv[_DecalTextureUV], _DecalTexture) + _Time.x * _DecalTexturePan;
        #if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
            float4 decalColor = POI2D_SAMPLER_PAN(_DecalTexture, _MainTex, uv, _DecalTexturePan) * _DecalColor;
        #else
            float4 decalColor = _DecalColor;
        #endif
        
        UNITY_BRANCH
        if (_DecalHueShiftEnabled)
        {
            decalColor.rgb = hueShift(decalColor.rgb, _DecalHueShift + _Time.x * _DecalHueShiftSpeed);
        }
        
        #if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
            decalAlpha *= POI2D_SAMPLER_PAN(_DecalMask, _MainTex, poiMesh.uv[_DecalMaskUV], _DecalMaskPan).r;
        #endif
        
        UNITY_BRANCH
        if(!_DecalTiled)
        {
            if(uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
            {
                decalAlpha = 0;
            }
        }
        
        albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType), decalColor.a * decalAlpha * _DecalBlendAlpha);
        albedo = saturate(albedo);
        decalEmission = decalColor.rgb * decalColor.a * decalAlpha * _DecalEmissionStrength;
    }
    
#endif