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
    private Deformer[] _trackers;
    
    private readonly int TopCamData = Shader.PropertyToID("_TopCamData");
    public readonly int GlitterColor = Shader.PropertyToID("_GlitterColor");
    public readonly int TerrainRimColor = Shader.PropertyToID("_TerrainRimColor");
    public readonly int TerrainColor = Shader.PropertyToID("_TerrainColor");

    private Material _snowMaterial;
    
    void Start()
    {
        _trackers = GetComponentsInChildren<Deformer>();
        _snowMaterial = _snowPlane.material;
        foreach (var deformer in _trackers)
        {
            deformer.enabled = false;
        }
        Add(Networking.LocalPlayer);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
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
                _trackers[i].enabled = false;
                _trackers[i].RemovePlayer();
                return;
            }
        }
    }

    private void Update()
    {
        var pos = Networking.LocalPlayer.GetPosition() + Vector3.up * _camOffset;
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
                _trackers[i].enabled = true;
                _trackers[i].SetPlayer(p);
                return;
            }
        }
    }

    public void SetSnowData(Weather weather)
    {
        var mat = _snowMaterial;
        mat.SetColor(GlitterColor, weather.SnowGlitter);
        mat.SetColor(TerrainRimColor, weather.SnowRim);
        mat.SetColor(TerrainColor, weather.TerrainColor);
    }
}
