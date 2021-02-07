using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SnowManager : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject _tracker;

    [SerializeField]
    private Renderer _snowPlane;
    
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

    private Material _snowMaterial;
    
    void Start()
    {
        _snowMaterial = _snowPlane.material;
        foreach (var deformer in _trackers)
        {
            deformer.enabled = false;
        }
        Add(Networking.LocalPlayer);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Debug.Log($"Player joined: {player.displayName}");
        Add(player);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Remove(player);
    }

    private void Remove(VRCPlayerApi player)
    {
        for (var i = 0; i < _players.Length; i++)
        {
            if (_players[i] == player)
            {
                _players[i] = null;
                _trackers[i].Player = null;
                _trackers[i].enabled = false;
                //Repool trackers eventually
                return;
            }
        }
    }

    private void Update()
    {
        #if !UDONSHARP_COMPILER && UNITY_EDITOR
        return;
        #endif
        
        for (var i = 0; i < _players.Length; i++)
        {
            var player = _players[i];
            if (player != null)
            {
                _trackers[i].transform.position = player.GetPosition();
            }
        }
        
        var pos = Networking.LocalPlayer.GetPosition() + Vector3.up * _camOffset;
        float size = 1f / _snowCam.orthographicSize;
        _snowCam.transform.position = pos;
        _snowPlane.material.SetVector("_TopCamData", new Vector4(pos.x, pos.y, pos.z, size));
    }

    private void Add(VRCPlayerApi p)
    {
        for (var i = 0; i < _players.Length; i++)
        {
            if (_players[i] == null)
            {
                _players[i] = p;
                var tracker = VRCInstantiate(_tracker.gameObject).GetComponent<Deformer>();
                tracker.Player = p;
                tracker.enabled = true;
                _trackers[i] = tracker;
                return;
            }
        }
    }

    public void SetSnowData(Weather weather)
    {
        var mat = _snowPlane.material;
        mat.SetColor(GlitterColor, weather.SnowGlitter);
        mat.SetColor(TerrainRimColor, weather.SnowRim);
        mat.SetColor(TerrainColor, weather.TerrainColor);
    }
}
