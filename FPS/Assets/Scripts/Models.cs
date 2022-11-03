using System;
using System.Collections.Generic;
using UnityEngine;

public class Models
{
    #region - Player - 

    public enum PlayerStance {
        Standing,
        Crouching,
    }

    [Serializable]
    public class PlayerSettingsModel {
        [Header("ViewSettings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Settings")]
        public float MovementSmoothing;

        [Header("Movement - Sprinting")]
        public float SprintingForwardSpeed;
        public float SprintingStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed;
        public float WalkingStrafeSpeed;
        public float WalkingBackwardsSpeed;

        [Header("Jumping")]
        public float JumpHeight;
        public float JumpFalloff;

        [Header("SpeedModifiers")]
        public float SpeedModifier = 1;
        public float CrouchSpeedModifier;
    }

    [Serializable]
    public class CharacterStance {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion

    [Serializable]
    public class WeaponSettingsModel {
        [Header("Sway")]
        public float SwayAmount;
        public float SwaySmoothing;
        public bool SwayYInverted;
        public bool SwayXInverted;
    }

    #region - Weapons -

    #endregion
}
