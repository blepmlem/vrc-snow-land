using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UdonSharp;

public class SnowboardMovement : UdonSharpBehaviour
{

    public LayerMask SlopeMask;
    public Text DebugText;
    public float wantedDistanceToGround = 0.02f;
    public float acceleration = 1f;
    public float friction = 3f;
    public float gravity = 0.5f;

    private Vector2 momentum = Vector2.zero;
    private Vector2 direction = new Vector2(1, 0);
    private float gravityMomentum = 0;
    private bool isFalling = false;

 

    // Update is called once per frame
    void Update()
    {
  

        DebugText.text = "momentum: " + momentum + ", total speed: "+ momentum.magnitude + ",  transform.eulerAngles.x: " +  transform.eulerAngles.x ;

        Vector3 movement = transform.forward * momentum.x + transform.right * momentum.y;
        transform.position = transform.position + movement;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.2f, SlopeMask))
        {
           
            momentum += Time.deltaTime * direction * acceleration * ScaledToAngle();
            momentum -= Time.deltaTime * momentum * friction;

            transform.position = transform.position + ((hit.distance - 1) - wantedDistanceToGround) * Vector3.down;
            transform.rotation = Quaternion.FromToRotation (transform.up, hit.normal) * transform.rotation;
            gravityMomentum = 0;
            isFalling = false;
        }
        else
        {
            if(!isFalling)
            {
                gravityMomentum = -movement.y;
                isFalling = true;
            }

            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            gravityMomentum += Time.deltaTime * gravity;
            transform.position += Vector3.down * gravityMomentum;
        }
    }

    private float ScaledToAngle()
    {
        if(transform.eulerAngles.x > 0 && transform.eulerAngles.x < 90)
        {
            return transform.eulerAngles.x / 90f;
        }
        else
        {
            return 0;
        }
    }
}
