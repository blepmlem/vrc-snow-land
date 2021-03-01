#ifndef SUBSURFACE
    #define SUBSURFACE
    
    float _SSSThicknessMod;
    float _SSSSCale;
    float _SSSPower;
    float _SSSDistortion;
    float4 _SSSColor;
    float _EnableSSS;
    
    #if defined(PROP_SSSTHICKNESSMAP) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_SSSThicknessMap);
    #endif
    
    float3 calculateSubsurfaceScattering()
    {
        #if defined(PROP_SSSTHICKNESSMAP) || !defined(OPTIMIZER_ENABLED)
            float SSS = 1 - POI2D_SAMPLER_PAN(_SSSThicknessMap, _MainTex, poiMesh.uv[_SSSThicknessMapUV], _SSSThicknessMapPan);
        #else
            float SSS = 1;
        #endif
        half3 vLTLight = poiLight.direction + poiMesh.normals[0] * _SSSDistortion;
        half flTDot = pow(saturate(dot(poiCam.viewDir, -vLTLight)), _SSSPower) * _SSSSCale;
        #ifdef FORWARD_BASE_PASS
            half3 fLT = (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #else
            half3 fLT = poiLight.attenuation * (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #endif
        
        return fLT * poiLight.color * _SSSColor;
    }
    
#endif