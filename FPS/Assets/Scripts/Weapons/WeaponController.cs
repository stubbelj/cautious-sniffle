using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class WeaponController : MonoBehaviour
{
    private PlayerCharacterController characterController;

    [Header("Settings")]
    public WeaponSettingsModel weaponSettings;

    bool isInitialized;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

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
        
        targetWeaponRotation.y += weaponSettings.SwayAmount * (weaponSettings.SwayXInverted ?  -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += weaponSettings.SwayAmount * (weaponSettings.SwayYInverted ?  characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;
        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -weaponSettings.SwayClampX, weaponSettings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -weaponSettings.SwayClampY, weaponSettings.SwayClampY);

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, weaponSettings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, weaponSettings.SwaySmoothing);
        
        transform.localRotation = Quaternion.Euler(newWeaponRotation);
    }
}
