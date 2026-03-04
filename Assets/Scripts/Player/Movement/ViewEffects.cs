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
    public float walkBobSpeed;
    public float walkBobAmount;
    public float walkBobAmountX;
    public float walkBobAmountMultiplier;
    public float sprintMultiplier;

    [Header("Sprint Position Shift")]
    public Vector3 sprintPositionShift;
    public float   sprintShiftSmoothness;

    [Header("Bob Smoothing")]
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
    public float breathAmountY;
    public float breathAmountX;
    public float breathSpeed;
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

    [Header("Position Recovery")]
    public AnimationCurve recoveryCurve;
    public float recoveryDuration;

    [Header("Landing Rotation")]
    [Tooltip("Peak pitch (X-axis) rotation on landing")]
    public float pitchIntensity;
    [Tooltip("Peak roll (Z-axis) rotation on landing")]
    public float rollIntensity;
    public AnimationCurve rotationRecoveryCurve;
}

// ─────────────────────────────────────────────
//  ViewEffects
// ─────────────────────────────────────────────

public class ViewEffects : MonoBehaviour
{
    [Header("Effects")]
    public TiltSettings    Tilt;
    public BobSettings     Bob;
    public BreathSettings  Breath;
    public LandingSettings Landing;

    [Header("References")]
    public PlayerMovementVariables movement;

    [Header("Global Toggle")]
    public bool disable = false;

    // ── cached refs ───────────────────────────
    private Rigidbody _rb;

    // ── transform base ────────────────────────
    private Vector3    _startPos;
    private Quaternion _startRot;

    // ── per-frame shared values ───────────────
    private float _dt;
    private bool  _grounded;
    private bool  _running;
    private float _moveSqr;   // moveDir.sqrMagnitude

    // ── landing state ─────────────────────────
    private bool  _wasGrounded;
    private float _lastFallVelocity;
    private float _landingOffset;
    private float _landingTimer;
    private float _landingIntensity;
    private float _landingRotIntensity;
    private float _landingPitch;
    private float _landingRoll;

    // ──────────────────────────────────────────
    private static readonly AnimationCurve DefaultCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private void Start()
    {
        _rb       = movement.rb;
        _startPos = transform.localPosition;
        _startRot = transform.localRotation;

        _wasGrounded = movement.IsGrounded();

        if (Landing.recoveryCurve         == null || Landing.recoveryCurve.length         == 0)
            Landing.recoveryCurve         = DefaultCurve;
        if (Landing.rotationRecoveryCurve == null || Landing.rotationRecoveryCurve.length == 0)
            Landing.rotationRecoveryCurve = DefaultCurve;
    }

    private void Update()
    {
        if (disable || movement == null) return;

        // ── gather shared state once ──────────
        _dt       = Time.deltaTime;
        _grounded = movement.IsGrounded();   // single call per frame
        _running  = movement.isRunning;
        _moveSqr  = movement.moveDir.sqrMagnitude;  // no sqrt

        TrackFallVelocity();
        HandleLanding();
        ApplyTilt();
        ApplyBob();
        ApplyBreath();
        CommitTransform();
    }

    // ── Fall velocity tracker ──────────────────
    private void TrackFallVelocity()
    {
        if (!_grounded)
        {
            float vy = -_rb.linearVelocity.y;
            if (vy > 0f) _lastFallVelocity = vy;
        }
    }

    // ── Landing ───────────────────────────────
    private void HandleLanding()
    {
        if (!Landing.enabled)
        {
            _landingOffset = 0f;
            _landingPitch  = 0f;
            _landingRoll   = 0f;
            _wasGrounded   = _grounded;
            return;
        }

        if (!_wasGrounded && _grounded)
        {
            if (_lastFallVelocity > Landing.minimumVelocityThreshold)
            {
                float scale = Mathf.Min(
                    Landing.baseIntensity + _lastFallVelocity * Landing.velocityMultiplier,
                    Landing.maxIntensity
                );
                _landingIntensity    = scale;
                _landingRotIntensity = scale;
                _landingTimer        = 0f;
            }
            _lastFallVelocity = 0f;
        }

        if (_landingTimer < Landing.recoveryDuration)
        {
            _landingTimer += _dt;
            float t = Mathf.Clamp01(_landingTimer / Landing.recoveryDuration);

            _landingOffset = -_landingIntensity    * Landing.recoveryCurve.Evaluate(t);
            _landingPitch  =  Landing.pitchIntensity * _landingRotIntensity * Landing.rotationRecoveryCurve.Evaluate(t);
            _landingRoll   =  Landing.rollIntensity  * _landingRotIntensity * Landing.rotationRecoveryCurve.Evaluate(t);
        }
        else
        {
            _landingOffset       = 0f;
            _landingIntensity    = 0f;
            _landingRotIntensity = 0f;
            _landingPitch        = 0f;
            _landingRoll         = 0f;
        }

        _wasGrounded = _grounded;
    }

