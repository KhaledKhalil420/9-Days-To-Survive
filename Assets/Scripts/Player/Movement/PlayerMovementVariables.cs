// PlayerMovementVariables.cs
using UnityEngine;

[System.Serializable]
public class PlayerMovementVariables : MonoBehaviour
{
    public static PlayerMovement playerMovement;

    public enum PlayerMovementState
    {
        Idle,
        Walking,
        Running,
        OnAir,
        Crouching, 
        CrouchMoving,
        Sliding
    }

    public void Update()
    {
        HandleStates();
        HandleSpeed();
        HandleMaxSpeed();
    }

    #region State Variables

    [Header("Movement Modules")]
    [SerializeField] protected bool enableJumping = true;
    [SerializeField] protected bool enableCrouching = true;
    [SerializeField] protected bool enableSliding = true;
    [SerializeField] protected bool enableMaxSpeed = true; 

    [Header("State")]
    [SerializeField] protected PlayerMovementState state;

    internal Vector3 inputDir;
    internal Vector3 moveDir;
    internal float currentSpeed;
    protected float currentSlideForce;

    internal bool isCrouching = false;
    internal bool isRunning = false;
    internal bool isSliding = false;

    #endregion

    #region Movement Speeds

    [Header("Forces")]
    [SerializeField] protected float walkingSpeed;
    [SerializeField] protected float runSpeed;
    [SerializeField] protected float airSpeed;
    [SerializeField] protected float crouchSpeed;
    [SerializeField] protected float jumpForce;
    [SerializeField] protected float slideForce;
    [SerializeField] protected float slideTime;
    [SerializeField] protected float speedMultiplier = 1;

    [Header("Max Speed")]
    [SerializeField] protected float maxSpeed = 12f; 

    #endregion

    #region Game Feel

    [Header("Game Feel")]
    [SerializeField] protected float bufferTime;
    [SerializeField] protected float cayoteTime;
    protected float bufferTimer = 0;
    protected float cayoteTimer = 0;

    [Header("Counter Movement")]
    [SerializeField] protected float groundDrag = 6f;
    [SerializeField] protected float counterMovementForce = 10f;

    #endregion

    #region References

    [Header("References")]
    [SerializeField] internal Transform feet;
    [SerializeField] protected Transform head;
    public Rigidbody rb;
    [SerializeField] protected LayerMask unJumpableLayers;
    public CapsuleCollider playerCollider;
    public PlayerLook playerLook;

    #endregion

    #region Sounds

    [Header("Sounds")]
    // [SerializeField] protected FootstepSounds footstepSounds;
    // [SerializeField] protected AudioSource footstepsSource;
    // [SerializeField] protected float footstepsCoolDownWalking = 0.4f;
    // [SerializeField] protected float footstepsCoolDownRunning = 0.15f;

    protected float footstepTimer;

    #endregion

    #region Slope Handling
    
    [Header("Slope Handling")]
    [SerializeField] internal float maxSlopeAngle = 45f;
    protected RaycastHit slopeHit;
    protected bool onSlope;
    protected bool isJumpingThisFrame;
    
    protected bool OnSlope()
    {
        if (Physics.Raycast(feet.position, Vector3.down, out slopeHit, 1.25f, ~unJumpableLayers))
        {
            float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
            onSlope = angle > 0f && angle < 90f;
            return onSlope;
        }
    
        onSlope = false;
        return false;
    }
    
    protected Vector3 GetSlopeMoveDir()
    {
        if (slopeHit.collider == null) return moveDir.normalized;
        Vector3 p = Vector3.ProjectOnPlane(moveDir, slopeHit.normal);
        return p.sqrMagnitude > 0.0001f ? p.normalized : Vector3.zero;
    }
    
    protected float GetSlopeAngle()
    {
        if (slopeHit.collider == null) return 0f;
        return Vector3.Angle(slopeHit.normal, Vector3.up);
    }
    
    protected bool IsTooSteep()
    {
        if (!OnSlope()) return false;
        return GetSlopeAngle() > maxSlopeAngle;
    }
    
    #endregion
    
    #region Moving
    
