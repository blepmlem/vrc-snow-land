
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SnowDeformerObject : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject toSpawn;

    [SerializeField]
    private float _offset = 25f;
    
    private Transform instance;

    
    private void Start()
    {
        instance = VRCInstantiate(toSpawn).transform;
    }

    private void Update()
    {
        instance.position = transform.position + Vector3.down * _offset;
    }
}
