#ifndef POI_BRDF
    #define POI_BRDF
    
    float _BRDFSurfaceType;
    float _BRDFMetallic;
    float _BRDFRoughness;
    float _BRDFSpecularGlossiness;
    
    /*
    *    v	    View unit vector
    *    l	    Incident light unit vector
    *    n	    Surface normal unit vector
    *    h	    Half unit vector between l and v
    *    f	    BRDF
    *    fd	    Diffuse component of a BRDF
    *    fr	    Specular component of a BRDF
    *    a	    Roughness, remapped from using input perceptualRoughness
    *    o	    Diffuse reflectance
    *    Ω	    Spherical domain
    *    f0	    Reflectance at normal incidence
    *    f90	Reflectance at grazing angle
    *    χ +(a)	Heaviside function (1 if a>0 and 0 otherwise)
    *    nior	Index of refraction (IOR) of an interface
    *    ⟨n⋅l⟩	 Dot product clamped to [0..1]
    *    ⟨a⟩	Saturated value (clamped to [0..1])
    */
    
    void initializeCommonPixelParams(inout PixelParams pixel, const float4 baseColor)
    {
        
    }
    
    float3 computeDiffuseColor(const float4 baseColor, float metallic)
    {
        return baseColor.rgb * (1.0 - metallic);
    }
    
    float4 poiBRDF(const float4 baseColor)
    {
        PixelParams pixel;
        PoiInitStruct(PixelParams, pixel);
        initializeCommonPixelParams(pixel, baseColor);
        float3 specular;
        //SpecularDFG
        
        if (_BRDFSurfaceType)
        {
            //return pixel.f0 * pixel.dfg.z;
        }
        else
        {
            //return mix(pixel.dfg.xxx, pixel.dfg.yyy, pixel.f0);
        }
        
        return float4(0.0274, .5686, 0.6196, 1);
    }
    
#endif
