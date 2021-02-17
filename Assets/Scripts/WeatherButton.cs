
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

public class WeatherButton : UdonSharpBehaviour
{
	[FormerlySerializedAs("_weatherManager"),SerializeField]
	private TimeOfDayManager timeOfDayManager;

	public override void Interact()
	{
		timeOfDayManager.SetNextWeather();
	}
}
