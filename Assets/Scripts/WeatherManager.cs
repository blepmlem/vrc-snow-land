
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WeatherManager : UdonSharpBehaviour
{
    [SerializeField]
    private SnowManager _snowManager;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Light _directionalLight;

    [SerializeField]
    private float _timeOfDay;

    [SerializeField]
    private int _targetWeatherIndex;

    [SerializeField]
    private Weather[] _weatherTemplates;

    [SerializeField]
    private Weather _internalWeather;

    [SerializeField]
    private Weather _internalTargetWeather;

    [SerializeField]
    private float _transitionSpeed = 2;

    private Material _skyBox;

    private readonly int SkyTint = Shader.PropertyToID("_SkyTint");
    private readonly int GroundColor = Shader.PropertyToID("_GroundColor");
    private readonly int Exposure = Shader.PropertyToID("_Exposure");


    private void Start()
    {
        _skyBox = RenderSettings.skybox;
        _internalWeather.Set(_weatherTemplates[_targetWeatherIndex]);
        _internalTargetWeather.Set(_internalWeather);
    }

    private void Update()
    {
       _internalWeather.MoveTowards(_internalTargetWeather, Time.deltaTime * _transitionSpeed);
       UpdateWeatherSystems(_internalWeather);
    }

    [ContextMenu("Set Weather Debug")]
    public void SetWeatherDebug()
    {
        _targetWeatherIndex++;
        if (_targetWeatherIndex > _weatherTemplates.Length - 1)
        {
            _targetWeatherIndex = 0;
        }

        _internalTargetWeather.Set(_weatherTemplates[_targetWeatherIndex]);
    }

    public void SetWeather(Weather w)
    {
        _internalTargetWeather.Set(w);
    }
    
    private void UpdateWeatherSystems(Weather w)
    {
        _snowManager.SetSnowData(w);
        
        _directionalLight.color = w.SunColor;
        _directionalLight.intensity = w.SunIntensity;
        _directionalLight.transform.rotation = Quaternion.Euler(w.SunAngle);
        
        _skyBox.SetColor(SkyTint, w.SkyTint);
        _skyBox.SetColor(GroundColor, w.SkyGroundTint);
        _skyBox.SetFloat(Exposure, w.SkyExposure);
    }
}
