using System;
using UnityEngine;

// ─────────────────────────────────────────────
//  Structs
// ─────────────────────────────────────────────

[Serializable]
public struct TiltSettings
{
    public bool enabled;

    [Header("Tilt Parameters")]
    [Tooltip("How much the item tilts while moving sideways")]
    public float tiltValue;
    [Tooltip("How smoothly the item tilts")]
    public float tiltSmoothness;

    [HideInInspector] public float angle;
}

[Serializable]
public struct BobSettings
{
    public bool enabled;

    [Header("Bob Parameters")]
    [Tooltip("Speed of bobbing while walking")]
    public float walkBobSpeed;
    [Tooltip("Vertical bob strength while walking")]
    public float walkBobAmount;
    [Tooltip("Horizontal bob strength (X axis)")]
    public float walkBobAmountX;
    [Tooltip("Extra strength multiplier on bob amounts")]
    public float walkBobAmountMultiplier;
    [Tooltip("Speed/amount multiplier while sprinting")]
    public float sprintMultiplier;

    [Header("Sprint Position Shift")]
    [Tooltip("Offset applied to item position while sprinting")]
    public Vector3 sprintPositionShift;
    [Tooltip("Smoothness of sprint shift transition")]
    public float sprintShiftSmoothness;

    [Header("Bob Smoothing")]
    [Tooltip("How quickly bob positions smooth out")]
    public float smoothSpeed;

    [HideInInspector] public float timer;
    [HideInInspector] public float currentBobY;
    [HideInInspector] public float currentBobX;
    [HideInInspector] public Vector3 currentSprintShift;
}

[Serializable]
public struct BreathSettings
{
    public bool enabled;

    [Header("Idle Breath Parameters")]
    [Tooltip("Vertical amplitude of idle breathing")]
    public float breathAmountY;
    [Tooltip("Horizontal amplitude of idle breathing")]
    public float breathAmountX;
    [Tooltip("Speed of idle breathing cycle")]
    public float breathSpeed;
    [Tooltip("How smoothly breathing fades in/out")]
    public float breathSmoothness;

    [HideInInspector] public float timer;
    [HideInInspector] public float currentBreathY;
    [HideInInspector] public float currentBreathX;
}

[Serializable]
public struct LandingSettings
{
    public bool enabled;

    [Header("Landing Parameters")]
    public float baseIntensity;
    public float velocityMultiplier;
    public float maxIntensity;
    public float minimumVelocityThreshold;

    [Header("Recovery")]
    [Tooltip("Curve controlling how the item recovers after landing (X = time 0–1, Y = offset multiplier)")]
    public AnimationCurve recoveryCurve;
    [Tooltip("How long the full recovery takes in seconds")]
    public float recoveryDuration;
}

// ─────────────────────────────────────────────
//  ViewEffects
// ─────────────────────────────────────────────

public class ViewEffects : MonoBehaviour
{
    [Header("Effects")]
    public TiltSettings Tilt;
    public BobSettings Bob;
    public BreathSettings Breath;
    public LandingSettings Landing;

    [Header("References")]
    public PlayerMovementVariables movement;

    [Header("Global Toggle")]
    public bool disable = false;

    // ── private state ──────────────────────────
    private Vector3 _startPos;

    private bool  _wasGrounded;
    private float _lastFallVelocity;
    private float _landingOffset;
    private float _landingTimer;
    private float _landingIntensity;

    // ──────────────────────────────────────────
    private void Start()
    {
        _startPos    = transform.localPosition;
        _wasGrounded = movement.IsGrounded();

        // Ensure a default recovery curve if none is set
        if (Landing.recoveryCurve == null || Landing.recoveryCurve.length == 0)
        {
            Landing.recoveryCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }
    }

    private void Update()
    {
        if (disable || movement == null) return;

        TrackFallVelocity();
        HandleLanding();
        ApplyTilt();
        ApplyBob();        // also handles sprint shift
        ApplyBreath();
        CommitPosition();
    }

    // ── Fall velocity tracker ──────────────────
    private void TrackFallVelocity()
    {
        if (!movement.IsGrounded())
        {
            // Store the downward speed; only track the falling direction
            float vy = -movement.rb.linearVelocity.y;
            if (vy > 0f) _lastFallVelocity = vy;
        }
    }

    // ── Landing ───────────────────────────────
    private void HandleLanding()
    {
        if (!Landing.enabled) { _landingOffset = 0f; _wasGrounded = movement.IsGrounded(); return; }

        bool isGrounded = movement.IsGrounded();

        // Just landed
        if (!_wasGrounded && isGrounded)
        {
            if (_lastFallVelocity > Landing.minimumVelocityThreshold)
            {
                _landingIntensity = Mathf.Min(
                    Landing.baseIntensity + _lastFallVelocity * Landing.velocityMultiplier,
                    Landing.maxIntensity
                );
                _landingTimer = 0f;
            }
            _lastFallVelocity = 0f;
        }

        // Drive recovery with the curve
        if (_landingTimer < Landing.recoveryDuration)
        {
            _landingTimer += Time.deltaTime;
            float t          = Mathf.Clamp01(_landingTimer / Landing.recoveryDuration);
            float curveValue = Landing.recoveryCurve.Evaluate(t);   // 1 → 0 over recovery
            _landingOffset   = -_landingIntensity * curveValue;
        }
        else
        {
            _landingOffset    = 0f;
            _landingIntensity = 0f;
        }

        _wasGrounded = isGrounded;
    }

