using System;
using UdonSharp;
using UdonSharpEditor;
using UnityEngine;
using VRC.SDKBase;

public class Deformer : UdonSharpBehaviour
{
    public VRCPlayerApi Player;

    [SerializeField]
    private ParticleSystem _particles;
    private void Start()
    {
        _particles.Pause();
    }

    private void Update()
    {
        bool isPlayer = Player != null;
        var position = isPlayer ? Player.GetPosition() : transform.position;
        _particles.transform.position = position + Vector3.down * 50;
        
        if (!isPlayer)
        {
            return;
        }
        
        if (Player.IsPlayerGrounded())
        {
            _particles.Play();
        }
        else
        {
            _particles.Pause();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _particles.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        _particles.Pause();
    }
}
