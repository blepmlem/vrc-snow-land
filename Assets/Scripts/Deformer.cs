
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Deformer : UdonSharpBehaviour
{
	private VRCPlayerApi _player;

    [SerializeField]
    private ParticleSystem _particles;

    private void Start()
    {
	    _particles.transform.localPosition = Vector3.up * -30;
    }

    public void SetPlayer(VRCPlayerApi player)
    {
	    _player = player;
    }

    public void RemovePlayer()
    {
	    _player = null;
    }
    
    private void Update()
    {
	    if (_player == null)
	    {
		    return;
	    }
	    
	    transform.position = _player.GetPosition();
    }
}