    // ── Bob + sprint shift ─────────────────────
    private void ApplyBob()
    {
        if (!Bob.enabled)
        {
            Bob.currentBobY     = 0f;
            Bob.currentBobX     = 0f;
            Bob.currentSprintShift = Vector3.zero;
            return;
        }

        bool isMoving = movement.moveDir.magnitude > 0.01f && movement.IsGrounded();

        if (isMoving)
        {
            float speedMult  = movement.isRunning ? Bob.sprintMultiplier : 1f;
            Bob.timer += Time.deltaTime * Bob.walkBobSpeed * speedMult;

            float targetY = Mathf.Sin(Bob.timer)       * Bob.walkBobAmount  * Bob.walkBobAmountMultiplier * speedMult;
            float targetX = Mathf.Cos(Bob.timer * 0.5f) * Bob.walkBobAmountX * Bob.walkBobAmountMultiplier * speedMult;

            Bob.currentBobY = Mathf.Lerp(Bob.currentBobY, targetY, Time.deltaTime * Bob.smoothSpeed);
            Bob.currentBobX = Mathf.Lerp(Bob.currentBobX, targetX, Time.deltaTime * Bob.smoothSpeed);
        }
        else
        {
            Bob.timer       = 0f;
            Bob.currentBobY = Mathf.Lerp(Bob.currentBobY, 0f, Time.deltaTime * Bob.smoothSpeed);
            Bob.currentBobX = Mathf.Lerp(Bob.currentBobX, 0f, Time.deltaTime * Bob.smoothSpeed);
        }

        // Sprint position shift
        Vector3 targetShift = (movement.isRunning && movement.IsGrounded())
            ? Bob.sprintPositionShift
            : Vector3.zero;

        Bob.currentSprintShift = Vector3.Lerp(
            Bob.currentSprintShift,
            targetShift,
            Time.deltaTime * Bob.sprintShiftSmoothness
        );
    }

    // ── Idle breath ────────────────────────────
    private void ApplyBreath()
    {
        if (!Breath.enabled)
        {
            Breath.currentBreathY = 0f;
            Breath.currentBreathX = 0f;
            return;
        }

        bool isIdle = movement.moveDir.magnitude < 0.01f && movement.IsGrounded();

        if (isIdle)
        {
            Breath.timer += Time.deltaTime * Breath.breathSpeed;

            float targetY = Mathf.Sin(Breath.timer)        * Breath.breathAmountY;
            float targetX = Mathf.Cos(Breath.timer * 0.6f) * Breath.breathAmountX; // offset phase

            Breath.currentBreathY = Mathf.Lerp(Breath.currentBreathY, targetY, Time.deltaTime * Breath.breathSmoothness);
            Breath.currentBreathX = Mathf.Lerp(Breath.currentBreathX, targetX, Time.deltaTime * Breath.breathSmoothness);
        }
        else
        {
            Breath.timer          = 0f;
            Breath.currentBreathY = Mathf.Lerp(Breath.currentBreathY, 0f, Time.deltaTime * Breath.breathSmoothness);
            Breath.currentBreathX = Mathf.Lerp(Breath.currentBreathX, 0f, Time.deltaTime * Breath.breathSmoothness);
        }
    }

    // ── Tilt ───────────────────────────────────
    private void ApplyTilt()
    {
        if (!Tilt.enabled) { Tilt.angle = 0f; return; }

        Tilt.angle = Mathf.Lerp(
            Tilt.angle,
            movement.inputDir.x * Tilt.tiltValue,
            Tilt.tiltSmoothness * Time.deltaTime
        );

        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            Tilt.angle
        );
    }

    // ── Write final position ───────────────────
    private void CommitPosition()
    {
        Vector3 bobOffset    = new Vector3(Bob.currentBobX,    Bob.currentBobY,    0f);
        Vector3 breathOffset = new Vector3(Breath.currentBreathX, Breath.currentBreathY, 0f);
        Vector3 landingVec   = new Vector3(0f, _landingOffset, 0f);

        transform.localPosition =
            _startPos
            + bobOffset
            + breathOffset
            + landingVec
            + Bob.currentSprintShift;
    }

    // ── Public helpers ─────────────────────────
    public void ResetView()
    {
        Bob.timer              = 0f;
        Bob.currentBobY        = 0f;
        Bob.currentBobX        = 0f;
        Bob.currentSprintShift = Vector3.zero;

        Breath.timer          = 0f;
        Breath.currentBreathY = 0f;
        Breath.currentBreathX = 0f;

        _landingOffset    = 0f;
        _landingTimer     = 0f;
        _landingIntensity = 0f;
        _lastFallVelocity = 0f;

        Tilt.angle = 0f;

        transform.localPosition    = _startPos;
        transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            0f
        );
    }
}