    // ── Bob + sprint shift ─────────────────────
    private void ApplyBob()
    {
        if (!Bob.enabled)
        {
            Bob.currentBobY        = 0f;
            Bob.currentBobX        = 0f;
            Bob.currentSprintShift = Vector3.zero;
            return;
        }

        float bobSmooth = _dt * Bob.smoothSpeed;

        if (_moveSqr > 0.0001f && _grounded)
        {
            float speedMult = _running ? Bob.sprintMultiplier : 1f;
            float amount    = Bob.walkBobAmountMultiplier * speedMult;

            Bob.timer += _dt * Bob.walkBobSpeed * speedMult;

            Bob.currentBobY = Mathf.Lerp(Bob.currentBobY, Mathf.Sin(Bob.timer)        * Bob.walkBobAmount  * amount, bobSmooth);
            Bob.currentBobX = Mathf.Lerp(Bob.currentBobX, Mathf.Cos(Bob.timer * 0.5f) * Bob.walkBobAmountX * amount, bobSmooth);
        }
        else
        {
            // Let timer keep its phase — only stop incrementing
            // so resuming movement doesn't pop back to sin(0)
            Bob.currentBobY = Mathf.Lerp(Bob.currentBobY, 0f, bobSmooth);
            Bob.currentBobX = Mathf.Lerp(Bob.currentBobX, 0f, bobSmooth);
        }

        Vector3 targetShift = (_running && _grounded) ? Bob.sprintPositionShift : Vector3.zero;
        Bob.currentSprintShift = Vector3.Lerp(Bob.currentSprintShift, targetShift, _dt * Bob.sprintShiftSmoothness);
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

        float breathSmooth = _dt * Breath.breathSmoothness;

        if (_moveSqr < 0.0001f && _grounded)
        {
            Breath.timer += _dt * Breath.breathSpeed;

            Breath.currentBreathY = Mathf.Lerp(Breath.currentBreathY, Mathf.Sin(Breath.timer)        * Breath.breathAmountY, breathSmooth);
            Breath.currentBreathX = Mathf.Lerp(Breath.currentBreathX, Mathf.Cos(Breath.timer * 0.6f) * Breath.breathAmountX, breathSmooth);
        }
        else
        {
            // Same as bob — keep timer phase, just fade out
            Breath.currentBreathY = Mathf.Lerp(Breath.currentBreathY, 0f, breathSmooth);
            Breath.currentBreathX = Mathf.Lerp(Breath.currentBreathX, 0f, breathSmooth);
        }
    }

    // ── Tilt ───────────────────────────────────
    private void ApplyTilt()
    {
        if (!Tilt.enabled) { Tilt.angle = 0f; return; }

        Tilt.angle = Mathf.Lerp(
            Tilt.angle,
            movement.inputDir.x * Tilt.tiltValue,
            Tilt.tiltSmoothness * _dt
        );
    }

    // ── Commit everything in one shot ──────────
    private void CommitTransform()
    {
        // Position — reuse _startPos to avoid a new Vector3 allocation
        transform.localPosition = new Vector3(
            _startPos.x + Bob.currentBobX    + Breath.currentBreathX + Bob.currentSprintShift.x,
            _startPos.y + Bob.currentBobY    + Breath.currentBreathY + _landingOffset           + Bob.currentSprintShift.y,
            _startPos.z + Bob.currentSprintShift.z
        );

        // Rotation — single Quaternion multiply, no intermediate euler allocation
        transform.localRotation = _startRot * Quaternion.Euler(_landingPitch, 0f, Tilt.angle + _landingRoll);
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

        _landingOffset       = 0f;
        _landingTimer        = Landing.recoveryDuration; // skip recovery
        _landingIntensity    = 0f;
        _landingRotIntensity = 0f;
        _landingPitch        = 0f;
        _landingRoll         = 0f;
        _lastFallVelocity    = 0f;

        Tilt.angle = 0f;

        transform.localPosition = _startPos;
        transform.localRotation = _startRot;
    }
}