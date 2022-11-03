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
        
        newWeaponRotation.y += weaponSettings.SwayAmount * (weaponSettings.SwayXInverted ?  -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        newWeaponRotation.x += weaponSettings.SwayAmount * (weaponSettings.SwayYInverted ?  characterController.input_View.y : -characterController.input_View.y) * Time.deltaTime;
        //newWeaponRotation.x = Mathf.Clamp(newWeaponRotation.x, viewClampYMin, viewClampYMax);
        
        transform.localRotation = Quaternion.Euler(newWeaponRotation);
    }
}
