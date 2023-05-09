using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool canMove { get; private set;} = true;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1,10)] private float lockSpeedX = 2.0f;
    [SerializeField, Range(1,10)] private float lockSpeedY = 2.0f;
    [SerializeField, Range(1,180)] private float upperLockLimit = 80.0f;
    [SerializeField, Range(1,180)] private float lowerLockLimit = 80.0f;

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
            HandleMovement();
            HandleMouseMovement();
            ApplyFinalMovement();
        }
    }
    private void HandleMovement() {
        currentInput = new Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward * currentInput.x) + transform.TransformDirection(Vector3.right * currentInput.y));
        moveDirection.y = moveDirectionY;
    }
    private void HandleMouseMovement() {
        rotationX -= Input.GetAxis("Mouse Y") * lockSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLockLimit,lowerLockLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lockSpeedX,0);

    }
    private void ApplyFinalMovement() {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }
}