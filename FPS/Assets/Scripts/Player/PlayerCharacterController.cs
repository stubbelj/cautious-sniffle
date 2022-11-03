using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class PlayerCharacterController : MonoBehaviour
{
    private CharacterController characterController;
    private DefaultInput defaultInput;
    private Vector2 input_Movement;
    [HideInInspector]
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    [Header("References")]
    public Transform cameraHolder;
    public Transform feetTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70;
    public float viewClampYMax = 80;
    public LayerMask playerMask;

    [Header("Gravity")]
    public float gravity;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpForce;
    public Vector3 jumpForceVelocity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public bool waitingToStand = false;
    public float playerStanceSmoothing;

    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;

    private float stanceCheckErrorMargin = 0.05f;
    private float cameraHeight;
    private float cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    public bool isSprinting;
    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;

    [Header("Weapon")]
    public WeaponController currentWeapon;

    private void Awake() {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Crouch.canceled += e => StartCoroutine(Stand());
        defaultInput.Character.Sprint.performed += e => ToggleSprint();
        defaultInput.Character.Sprint.canceled += e => ToggleSprint();
        
        defaultInput.Enable();

        newCameraRotation = cameraHolder.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();

        cameraHeight = cameraHolder.localPosition.y;

        if (currentWeapon) {
            currentWeapon.Initialize(this);
        }
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
        var forwardSpeed = playerSettings.WalkingForwardSpeed;
        var strafeSpeed = playerSettings.WalkingStrafeSpeed;

        if (isSprinting) {
            forwardSpeed = playerSettings.SprintingForwardSpeed;
            strafeSpeed = playerSettings.SprintingStrafeSpeed;
        }

        if (playerStance == PlayerStance.Crouching) {
            forwardSpeed *= playerSettings.CrouchSpeedModifier;
            strafeSpeed *= playerSettings.CrouchSpeedModifier;
        } else {
            forwardSpeed *= playerSettings.SpeedModifier;
            strafeSpeed *= playerSettings.SpeedModifier;
        }

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(strafeSpeed * input_Movement.x * Time.deltaTime, 0, forwardSpeed * input_Movement.y * Time.deltaTime), ref newMovementSpeedVelocity, playerSettings.MovementSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin) {
            playerGravity -= gravity * Time.deltaTime;
        } 

        if (playerGravity < -0.1f  && characterController.isGrounded) {
            playerGravity = - 0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpForce * Time.deltaTime;

        characterController.Move(movementSpeed);
    }

    private void CalculateJump(){
        jumpForce = Vector3.SmoothDamp(jumpForce, Vector3.zero, ref jumpForceVelocity, playerSettings.JumpFalloff);
    }

    private void CalculateStance() {
        var currentStance = playerStandStance;
        if (playerStance == PlayerStance.Crouching) {
            currentStance = playerCrouchStance;
        }
        cameraHeight = Mathf.SmoothDamp(cameraHolder.transform.localPosition.y, currentStance.CameraHeight, ref cameraHeightVelocity, playerStanceSmoothing);

        cameraHolder.transform.localPosition = new Vector3(cameraHolder.transform.localPosition.x, cameraHeight, cameraHolder.transform.localPosition.z);

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void Jump() {
        if (!characterController.isGrounded) { return; }

        if (playerStance == PlayerStance.Crouching) {
            playerStance = PlayerStance.Standing;
            return; 
        }

        if (StanceCheck(playerStandStance.StanceCollider.height)) {
            return;
        }

        jumpForce = Vector3.up * playerSettings.JumpHeight;
        playerGravity = 0;
    }

    private IEnumerator Stand() {
        //stand up once player has space
        //if player releases crouch while in an unstandable position, Stand() is triggered. If they press crouch again, it is cancelled.
        waitingToStand = true;
        while (waitingToStand && StanceCheck(playerStandStance.StanceCollider.height)) { yield return new WaitForSeconds(0.1f); }
        if (waitingToStand) {
            playerStance = PlayerStance.Standing;
            waitingToStand = false;
        }
    }

    private void Crouch() {
        waitingToStand = false;
        playerStance = PlayerStance.Crouching;
    }

    private bool StanceCheck(float stanceCheckHeight) {
        Vector3 start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        Vector3 end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);
        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    private void ToggleSprint() {
        /*if (input_Movement.y <= 0.2f) {
            isSprinting = false;
            return;
        }*/
        isSprinting = !isSprinting;
    }
}
