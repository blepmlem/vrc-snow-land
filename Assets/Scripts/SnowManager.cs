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
    private Transform[] _trackers = new Transform[32];
    private readonly int TopCamData = Shader.PropertyToID("_TopCamData");

    void Start()
    {
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
                // _trackers[i] = VRCInstantiate(_tracker).transform;
                return;
            }
        }
    }

    private void Update()
    {
        for (var i = 0; i < _players.Length; i++)
        {
            var player = _players[i];
            if (player != null)
            {
                _trackers[i].position = player.GetPosition() - (Vector3.up * 10);
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
                _trackers[i] = VRCInstantiate(_tracker).transform;
                return;
            }
        }
    }
}
