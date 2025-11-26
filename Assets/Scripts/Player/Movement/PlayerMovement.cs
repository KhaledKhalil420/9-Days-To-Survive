// PlayerMovement.cs
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlayerMovement : PlayerMovementVariables
{
    public void LateUpdate()
    {
        Inputs();
        // HandleAnimations();
    }

    public void FixedUpdate()
    {
        ApplyCounterMovement();
        MovePlayer();
        // HandleFootsteps();
        isJumpingThisFrame = false;
    }

    #region Movement Mechanics

    private void Inputs()
    {
        HandleRunning();
        HandleDirection();
        HandleJumping();
        HandleCrouching();
        HandleSliding();
    }

    private void HandleRunning()
    {
        isRunning = Input.GetButton("Running");
    }

    private void HandleDirection()
    {
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), inputDir.y, Input.GetAxisRaw("Vertical"));
        moveDir = inputDir.x * transform.right + inputDir.z * transform.forward;
    }

    private void HandleJumping()
    {
        if (!enableJumping) return;

        if (Input.GetButton("Jump") && IsGrounded())
        {
            isJumpingThisFrame = true;

            isCrouching = false;
            playerCollider.height = 2f;
            playerCollider.center = Vector3.zero;

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                jumpForce * Time.fixedDeltaTime,
                rb.linearVelocity.z
            );
        }
    }

    private void HandleCrouching()
    {
        if (!enableCrouching) return;

        if (Input.GetKeyDown(Keybinds.Key("Crouch")) && !isSliding && IsGrounded())
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                playerCollider.height = 1f;
                playerCollider.center = new Vector3(0, -0.5f, 0);
                AnimateCrouch();
            }
            else
            {
                if (IsHeaded())
                {
                    isCrouching = true;
                    StartCoroutine(TryToStopCrouch());
                }
                else
                {
                    playerCollider.height = 2f;
                    playerCollider.center = Vector3.zero;
                    isCrouching = false;
                    AnimateCrouch();
                }
            }
        }
    }

    private void AnimateCrouch()
    {
        playerLook._mainCamera.transform.DOLocalMove(new Vector3(0, -0.15f, 0), 0.1f).OnComplete(() =>
        {
            playerLook._mainCamera.transform.DOLocalMove(Vector3.zero, 0.15f);
        });
    }

    private Tween slideTween;

    private void HandleSliding()
    {
        if (!enableSliding) return;

        if (Input.GetKeyDown(Keybinds.Key("Crouch")) && isRunning && IsGrounded())
        {
            if (isSliding) return;

            isSliding = true;
            playerCollider.height = 1f;
            playerCollider.center = new Vector3(0, -0.5f, 0);
            playerLook._mainCamera.transform.DOLocalRotate(new Vector3(0, 0, 4), 0.25f).SetEase(Ease.OutSine);

            slideTween = DOTween.To(() => currentSlideForce, x => currentSlideForce = x, 0f, slideTime)
                .SetAutoKill(true)
                .OnUpdate(() =>
                {
                    if (IsGrounded())
                        rb.AddForce(currentSlideForce * Time.deltaTime * moveDir.normalized, ForceMode.Impulse);
                })
                .OnComplete(StopSlide)
                .OnKill(StopSlide);
        }

        if (Input.GetKeyUp(Keybinds.Key("Crouch")) && isSliding)
        {
            slideTween.Kill();
        }

        if (!isSliding)
        {
            currentSlideForce = slideForce;
        }
    }

    private void StopSlide()
    {
        isSliding = false;
        StartCoroutine(TryToStopCrouch());
        playerLook._mainCamera.transform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.InSine);
    }

    private IEnumerator TryToStopCrouch()
    {
        if (!IsHeaded())
            yield return null;

        while (isCrouching)
        {
            if (!IsHeaded())
            {
                playerCollider.height = 2f;
                playerCollider.center = Vector3.zero;
                isCrouching = false;
                AnimateCrouch();
            }

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion



    #region Sounds

    // private void HandleFootsteps()
    // {
    //     if (!IsGrounded() || moveDir.magnitude < 0.1f || isSliding)
    //         return;

    //     footstepTimer -= Time.fixedDeltaTime;

    //     if (footstepTimer <= 0f)
    //     {
    //         footstepTimer = FootstepsCooldown();
    //         SurfaceType surfaceType = SurfaceType.Grass; // default

    //         if (Physics.Raycast(feet.position, -feet.transform.up, out RaycastHit hit, 5f))
    //         {
    //             if (hit.transform.TryGetComponent(out SoundMap soundMap))
    //             {
    //                 surfaceType = soundMap.surfaceType;
    //             }
    //         }

    //         AudioClip[] clips = footstepSounds.GetClips(surfaceType);
    //         footstepsSource.pitch = Random.Range(0.95f, 1.05f);
    //         footstepsSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    //     }
    // }
    #endregion

    // #region Animations

    // private void HandleAnimations()
    // {
    //     animator.SetBool("IsGrounded", IsGrounded());
    //     animator.SetBool("IsWalking", state == PlayerMovementState.Walking);
    //     animator.SetBool("IsRunning", state == PlayerMovementState.Running);
    //     animator.SetBool("IsCrouching", state == PlayerMovementState.Crouching);
    //     animator.SetBool("IsCrouchMoving", state == PlayerMovementState.CrouchMoving);
    //     animator.SetBool("IsSliding",  state == PlayerMovementState.Sliding);
        
    //  }

    // #endregion
}
