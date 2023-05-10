using System.Collections;
using System.Collections.Generic;
using static Models;
using UnityEngine;

public class PlayerControllers : MonoBehaviour
{
    private Inputs defaultInput;
    public Vector2 inputMovement;
    public Vector2 inputView;

    private Vector3 newCameraRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;


    private void Awake() {
        defaultInput = new Inputs();
        defaultInput.Character.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();        
        defaultInput.Character.View.performed += e => inputView = e.ReadValue<Vector2>();        

        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
    }

    private void Update() 
    {
        CalculateView();
        CalculateMovement();
    }

    private void CalculateView()
    {
        newCameraRotation.x += playerSettings.viewYSensitivity * inputView.y * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void CalculateMovement()
    {

    }

}
