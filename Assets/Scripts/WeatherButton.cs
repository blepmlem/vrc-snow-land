
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WeatherButton : UdonSharpBehaviour
{
	[SerializeField]
	private WeatherManager _weatherManager;

	public override void Interact()
	{
		_weatherManager.SetWeatherDebug();
	}
}
