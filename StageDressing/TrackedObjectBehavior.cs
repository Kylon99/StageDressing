using Newtonsoft.Json;
using StageDressing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StageDressing
{
    public class TrackedObjectBehavior : MonoBehaviour
    {
        private List<InstanceData> trackedInstances = new List<InstanceData>();

        public void LoadTrackedInstances(bool gameScene = false)
        {
            var scenes = gameScene
                ? StageManager.instance.Configuration.Scenes.Where(s => s.ShowInGame)
                : StageManager.instance.Configuration.Scenes.Where(s => s.ShowInMenu);

            this.trackedInstances = scenes.SelectMany(s => s.Prefabs).Where(p => p.Enabled)
                .SelectMany(p => p.Instances).Where(i => !String.IsNullOrEmpty(i.CalibratedTrackerSerial)).ToList();
        }

        public static void Calibrate(InstanceData instance)
        {
            StageDressing.Logger.Info($"instance: {JsonConvert.SerializeObject(instance, Formatting.Indented)}");

            // Attempt to get the current tracker position.  Calibration fails if this fails
            var trackerPose = TrackedDeviceManager.instance.GetPoseFromSerial(instance.CalibratedTrackerSerial);
            if (trackerPose == null)
            {
                StageDressing.Logger.Info($"tracker pose was null!");
                instance.IsCalibrated = false;
                return;
            }

            // Save the current Pose and Tracker Pose to calculate difference from in the future
            StageDressing.Logger.Info($"Calibrated: {instance.Instance.name}");

            instance.IsCalibrated = true;
            instance.CalibratedPose = instance.Pose;
            instance.CalibratedTrackerPose = trackerPose.Value;
        }

        public static void UnCalibrate(InstanceData instance)
        {
            instance.IsCalibrated = false;
            instance.CalibratedPose = new Pose(Vector3.one, Quaternion.identity);
            instance.CalibratedTrackerPose = new Pose(Vector3.one, Quaternion.identity);
        }

        #region Unity Methods

        private void Update()
        {
            // Track all instances that have been calibrated and hasn't been grabbed
            foreach (var instance in trackedInstances.Where(i => i.IsCalibrated && !i.IsGrabbed))
            {
                var newInstancePose = TrackedDeviceManager.instance.GetTrackedObjectPoseBySerial(instance.CalibratedTrackerSerial, instance.CalibratedPose, instance.CalibratedTrackerPose);
                if (newInstancePose == null) continue;

                instance.Pose = newInstancePose.Value;
                instance.Instance.transform.position = newInstancePose.Value.position;
                instance.Instance.transform.rotation = newInstancePose.Value.rotation;

                if (instance.GrabbableInGame || instance.GrabbableInMenu)
                {
                    // Also calculate new grabbable bounds in case it is grabbed
                    instance.ExpandedBounds = GrabbableBehavior.CalculateExpandedBounds(instance.Instance);
                }
            }
        }

        #endregion
    }

}
