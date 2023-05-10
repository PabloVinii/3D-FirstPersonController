using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Models
{
    #region - Player -

    [System.Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float viewXSensitivity;
        public float viewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;
    }

    #endregion
}
