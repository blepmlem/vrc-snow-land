using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SnowManager : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject _tracker;

    [SerializeField]
    private Renderer[] _snowPlanes;

    [SerializeField]
    private Camera _snowCam;

    [SerializeField]
    private float _camOffset = 50;
    
    private VRCPlayerApi[] _players = new VRCPlayerApi[32];
    
    [SerializeField]
    private Deformer[] _trackers;
    
    private readonly int TopCamData = Shader.PropertyToID("_TopCamData");
    public readonly int GlitterColor = Shader.PropertyToID("_GlitterColor");
    public readonly int TerrainRimColor = Shader.PropertyToID("_TerrainRimColor");
    public readonly int TerrainColor = Shader.PropertyToID("_TerrainColor");

    private bool _initialized = false;
    private VRCPlayerApi _localPlayer;

    private Material _snowMaterial;
    
    private void Initialize()
    {
        if (_snowMaterial == null)
        {
            _snowMaterial = _snowPlanes[0].sharedMaterial;
        }
        
        var localPlayer = Networking.LocalPlayer;
        if (localPlayer != null)
        {
            Debug.Log($"Initialized! Welcome {localPlayer.displayName}!");
            _initialized = true;
            _localPlayer = Networking.LocalPlayer;
            Add(_localPlayer);
        }
        
        _snowMaterial.color = Color.black;;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Debug.Log($"Player joined: {player.displayName}");
        Add(player);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Debug.Log($"Player left: {player.displayName}");
        Remove(player);
    }

    private void Remove(VRCPlayerApi player)
    {
        for (var i = 0; i < _players.Length; i++)
        {
            if (_players[i] == player)
            {
                _players[i] = null;
                _trackers[i].enabled = false;
                _trackers[i].RemovePlayer();
                return;
            }
        }
    }

    private void Update()
    {
        if (!_initialized)
        {
            Initialize();
            return;
        }
        
        var pos = _localPlayer.GetPosition() + Vector3.up * _camOffset;
        float size = 1f / _snowCam.orthographicSize;
        _snowCam.transform.position = pos;
        
        _snowMaterial.SetVector("_TopCamData", new Vector4(pos.x, pos.y, pos.z, size));
    }

    private void Add(VRCPlayerApi p)
    {
        for (var i = 0; i < _players.Length; i++)
        {
            if (_players[i] == null)
            {
                _players[i] = p;
                _trackers[i].SetPlayer(p);
                return;
            }
        }
    }

    public void SetSnowData(Weather weather)
    {
        if (_snowMaterial == null)
        {
            _snowMaterial = _snowPlanes[0].sharedMaterial;
        }
        
        var mat = _snowMaterial;
        mat.SetColor(GlitterColor, weather.SnowGlitter);
        mat.SetColor(TerrainRimColor, weather.SnowRim);
        mat.SetColor(TerrainColor, weather.TerrainColor);
    }
}
