using System;
using System.Collections.Generic;
using UnityEngine;

public static class Models
{
    #region - Player -

    public enum PlayerStance
    {
        Stand,
        Crouch,
        Prone,
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        [Range(0, 100)] public float viewXSensitivity;
        [Range(0, 100)] public float viewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Walking")]
        [Range(0, 50)] public float walkingForwardSpeed;
        [Range(0, 50)] public float walkingBackwardSpeed;
        [Range(0, 50)] public float walkingStrafeSpeed;

        [Header("Movement - Running")]
        [Range(0, 100)] public float runningForwardSpeed;
        [Range(0, 100)] public float runningStrafeSpeed;

        [Header("Jumping")]
        [Range(0, 20)] public float jumpingHeight;
        [Range(0, 10)] public float jumpingFalloff;
    }

    [Serializable]
    public class CharacterStance
    {
        public float cameraHeight;
        public CapsuleCollider stanceCollider;
    }
    #endregion
}
