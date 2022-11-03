using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerCharacterController characterController;

    [Header("References")]
    public Animator weaponAnimator;

    [Header("Settings")]
    public WeaponSettingsModel weaponSettings;

    bool isInitialized;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;
    private float fallingDelay;
    
    //private bool hasLanded;
    //private float hasLandedDelay = 0;

    private void Start() {
        newWeaponRotation = transform.localRotation.eulerAngles;
    }

    public void Initialize(PlayerCharacterController newCharacterController) {
        characterController = newCharacterController;
        isInitialized = true;
    }

    public void Update() {
        if (!isInitialized) {
            return;
        }
        CalculateWeaponRotation();
        SetWeaponAnimations();

    }

    public void TriggerJump() {
        isGroundedTrigger = false;
        weaponAnimator.SetTrigger("Jumping");
    }

    private void CalculateWeaponRotation() {
        
        targetWeaponRotation.y += weaponSettings.SwayAmount * (weaponSettings.SwayXInverted ?  -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime + 206;
        targetWeaponRotation.x += weaponSettings.SwayAmount * (weaponSettings.SwayYInverted ?  characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -weaponSettings.SwayClampX, weaponSettings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -weaponSettings.SwayClampY, weaponSettings.SwayClampY + 206);
        targetWeaponRotation.z = targetWeaponRotation.y - 206;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, weaponSettings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, weaponSettings.SwaySmoothing);

        targetWeaponMovementRotation.z = weaponSettings.MovementSwayX * (weaponSettings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = weaponSettings.MovementSwayY * (weaponSettings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, weaponSettings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, weaponSettings.MovementSwaySmoothing);
        
        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations() {
        if(isGroundedTrigger) {
            fallingDelay = 0;
        } else {
            fallingDelay += Time.deltaTime;
        }

        /*if(!hasLanded) {
            hasLandedDelay += Time.deltaTime;
        }
        
        if (hasLanded) {
            hasLandedDelay = 0;

        }*/

        if (characterController.isGrounded && !isGroundedTrigger && fallingDelay > 0.1f) {
            weaponAnimator.SetTrigger("Landing");
            isGroundedTrigger = true;
        }
        if (!characterController.isGrounded && isGroundedTrigger) {
            weaponAnimator.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnimator.SetBool("IsSprinting", characterController.isSprinting);
        weaponAnimator.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);
        
    }
}
