using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Weather : UdonSharpBehaviour
{
	[Header("Sun")]
	[ColorUsage(false, true)]
	public Color SunColor;
	public float SunIntensity;
	public Vector3 SunAngle;
	
	[Header("Sky")]
	[ColorUsage(false, true)]
	public Color SkyTint;
	[ColorUsage(false, true)]
	public Color SkyGroundTint;
	public float SkyAtmosphereThickness;
	public float SkyExposure;

	[Header("Snow")]
	[ColorUsage(false, true)]
	public Color SnowRim;
	[ColorUsage(false, true)]
	public Color SnowGlitter;
	[ColorUsage(false, true)]
	public Color TerrainColor;

	public void Set(Weather w)
	{
		SunColor = w. SunColor;
		SunIntensity = w. SunIntensity;
		SunAngle = w. SunAngle;
		
		SnowRim = w. SnowRim;
		SnowGlitter = w. SnowGlitter;
		TerrainColor = w. TerrainColor;
		
		SkyTint = w.SkyTint;
		SkyGroundTint = w.SkyGroundTint;
		SkyAtmosphereThickness = w.SkyAtmosphereThickness;
		SkyExposure = w.SkyExposure;
	}

	public void Lerp(Weather w, float t)
	{
		SunColor = Color.Lerp(SunColor, w.SunColor, t);
		SunIntensity = Mathf.Lerp(SunIntensity, w.SunIntensity, t);
		SunAngle = Vector3.Slerp(SunAngle, w.SunAngle, t);
		SnowRim = Color.Lerp(SnowRim, w.SnowRim, t);
		SnowGlitter = Color.Lerp(SnowGlitter, w.SnowGlitter, t);
		TerrainColor = Color.Lerp(TerrainColor, w.TerrainColor, t);
		
		SkyTint = Color.Lerp(SkyTint, w.SkyTint, t);
		SkyGroundTint = Color.Lerp(SkyGroundTint, w.SkyGroundTint, t);
		SkyAtmosphereThickness = Mathf.Lerp(SkyAtmosphereThickness, w.SkyAtmosphereThickness, t);
		SkyExposure = Mathf.Lerp(SkyExposure, w.SkyExposure, t);
	}
	
	private Color MoveTowardsRGB(Color a, Color b, float delta)
	{
		float outputR = Mathf.MoveTowards(a.r, b.r, delta);
		float outputG = Mathf.MoveTowards(a.g, b.g, delta);
		float outputB = Mathf.MoveTowards(a.b, b.b, delta);

		return new Color(outputR, outputG, outputB);
	}
	
	private Color MoveTowardsHSV(Color a, Color b, float delta)
	{
		float a_h;
		float a_s;
		float a_v;
		Color.RGBToHSV(a, out a_h, out a_s, out a_v);
		
		float b_h;
		float b_s;
		float b_v;
		Color.RGBToHSV(b, out b_h, out b_s, out b_v);

		float h = Mathf.MoveTowards(a_h, b_h, delta);
		float s = Mathf.MoveTowards(a_s, b_s, delta);
		float v = Mathf.MoveTowards(a_v, b_v, delta);

		return Color.HSVToRGB(h,s,v);
	}
}
