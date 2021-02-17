using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class TimeOfDayManager : UdonSharpBehaviour
{
    [SerializeField]
    private Animator[] _animators;

    [SerializeField]
    private float _dayNightCycleDuration;

    [UdonSynced]
    private float _tSynced;

    [SerializeField, Range(0,1)]
    private float _t = 0;

    [SerializeField]
    private bool _pauseTime;
    
    [SerializeField]
    private Camera _skyboxCamera;
    //
    // [SerializeField, Header("Colors for the fog over a day. Alpha is density")]
    // private Gradient _fogGradient;

    [SerializeField]
    private Color[] _fogColors;

    private bool _timeWasSet;
    private bool _initialized;
    private readonly int TimeOfDay = Animator.StringToHash("TimeOfDay");

    private void Initialize()
    {
        if (Networking.IsMaster)
        {
            _t = Random.Range(0, 1);
        }
        
        _initialized = true;
    }
    
    private void Start()
    {
        if (!_initialized)
        {
            
            Initialize();
        }        
    }

    private void Update()
    {
        if(!_pauseTime)
        {
            _t += Time.deltaTime * (1f / _dayNightCycleDuration);
            _tSynced = _t;
            if (_t >= 1)
            {
                _t = 0;
            }
        }
        
        foreach (Animator animator in _animators)
        {
            animator.SetFloat(TimeOfDay, _t);
        }

        // var color = _fogGradient.Evaluate(_t);
        // RenderSettings.fogColor = color;
        // RenderSettings.fogDensity = color.a * 0.004f;
        
        var color = _fogColors[Mathf.RoundToInt(_t * (_fogColors.Length - 1))];
        color = Vector4.MoveTowards(RenderSettings.fogColor, color, Time.deltaTime * (1 / _dayNightCycleDuration));
        RenderSettings.fogColor = color;
        RenderSettings.fogDensity = color.a * 0.004f;
		
        // var p = Networking.LocalPlayer;
        // if (p == null)
        // {
        //     return;
        // }
		      //
        // var rotation = p.GetRotation();
        // _skyboxCamera.transform.rotation = rotation;
    }
    
    public void SetNextWeather()
    {
        // SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(SetNextWeather_OWNER));
    }
    
    public void SetNextWeather_OWNER()
    {
        // _targetWeatherIndex++;
        // if (_targetWeatherIndex >= _weatherTemplates.Length)
        // {
        //     _targetWeatherIndex = 0;
        // }
        //
        // _targetWeatherIndexSynced = _targetWeatherIndex;
        // SetWeatherInternal(_weatherTemplates[_targetWeatherIndex]);
    }

    public override void OnDeserialization()
    {
        if (Mathf.Abs(_tSynced - _t) > .01f)
        {
            _t = _tSynced;
        }
    }
}
