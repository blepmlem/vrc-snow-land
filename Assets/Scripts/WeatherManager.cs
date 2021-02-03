
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
    private Weather[] _weatherTemplates;

    
    void Start()
    {
        
    }
}
