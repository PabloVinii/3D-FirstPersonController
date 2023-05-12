using System.Collections;
using System.Collections.Generic;
using static Models;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Inputs defaultInput;
    private Vector2 inputMovement;
    private Vector2 inputView;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public LayerMask playerMask;
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;

    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;
    private float stanceCheckErrorMargin = 0.05f;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    private bool isSprinting;

    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    private void Awake() {
        defaultInput = new Inputs();
        defaultInput.Character.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();        
        defaultInput.Character.View.performed += e => inputView = e.ReadValue<Vector2>();        
        defaultInput.Character.Jump.performed += e => jump();
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Prone.performed += e => Prone();
        defaultInput.Character.Sprint.performed += e => IsSprinting();
        defaultInput.Character.SprintReleased.performed += e => StopSprinting();

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;
    }


    private void Update() 
    {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();
    }

    private void CalculateView()
    {
        // Inversão das entradas
        float invertX = playerSettings.ViewXInverted ? -1f : 1f;
        float invertY = playerSettings.ViewYInverted ? 1f : -1f;

        // Rotação do personagem apenas no eixo Y
        newCharacterRotation.y += playerSettings.viewXSensitivity * invertX * inputView.x * Time.fixedDeltaTime;
        transform.rotation = Quaternion.Euler(0f, newCharacterRotation.y, 0f);

        // Rotação da câmera no eixo X com limite
        newCameraRotation.x += playerSettings.viewYSensitivity * invertY * inputView.y * Time.fixedDeltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }


    private void CalculateMovement()
    {
        if (inputMovement.y <= 0.2f)
        {
            isSprinting = false;
        }
    
        var verticalSpeed = playerSettings.walkingForwardSpeed;
        var horizontalSpeed = playerSettings.walkingStrafeSpeed;

        if (isSprinting)
        {
            verticalSpeed = playerSettings.runningForwardSpeed;
            horizontalSpeed = playerSettings.runningStrafeSpeed;
        }

        if (!characterController.isGrounded)
        {
            playerSettings.speedEffector = playerSettings.FallingSpeedEffector;
        }
        else if (playerStance == PlayerStance.Crouch)
        {
            playerSettings.speedEffector = playerSettings.crouchSpeedEffector;
        }
        else if (playerStance == PlayerStance.Prone)
        {
            playerSettings.speedEffector = playerSettings.proneSpeedEffector;
        }
        else
        {
            playerSettings.speedEffector = 1;
        }
        //Effectors
        verticalSpeed *= playerSettings.speedEffector;
        horizontalSpeed *= playerSettings.speedEffector;

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * inputMovement.x * Time.deltaTime, 0, verticalSpeed * inputMovement.y * Time.deltaTime), 
            ref newMovementSpeedVelocity, characterController.isGrounded ? playerSettings.movementSmoothing : playerSettings.fallingSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravityAmount * Time.deltaTime;
        }


        if (playerGravity < -0.1f && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }


        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.jumpingFalloff);
    }

    private void CalculateStance()
    {
        Dictionary<PlayerStance, CharacterStance> stanceDictionary = new Dictionary<PlayerStance, CharacterStance>()
        {
            { PlayerStance.Stand, playerStandStance },
            { PlayerStance.Crouch, playerCrouchStance },
            { PlayerStance.Prone, playerProneStance }
        };

        CharacterStance targetStance = stanceDictionary[playerStance];

        // Suavização da altura da câmera
        cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, targetStance.cameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, cameraHeight, cameraHolder.localPosition.z);

        // Suavização da altura e centro do CharacterController
        SmoothDampCharacterControllerHeight(targetStance.stanceCollider.height, targetStance.stanceCollider.center);
    }

    private void SmoothDampCharacterControllerHeight(float targetHeight, Vector3 targetCenter)
    {
        characterController.height = Mathf.SmoothDamp(characterController.height, targetHeight, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, targetCenter, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }


    private void jump()
    {
        if (!characterController.isGrounded || playerStance == PlayerStance.Prone)
        {
            return;
        }

        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            return;
        }

        //Jump
        jumpingForce = Vector3.up * playerSettings.jumpingHeight;
        playerGravity = 0;
    }
  
    private void Crouch()
    {
        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.stanceCollider.height))
            {
                return;
            }
            playerStance = PlayerStance.Stand;
            return;
        }

        if (StanceCheck(playerCrouchStance.stanceCollider.height))
            {
                return;
            }
            
        playerStance = PlayerStance.Crouch;
    }

    private void Prone()
    {
        playerStance = PlayerStance.Prone;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);



        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    private void IsSprinting()
    {
        if (inputMovement.y <= 0.2f)
        {
            isSprinting = false;
            return;
        }

        isSprinting = !isSprinting;
    }

    private void StopSprinting()
    {
        if (playerSettings.sprintingHold)
        {
            isSprinting = false;        
        }

    }

}
