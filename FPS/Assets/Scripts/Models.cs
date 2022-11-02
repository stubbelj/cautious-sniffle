using System;
using System.Collections.Generic;
using UnityEngine;

public class Models
{
    #region - Player - 

    public enum PlayerStance {
        Standing,
        Crouching,
        Proning
    }

    [Serializable]
    public class PlayerSettingsModel {
        [Header("ViewSettings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement")]
        public float WalkingForwardSpeed;
        public float WalkingStrafeSpeed;
        public float WalkingBackwardsSpeed;

        [Header("Jump")]
        public float JumpHeight;
        public float JumpFalloff;
    }

    [Serializable]
    public class CharacterStance {
        public float CameraHeight;
        public CapsuleCollider StanceCollider;
    }

    #endregion - Player -
}
