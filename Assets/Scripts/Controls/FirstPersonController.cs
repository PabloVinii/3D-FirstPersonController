using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool canMove { get; private set;} = true;
    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    private bool shouldJump => Input.GetKeyDown(jumKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Look Parameters")]
    [SerializeField, Range(1,10)] private float lockSpeedX = 2.0f;
    [SerializeField, Range(1,10)] private float lockSpeedY = 2.0f;
    [SerializeField, Range(1,180)] private float upperLockLimit = 80.0f;
    [SerializeField, Range(1,180)] private float lowerLockLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField, Range(3f, 20f)] private float jumpForce = 8.0f;
    [SerializeField, Range(1f, 100f)] private float gravity = 23.0f;
    [SerializeField, Range(0f, 0.5f)] private float coyoteTime = 0.2f;
    private float coyoteCounter = 0f;

    [Header("Crouch Parameters")]
    [SerializeField, Range(0f, 3f)] private float crounchHeight = 0.5f;
    [SerializeField, Range(0f, 5f)] private float standingHeight = 2f;
    [SerializeField, Range(0f, 5f)] private float timeToCrounch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector3 currentInput;
    private float rotationX;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
            {
                HandleJump();
            }
            if (canCrouch)
            {
                HandleCrouch();
            }
            ApplyFinalMovements();
        }
    }
    private void HandleMovementInput() {
        currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), 
            (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, walkSpeed);
        moveDirection = (transform.TransformDirection(Vector3.forward * currentInput.x) + transform.TransformDirection(Vector3.right * currentInput.y));
        moveDirection.y = moveDirectionY;
    }

    private void HandleJump()
    {
        if (shouldJump && coyoteCounter >= 0)
        {
            moveDirection.y = jumpForce;
            coyoteCounter = -1f;
        }
        else if (characterController.isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void HandleCrouch()
    {
        if (shouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }
    
    private void HandleMouseLook() {
        rotationX -= Input.GetAxis("Mouse Y") * lockSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLockLimit,lowerLockLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lockSpeedX,0);

    }
    private void ApplyFinalMovements() {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crounchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrounch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrounch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrounch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}