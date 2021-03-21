
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(Rigidbody))]
public class Snowball : UdonSharpBehaviour
{
    [SerializeField, Header("Size Gain")]
    private float _maxSize = 2f;
    [SerializeField]
    private float _sizeGainSpeed = 0.1f;
    [SerializeField]
    private float _growthMinVelocitySqrMagnitude = .01f;

    [SerializeField, Header("Impact")]
    private float _impactMinVelocitySqrMagnitude = .1f;
    [SerializeField]
    private ParticleSystem _impactParticles;

    private int _deformablelayer = 23;
    private Rigidbody _rigidbody;
    private Vector3 _maxSizeVector;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _maxSizeVector = Vector3.one * _maxSize;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == _deformablelayer)
        {
            return;
        }
        
        var sqrMag = _rigidbody.velocity.sqrMagnitude;

        if (sqrMag < _impactMinVelocitySqrMagnitude)
        {
            return;
        }
        
        _impactParticles.Play();
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.layer != _deformablelayer)
        {
            return;
        }

        var sqrMag = _rigidbody.velocity.sqrMagnitude;

        if (sqrMag < _growthMinVelocitySqrMagnitude)
        {
            return;
        }

        var gain = sqrMag * _sizeGainSpeed * Time.fixedDeltaTime;

        _rigidbody.mass = Mathf.MoveTowards(_rigidbody.mass, _maxSize, gain);
        var scale = transform.localScale;
        scale = Vector3.MoveTowards(scale, _maxSizeVector, gain);
        transform.localScale = scale;
    }
}
