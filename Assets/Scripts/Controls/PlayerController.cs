using System.Collections;
using System.Collections.Generic;
using static Models;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Inputs defaultInput;
    public Vector2 inputMovement;
    public Vector2 inputView;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    private void Awake() {
        defaultInput = new Inputs();
        defaultInput.Character.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();        
        defaultInput.Character.View.performed += e => inputView = e.ReadValue<Vector2>();        
        defaultInput.Character.Jump.performed += e => jump();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();
    }

    private void Update() 
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
    }

    private void CalculateView()
    {
        newCharacterRotation.y += playerSettings.viewXSensitivity * (playerSettings.ViewXInverted ? -inputView.x : inputView.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.viewYSensitivity * (playerSettings.ViewYInverted ? inputView.y : -inputView.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
        
    }

    private void CalculateMovement()
    {
        var verticalSpeed = playerSettings.walkingForwardSpeed * inputMovement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.walkingStrafeSpeed * inputMovement.x * Time.deltaTime;

        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin && jumpingForce.y < 0.1f)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }


        if (playerGravity < -1 && characterController.isGrounded)
        {
            playerGravity = -1;
        }

        if (jumpingForce.y > 0.1f)
        {
            playerGravity = 0;
        }
        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.jumpingFalloff);
    }

    private void jump()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        //Jump
        jumpingForce = Vector3.up * playerSettings.jumpingHeight;
    }


}
