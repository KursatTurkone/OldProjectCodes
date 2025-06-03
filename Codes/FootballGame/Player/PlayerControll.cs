using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour
{
    [SerializeField] private PlayerControllerBase PlayerControllerBase;
    [SerializeField] private PlayerAnimationController controller;
    [SerializeField] public int MoveForce;
    [HideInInspector] public FixedJoystick joystick;
    private Rigidbody rb;
    private bool isActive = true;

    private bool ExtraVelocityActive;

    //Get set
    public bool IsActive
    {
        get => isActive;
        set { isActive = value; }
    }

    void Awake()
    {
        GameObject.FindGameObjectWithTag("GameController").TryGetComponent(out joystick);
        TryGetComponent(out rb);
    }

    void FixedUpdate()
    {
        if (!PlayerControllerBase.IsActiveCharacter)
            return;
        if (!joystick)
            return;
        if (!isActive)
            return;
        if (joystick.Direction.sqrMagnitude > 0f)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForward * joystick.Vertical + cameraRight * joystick.Horizontal).normalized;

            if (moveDirection.sqrMagnitude > 0f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }

            var dist = Vector3.Distance(GameController.Instance.BallInteract.transform.position, transform.position);
            float adjustedMoveForce = dist < 5f ? MoveForce * 0.8f : MoveForce;

            rb.linearVelocity = moveDirection * adjustedMoveForce * joystick.Direction.sqrMagnitude;
        }
        else
        {
            if (ExtraVelocityActive)
                return;
            StopMovement();
        }
    }

    public void StopWalking()
    {
        isActive = false;
        StopMovement();
        controller.GetLocalMovement();
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void StartWalking()
    {
        isActive = true;
    }

    public void ApplyExtraVelocity(Vector3 direction, float multiplier)
    {
        /*if (!PlayerControllerBase.IsActiveCharacter)
            return;*/
        Vector3 targetVelocity = rb.linearVelocity + direction.normalized * multiplier * 10;
        float maxSpeed = 5f;
        if (targetVelocity.magnitude > maxSpeed)
        {
            targetVelocity = targetVelocity.normalized * maxSpeed;
        }

        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, 600f * Time.deltaTime);
    }

    public void ExtraVelocityActiveState(bool state)
    {
        ExtraVelocityActive = state;
    }
}