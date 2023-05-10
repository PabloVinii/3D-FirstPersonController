using System;
using System.Collections.Generic;
using UnityEngine;

public static class Models
{
    #region - Player -

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        [Range(0, 100)] public float viewXSensitivity;
        [Range(0, 100)] public float viewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement")]
        [Range(0, 100)] public float walkingForwardSpeed;
        [Range(0, 100)] public float walkingBackwardSpeed;
        [Range(0, 100)] public float walkingStrafeSpeed;

        [Header("Jumping")]
        [Range(0, 20)] public float jumpingHeight;
        [Range(0, 10)] public float jumpingFalloff;
    }

    #endregion
}
