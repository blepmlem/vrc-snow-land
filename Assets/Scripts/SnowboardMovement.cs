using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SnowboardMovement : UdonSharpBehaviour
{

    public Transform StartPosition;
    public LayerMask SlopeMask;
    public float wantedDistanceToGround = 0.15f;
    public float acceleration = 1f;
    public float friction = 3f;
    public float gravity = 0.5f;

    // The player collider needs to be off the ground otherwise they lag
    public float playerYOffset = 0.15f;
    public float RotationMultiplier = 80;

    private Vector2 momentum = Vector2.zero;
    private Vector2 direction = new Vector2(1, 0);
    private float gravityMomentum = 0;
    private bool isFalling = false;

    private VRCPlayerApi localUser;


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
        }
    }


    // Update is called once per frame
    void Update()
    {

        if (localUser != null)
        {
            SnowboardingUpdate();
        }
    }

    private void SnowboardingUpdate()
    {
        Vector3 movement = transform.forward * momentum.x + transform.right * momentum.y;
        transform.position = transform.position + movement;

        // Turn board
        if (localUser != null)
        {
            if (localUser.IsUserInVR())
            {
                VRCPlayerApi.TrackingData rightHand = localUser.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
                Vector3 wantedForward = (new Vector3(rightHand.position.x, 0, rightHand.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
                Quaternion wantedRotation = Quaternion.LookRotation(wantedForward, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, wantedRotation, RotationMultiplier * Time.deltaTime);
            }
        }


        // Update momentum
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.2f, SlopeMask))
        {
            // Is on ground
            momentum += Time.deltaTime * direction * acceleration * ScaledToAngle();
            momentum -= Time.deltaTime * momentum * friction;

            transform.position = transform.position + ((hit.distance - 1) - wantedDistanceToGround) * Vector3.down;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            gravityMomentum = 0;
            isFalling = false;
        }
        else
        {
            // Is falling in air
            if (!isFalling)
            {
                gravityMomentum = -movement.y;
                isFalling = true;
            }
            transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            gravityMomentum += Time.deltaTime * gravity;
            transform.position += Vector3.down * gravityMomentum;
        }

        // Move player
        if (localUser != null)
        {
            localUser.TeleportTo(transform.position + Vector3.up * playerYOffset, StartPosition.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, true);
        }
    }

    private float ScaledToAngle()
    {
        if (transform.eulerAngles.x > 0 && transform.eulerAngles.x < 90)
        {
            return transform.eulerAngles.x / 90f;
        }
        else
        {
            return 0;
        }
    }
}