    protected void ApplyCounterMovement()
    {
        if (IsGrounded())
        {
            rb.linearDamping = groundDrag;
    
            if (moveDir.magnitude < 0.1f)
            {
                Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                if (horizontalVel.magnitude > 0.1f)
                {
                    Vector3 counterForce = -horizontalVel.normalized * counterMovementForce;
                    rb.AddForce(counterForce, ForceMode.Acceleration);
                }
            }
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }
    
    protected void MovePlayer()
    {
        if (IsGrounded() && OnSlope())
        {
            float angle = GetSlopeAngle();
    
            if (IsTooSteep())
            {
                Vector3 downDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                float strength = slideForce + currentSpeed * 0.5f;
    
                if (!isJumpingThisFrame)
                {
                    rb.AddForce(downDir * strength, ForceMode.Acceleration);
                    rb.AddForce(-slopeHit.normal * (counterMovementForce * 0.5f), ForceMode.Acceleration);
                }
            }
            else
            {
                Vector3 slopeMove = GetSlopeMoveDir() * currentSpeed;
                if (slopeMove.sqrMagnitude > 0.0001f)
                    rb.AddForce(slopeMove, ForceMode.Acceleration);
    
                if (!isJumpingThisFrame)
                    rb.AddForce(-slopeHit.normal * (counterMovementForce * 0.3f), ForceMode.Acceleration);
            }
    
            return;
        }
    
        rb.AddForce(moveDir.normalized * currentSpeed, ForceMode.Acceleration);
    }
    
    #endregion

    #region States

    private void SetState(PlayerMovementState newState) => state = newState;

    private void HandleStates()
    {
        if (!IsGrounded())
        {
            SetState(PlayerMovementState.OnAir);
            return;
        }

        if (enableSliding && IsSliding()) return;
        if (enableCrouching && IsCrouchMoving()) return;
        if (enableCrouching && IsCrouching()) return;
        if (IsIdle()) return;
        if (IsRunning()) return;

        SetState(PlayerMovementState.Walking);
    }

    private bool IsCrouching()
    {
        if (isCrouching && rb.linearVelocity.magnitude < 0.1f)
        {
            SetState(PlayerMovementState.Crouching);
            return true;
        }
        return false;
    }


    private bool IsCrouchMoving()
    {
        if (isCrouching && rb.linearVelocity.magnitude > 0.1f)
        {
            SetState(PlayerMovementState.CrouchMoving);
            return true;
        }
        return false;
    }

    private bool IsIdle()
    {
        if (moveDir.magnitude == 0)
        {
            SetState(PlayerMovementState.Idle);
            return true;
        }
        return false;
    }

    private bool IsRunning()
    {
        if (isRunning)
        {
            SetState(PlayerMovementState.Running);
            return true;
        }
        return false;
    }

    private bool IsSliding()
    {
        if (isSliding)
        {
            SetState(PlayerMovementState.Sliding);
            return true;
        }
        return false;
    }

    #endregion

    #region Speed

    private void HandleSpeed()
    {
        switch (state)
        {
            case PlayerMovementState.Idle:
                currentSpeed = 0;
                break;

            case PlayerMovementState.Walking:
                currentSpeed = walkingSpeed;
                break;

            case PlayerMovementState.Running:
                currentSpeed = runSpeed;
                break;

            case PlayerMovementState.OnAir:
                currentSpeed = airSpeed;
                break;

            case PlayerMovementState.Crouching:
                currentSpeed = crouchSpeed;
                break;

            case PlayerMovementState.CrouchMoving:
                currentSpeed = crouchSpeed;
                break;

            case PlayerMovementState.Sliding:
                currentSpeed = airSpeed;
                break;
        }
    }

    private void HandleMaxSpeed()
    {
        if (!enableMaxSpeed) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    #endregion

    #region Collision Detection

    public bool IsGrounded()
    {
        // Slight offset for reliable ground check; keep using unJumpableLayers as originally done
        return Physics.CheckSphere(feet.position, 0.45f, ~unJumpableLayers);
    }

    public bool IsHeaded()
    {
        return Physics.CheckSphere(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y, 0), 0.45f, ~unJumpableLayers);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsHeaded() ? Color.red : Color.green;
        Gizmos.DrawWireSphere(playerCollider.bounds.center + new Vector3(0, playerCollider.bounds.extents.y, 0), 0.45f);

        // draw slope normal / debug
        #if UNITY_EDITOR
        if (feet != null && OnSlope())
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(slopeHit.point, slopeHit.point + slopeHit.normal);
            Gizmos.DrawWireSphere(slopeHit.point, 0.05f);
        }
        #endif
    }

    #endregion

    // #region Footsteps

    // public float FootstepsCooldown()
    // {
    //     float baseCooldown = isRunning ? footstepsCoolDownRunning : footstepsCoolDownWalking;

    //     float speedFactor = moveDir.magnitude / maxSpeed;
    //     float finalCooldown = baseCooldown / (1f + speedFactor * speedMultiplier);

    //     return finalCooldown;
    // }
     
    // #endregion
}

// [System.Serializable]
// public class FootstepSounds
// {
//     public AudioClip[] grass, stone, wood;

//     public AudioClip[] GetClips(SurfaceType surfaceType)
//     {
//         return surfaceType switch
//         {
//             SurfaceType.Grass => grass,
//             SurfaceType.Stone => stone,
//             SurfaceType.Wood => wood,
//             _ => grass
//         };
//     }
// }
