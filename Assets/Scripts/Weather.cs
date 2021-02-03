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

	public void MoveTowards(Weather w, float delta)
	{
		SunColor = MoveTowardsRGB(SunColor, w.SunColor, delta);
		SunIntensity = Mathf.MoveTowards(SunIntensity, w.SunIntensity, delta * 1000);
		SunAngle = Vector3.MoveTowards(SunAngle, w.SunAngle, delta * 1000);
		SnowRim = MoveTowardsRGB(SnowRim, w.SnowRim, delta);
		SnowGlitter = MoveTowardsRGB(SnowGlitter, w.SnowGlitter, delta);
		TerrainColor = MoveTowardsRGB(TerrainColor, w.TerrainColor, delta);
		
		SkyTint = MoveTowardsRGB(SkyTint, w.SkyTint, delta);
		SkyGroundTint = MoveTowardsRGB(SkyGroundTint, w.SkyGroundTint, delta);
		SkyAtmosphereThickness = Mathf.MoveTowards(SkyAtmosphereThickness, w.SkyAtmosphereThickness, delta * 100);
		SkyExposure = Mathf.MoveTowards(SkyExposure, w.SkyExposure, delta * 100);
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
