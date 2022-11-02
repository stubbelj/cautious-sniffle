using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class Character_Controller : MonoBehaviour
{
    private CharacterController characterController;
    private DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;

    [Header("Gravity")]
    public float gravity;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpForce;
    public Vector3 jumpForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;

    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerProneStance;

    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenter;
    private Vector3 stanceCapsuleCenterVelocity;

    private float stanceCapsuleHeight;
    private float stanceCapsuleHeightVelocity;

    private void Awake() {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        
        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;
    }

    private void Update() {
        CalculateView();
        CalculateMovement();
        CalculateJump();
        CalculateStance();

    }

    private void CalculateView() {
        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ?  -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ?  input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);
        
        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);

    }

    private void CalculateMovement() {
        var frontSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var sideSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;

        var newMovementSpeed = new Vector3(sideSpeed, 0, frontSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin) {
            playerGravity -= gravity * Time.deltaTime;
        } 

        if (playerGravity < -0.1f  && characterController.isGrounded) {
            playerGravity = - 0.1f;
        }

        newMovementSpeed.y += playerGravity;

        newMovementSpeed += jumpForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump(){
        jumpForce = Vector3.SmoothDamp(jumpForce, Vector3.zero, ref jumpForceVelocity, playerSettings.JumpFalloff);
    }

    private void CalculateStance() {
        var currentStance = playerStandStance;
        if (playerStance == PlayerStance.Crouching) {
            currentStance = playerCrouchStance;
        } else if (playerStance == PlayerStance.Proning) {
            currentStance = playerProneStance;
        }
        cameraHeight = Mathf.SmoothDamp(cameraHolder.transform.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);

        cameraHolder.transform.localPosition = new Vector3(cameraHolder.transform.localPosition.x, cameraHeight, cameraHolder.transform.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void Jump() {
        if (!characterController.isGrounded) { return; }

        jumpForce = Vector3.up * playerSettings.JumpHeight;
        playerGravity = 0;
    }
}
