using Newtonsoft.Json;
using System;
using UnityEngine;

namespace StageDressing.Models
{
    [Serializable]
    public class InstanceData
    {
        [JsonConverter(typeof(PoseConverter))]
        public Pose Pose { get; set; }
        public float Scale { get; set; }

        public bool GrabbableInMenu { get; set; }

        public bool GrabbableInGame { get; set; }

        #region Calibration Data

        public bool IsCalibrated { get; set; }

        public string CalibratedTrackerSerial { get; set; }

        [JsonConverter(typeof(PoseConverter))]
        public Pose CalibratedPose { get; set; }

        [JsonConverter(typeof(PoseConverter))]
        public Pose CalibratedTrackerPose { get; set; }

        #endregion

        #region Runtime Meta information

        [JsonIgnore]
        public GameObject Instance { get; set; }

        [JsonIgnore]
        public Bounds ExpandedBounds { get; set; }

        [JsonIgnore]
        public bool IsGrabbed { get; set; }

        #endregion
    }
}
