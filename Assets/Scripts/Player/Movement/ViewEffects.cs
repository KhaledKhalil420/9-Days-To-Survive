using System;
using UnityEngine;

[Serializable]
public struct TiltSettings
{
    [Header("Tilt Parameters")]
    [Tooltip("How much the camera tilts while moving sideways")]
    public float tiltValue;
    [Tooltip("How smoothly the camera tilts")]
    public float tiltSmoothness;

    [HideInInspector] public float angle;
}

[Serializable]
public struct BobSettings
{
    [Header("Bob Parameters")]
    [Tooltip("Speed of bobbing while walking")]
    public float walkBobSpeed;
    [Tooltip("Amount of vertical bob while walking")]
    public float walkBobAmount;
    [Tooltip("How much faster/stronger bobbing is while sprinting")]
    public float walkBobAmountMultiplier;
    public float sprintMultiplier;

    [Header("Bob Smoothing")]
    [Tooltip("How quickly to smooth between bob positions")]
    public float smoothSpeed;

    [HideInInspector] public float timer;
    [HideInInspector] public float currentBob;
}

[Serializable]
public struct LandingSettings
{
    [Header("Landing Parameters")]
    public float baseIntensity;
    public float velocityMultiplier;
    public float maxIntensity;
    public float recoverySpeed;
    public float minimumVelocityThreshold;

    public float fadeInSpeed;
}

public class ViewEffects : MonoBehaviour
{
    [Header("Effect Settings")]
    public TiltSettings Tilt;

    public BobSettings Bob;

    public LandingSettings Landing;

    [Header("References")]
    public PlayerMovementVariables movement;
    internal static bool disable = false;

    private Vector3 _startPos;
    private bool _wasGrounded;
    private float _landingTimer;
    private float _landingOffset;
    private float _lastFallVelocity;

    private void Start()
    {
        _startPos = transform.localPosition;
        _wasGrounded = movement.IsGrounded();
    }

    private void Update()
    {
        if (disable || movement == null) return;

        ViewTilt();
        ViewBob();
        HandleLanding();
        TrackFallVelocity();
    }

    private void TrackFallVelocity()
    {
        if (!movement.IsGrounded())
        {
            _lastFallVelocity = -movement.rb.linearVelocity.y;
            
            if (_landingOffset != 0f)
            {
                _landingOffset = 0f;
                _landingTimer = 0f;
            }
        }
    }

    private void ViewBob()
    {
        if (movement.moveDir.magnitude > 0f && movement.IsGrounded())
        {
            float speedMultiplier = movement.isRunning ? Bob.sprintMultiplier : 1f;
            float amountMultiplier = Bob.walkBobAmountMultiplier; // <— NEW
    
            Bob.timer += Time.deltaTime * Bob.walkBobSpeed * speedMultiplier;
    
            float targetBob =
                Mathf.Sin(Bob.timer) *
                Bob.walkBobAmount *
                amountMultiplier *
                speedMultiplier;
    
            Bob.currentBob = Mathf.Lerp(
                Bob.currentBob,
                targetBob,
                Time.deltaTime * Bob.smoothSpeed
            );
        }
        else
        {
            Bob.timer = 0f;
            Bob.currentBob = Mathf.Lerp(
                Bob.currentBob,
                0f,
                Time.deltaTime * Bob.smoothSpeed
            );
        }
    
        transform.localPosition = new Vector3(
            _startPos.x,
            _startPos.y + Bob.currentBob + _landingOffset,
            _startPos.z
        );
    }

    private float _targetLandingOffset;

    private void HandleLanding()
    {
        if (!_wasGrounded && movement.IsGrounded())
        {
            if (_lastFallVelocity > Landing.minimumVelocityThreshold)
            {
                float dynamicIntensity = Landing.baseIntensity + 
                    (_lastFallVelocity * Landing.velocityMultiplier);
                
                dynamicIntensity = Mathf.Min(dynamicIntensity, Landing.maxIntensity);
                
                _targetLandingOffset = -dynamicIntensity;
                _landingTimer = 0f;
            }
            
            _lastFallVelocity = 0f;
        }

        if (_targetLandingOffset != 0f)
        {
            _landingOffset = Mathf.Lerp(_landingOffset, _targetLandingOffset, Time.deltaTime * Landing.fadeInSpeed);
            _landingTimer += Time.deltaTime * Landing.recoverySpeed;
        
            _targetLandingOffset = Mathf.Lerp(_targetLandingOffset, 0f, _landingTimer);

            if (_landingTimer >= 1f)
            {
                _targetLandingOffset = 0f;
                _landingOffset = 0f;
            }
        }

        _wasGrounded = movement.IsGrounded();
    }

    private void ViewTilt()
    {
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

    public void ResetView()
    {
        Bob.timer = 0f;
        Bob.currentBob = 0f;
        _landingOffset = 0f;
        _landingTimer = 0f;
        _lastFallVelocity = 0f;
        transform.localPosition = _startPos;
        Tilt.angle = 0f;
    }
}