
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering;
using VRC.SDKBase;
using VRC.Udon;

public class SnowManager : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject _tracker;
    
    private VRCPlayerApi[] _players = new VRCPlayerApi[32];
    private Transform[] _trackers = new Transform[32];
    void Start()
    {
        Add(Networking.LocalPlayer);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Debug.Log($"Player joined: {player.displayName}");
        Add(player);
    }

    private void Update()
    {
        for (var i = 0; i < _players.Length; i++)
        {
            var player = _players[i];
            if (player != null)
            {
                _trackers[i].position = player.GetPosition();
            }
        }
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
