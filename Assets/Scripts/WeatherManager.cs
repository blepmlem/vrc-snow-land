
using System;
using System.Linq;
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
    private int _targetWeatherIndex;
    
    

    [SerializeField]
    private Weather _internalWeather;

    [SerializeField]
    private Weather _internalTargetWeather;

    [SerializeField]
    private float _transitionDuration = 5;
    
    [SerializeField]
    private float _t = -1;

    private float _transitionSpeed;
    private Weather[] _weatherTemplates;
    private Material _skyBox;

    private readonly int SkyTint = Shader.PropertyToID("_SkyTint");
    private readonly int GroundColor = Shader.PropertyToID("_GroundColor");
    private readonly int Exposure = Shader.PropertyToID("_Exposure");


    private void Start()
    {
        var allWeather = GetComponentsInChildren<Weather>();
        _weatherTemplates = new Weather[allWeather.Length - 2];
        for (var i = 2; i < allWeather.Length; i++)
        {
            _weatherTemplates[i - 2] = allWeather[i];
        }
        
        _transitionSpeed = 1 / _transitionDuration + Mathf.Epsilon;
        _skyBox = RenderSettings.skybox;
        _internalWeather.Set(_weatherTemplates[_targetWeatherIndex]);
        _internalTargetWeather.Set(_internalWeather);
    }

    private void Update()
    {
        #if UNITY_EDITOR
        _transitionSpeed = 1 / _transitionDuration + Mathf.Epsilon;
        #endif
        
        if (_t < 0)
        {
            return;
        }
            
        _t = Mathf.MoveTowards(_t , 1, Time.deltaTime * _transitionSpeed);
        _internalWeather.Lerp(_internalTargetWeather, _transitionSpeed);
        UpdateWeatherSystems(_internalWeather);

        if (_t >= 1)
        {
            _t = -1;
        }
    }

    [ContextMenu("Set Weather Debug")]
    public void SetWeatherDebug()
    {
        _targetWeatherIndex++;
        if (_targetWeatherIndex > _weatherTemplates.Length - 1)
        {
            _targetWeatherIndex = 0;
        }

        SetWeather(_weatherTemplates[_targetWeatherIndex]);
    }

    public void SetWeather(Weather w)
    {
        _internalTargetWeather.Set(w);
        _t = 0;
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
