﻿using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using UnityEngine.XR;
using VRC.SDKBase;

public class SnowboardMovement : UdonSharpBehaviour
{

    public Transform StartPosition;
    public LayerMask SlopeMask;
    public float wantedDistanceToGround = 0.15f;
    public float acceleration = 1f;
    public float friction = 3f;
    public float gravity = 0.5f;
    public float ResetTimeoutAfterUsage = 60f * 5;
    public bool RunWithoutPlayer = false;
    private float BoostCooldown = 2.5f;


    // The player collider needs to be off the ground otherwise they lag
    public float playerYOffset = 0.15f;
    public float RotationMultiplier = 80;

    // Desktop input modifiers
    public float DesktopSnowboardRotationSpeed = 80;
    public float DesktopSnowboardDeadZone = 0.01f;
    public KeyCode DesktopJumpOffSnowboardKey = KeyCode.F;
    public KeyCode DesktopSnowboardBoostKey = KeyCode.Q;


    private Vector2 momentum = Vector2.zero;
    private float gravityMomentum = 0;
    private bool isFalling = false;

    private VRCPlayerApi localUser;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float lastUsageTime = -1;
    private float lastBoostTime = -1;

    public void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }


    public override void Interact()
    {
        if (localUser == null)
        {
            localUser = Networking.LocalPlayer;
            localUser.TeleportTo(StartPosition.position, StartPosition.rotation);
            transform.position = StartPosition.position;
            transform.rotation = StartPosition.rotation;
            momentum = Vector2.zero;
            gravityMomentum = 0;
        }
        else
        {
            localUser = null;
            lastUsageTime = Time.time;
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (localUser != null || RunWithoutPlayer)
        {
            if (Input.GetKeyDown(DesktopJumpOffSnowboardKey)) localUser = null;
            SnowboardingUpdate();
        }

        if (transform.position.y < -100)
        {
            ResetSnowboard();
        }
        else if (lastUsageTime != -1 && Time.time - lastUsageTime > ResetTimeoutAfterUsage)
        {
            ResetSnowboard();
        }
    }

    private void ResetSnowboard()
    {
        localUser = null;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        lastUsageTime = -1;
    }

    private void SnowboardingUpdate()
    {
        // Turn board
        if (localUser != null)
        {
            if (localUser.IsUserInVR())
            {
                VrPlayerInputRotationUpdate();
            }
            else
            {
                DesktopPlayerInputRotationUpdate();
            }
        }


        // Boost
        string rightHand_AButton = "Fire2";
        if (Time.time - lastBoostTime > BoostCooldown && (Input.GetKeyDown(DesktopSnowboardBoostKey) || Input.GetButtonDown(rightHand_AButton)))
        {
            float boost = 2;
            momentum.x += boost;
            lastBoostTime = Time.time;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.2f, SlopeMask))
        {
            // On ground
            UpdateGroundMomentum(transform.forward, hit.normal);

            Vector3 wantedMovement = transform.forward * momentum.x + transform.right * momentum.y;
            wantedMovement = Vector3.ProjectOnPlane(wantedMovement, hit.normal).normalized * wantedMovement.magnitude;
            transform.position += wantedMovement * Time.deltaTime;


            Vector3 correctedPosition = transform.position + ((hit.distance - 1) - wantedDistanceToGround) * Vector3.down;
            Quaternion correctedRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, correctedRotation, 10f * Time.deltaTime);
            if (correctedPosition.y > transform.position.y)
            {
                transform.position = correctedPosition;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, correctedPosition, 10f * Time.deltaTime);
            }

            gravityMomentum = 0;
            isFalling = false;

            // Ensure player is not inside other colliders
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.2f, SlopeMask))
            {
                correctedPosition = transform.position + ((hit.distance - 1) - wantedDistanceToGround) * Vector3.down;
                if (correctedPosition.y > transform.position.y)
                {
                    transform.position = correctedPosition;
                }
            }
        }
        else
        {
            Vector3 wantedMovement = transform.forward * momentum.x * Time.deltaTime + transform.right * momentum.y * Time.deltaTime;
            transform.position = transform.position + wantedMovement;
            // In air
            if (!isFalling)
            {
                // First frame falling
                gravityMomentum = -wantedMovement.y;
                isFalling = true;
            }

            transform.position += Vector3.down * gravityMomentum;

            Quaternion correctedRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, correctedRotation, 10f * Time.deltaTime);
            // Update gravity momentum
            gravityMomentum += Time.deltaTime * gravity;
        }


        // Move player
        if (localUser != null)
        {

            if (localUser.IsUserInVR())
            {
                localUser.TeleportTo(transform.position + Vector3.up * playerYOffset, StartPosition.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, true);
            }
            else
            {
                localUser.TeleportTo(transform.position + Vector3.up * playerYOffset, localUser.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.Default, true);
            }
        }
    }

    private void DesktopPlayerInputRotationUpdate()
    {
        float turnInput = Input.GetAxis("Horizontal");

        // Check if input is within the deadzone
        if (DesktopSnowboardDeadZone > turnInput && turnInput > -DesktopSnowboardDeadZone) return;

        // Turn
        Vector3 wantedForward = (new Vector3(transform.position.x + transform.right.x * turnInput, 0, transform.position.z + transform.right.z * turnInput) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        Quaternion wantedRotation = Quaternion.LookRotation(wantedForward, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, RotationMultiplier * Time.deltaTime);
    }

    private void VrPlayerInputRotationUpdate()
    {
        VRCPlayerApi.TrackingData rightHand = localUser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
        Vector3 wantedForward = (new Vector3(rightHand.position.x, 0, rightHand.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        Quaternion wantedRotation = Quaternion.LookRotation(wantedForward, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, RotationMultiplier * Time.deltaTime);
    }

    private void UpdateGroundMomentum(Vector3 playerForward, Vector3 hitNormal)
    {
        float scaledAngle = ScaledToAngle(playerForward, hitNormal);
        momentum += Time.deltaTime * Vector2.right * acceleration * scaledAngle - Time.deltaTime * momentum * friction;
    }

    /// <summary>
    /// Returns an normalized value between 0 - 1.
    /// Where 1 means the board should points straight down along the hill
    /// Where 0 means the board points upwards
    /// </summary>
    /// <param name="playerForward"></param>
    /// <param name="hitNormal"></param>
    /// <returns></returns>
    private float ScaledToAngle(Vector3 playerForward, Vector3 hitNormal)
    {
        Vector3 hitForward = new Vector3(hitNormal.x, 0, hitNormal.z).normalized;
        Vector3 playerNormalizedForward = new Vector3(playerForward.x, 0, playerForward.z).normalized;

        // How steep is the slope
        float slopeAngle = Vector3.Angle(hitForward, hitNormal);

        // How much of the steepness is the player facing
        // upDownAngle: 0 - 90 = player is facing downhill
        // upDownAngle: 90 - 180 = player is facing uphill
        float upDownAngle = Vector3.Angle(playerNormalizedForward, hitForward);

        if (upDownAngle > 90) return 0;
        float angle = slopeAngle * (90 - upDownAngle) / 90;

        //Debug.Log("slopeAngle: " + slopeAngle + ", upDownAngle: " + upDownAngle + ", angle: "+ angle + ", (upDownAngle) / 90: " + ((90 - upDownAngle) / 90));
        if (angle > 0 && angle < 90)
        {
            return Mathf.Sin(angle * Mathf.PI / 180);
        }
        else
        {
            return 0;

        }
    }
}
