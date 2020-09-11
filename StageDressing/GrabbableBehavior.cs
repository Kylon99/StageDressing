using StageDressing.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace StageDressing
{
    public class GrabbableBehavior : MonoBehaviour
    {
        private class GrabbedInstanceInfo
        {
            public InstanceInfo grabbedInstance;
            public Pose grabbedStartPose;
            public Pose grabbedDevicePose;
            public InputDevice grabbedDevice;
        }

        private List<InstanceInfo> grabbableInstances = new List<InstanceInfo>();
        private GrabbedInstanceInfo leftGrabbed;
        private GrabbedInstanceInfo rightGrabbed;

        //private LineRenderer lines;
        //private void Start()
        //{
        //    this.lines = gameObject.AddComponent<LineRenderer>();
        //    this.lines.startColor = Color.white;
        //    this.lines.endColor = Color.white;
        //    this.lines.startWidth = 0.05f;
        //    this.lines.endWidth = 0.05f;
        //}


        public void LoadGrabbableInstances(bool gameScene = false)
        {
            var scenes = gameScene
                ? StageManager.instance.Configuration.Scenes.Where(s => s.ShowInGame)
                : StageManager.instance.Configuration.Scenes.Where(s => s.ShowInMenu);

            var allInstances = scenes.SelectMany(s => s.Prefabs).Where(p => p.Enabled).SelectMany(p => p.Instances);

            var grabbableInstancesEnum = gameScene ? allInstances.Where(i => i.GrabbableInGame) : allInstances.Where(i => i.GrabbableInMenu);
            this.grabbableInstances = grabbableInstancesEnum.ToList();
        }

        /// <summary>
        /// Calculates an expanded bounds for the given game object
        /// </summary>
        /// <param name="gameObject">The game object to calculate from</param>
        /// <returns>The bounds of the game object expanded by a small amount</returns>
        public static Bounds CalculateExpandedBounds(GameObject gameObject)
        {
            const float expandBounds = 0.05f;

            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (!renderers.Any()) return new Bounds(gameObject.transform.position, new Vector3(0.25f, 0.25f, 0.25f));

            Bounds result = renderers[0].bounds;
            foreach (Renderer r in renderers.Skip(1))
            {
                result.Encapsulate(r.bounds);
            }

            result.Expand(expandBounds);
            return result;
        }

        /// <summary>
        /// Attempts to choose the closest instance to the given position if it is within the instance
        /// bounds.
        /// </summary>
        private GrabbedInstanceInfo GrabInstance(InputDevice device)
        {
            // Get the position of the device first
            bool getPositionSuccess = device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePosition);
            if (!getPositionSuccess) return null;

            // Check if its position is within the bounds of any instance
            var containedInstances = this.grabbableInstances.Where(i => i.ExpandedBounds.Contains(devicePosition)).ToList();
            if (!containedInstances.Any()) return null;

            // Save the grabbed instance
            InstanceInfo grabbedInstance = (containedInstances.Count == 1)
                ? containedInstances[0]
                : containedInstances.OrderByDescending(i => (i.Instance.transform.position - devicePosition).sqrMagnitude).First();

            // We also need the grabbed device's rotation now
            bool getRotationSuccess = device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRotation);
            if (!getRotationSuccess) return null;

            StageDressing.Logger.Info($"Grabbed! {grabbedInstance.Instance.name}");

            // Release if already grabbed
            if (this.leftGrabbed != null && leftGrabbed.grabbedInstance == grabbedInstance)
            {
                ReleaseInstance(ref this.leftGrabbed);
            }

            if (this.rightGrabbed != null && rightGrabbed.grabbedInstance == grabbedInstance)
            {
                ReleaseInstance(ref this.rightGrabbed);
            }

            // Set all the initial positions of the grabbing
            grabbedInstance.IsGrabbed = true;
            return new GrabbedInstanceInfo
            {
                grabbedInstance = grabbedInstance,
                grabbedDevice = device,
                grabbedStartPose = new Pose(grabbedInstance.Instance.transform.position, grabbedInstance.Instance.transform.rotation),
                grabbedDevicePose = new Pose(devicePosition, deviceRotation),
            };
        }

        /// <summary>
        /// Release the instance that was previous grabbed
        /// </summary>
        private void ReleaseInstance(ref GrabbedInstanceInfo grabbedInstanceInfo)
        {
            grabbedInstanceInfo.grabbedInstance.IsGrabbed = false;
            grabbedInstanceInfo.grabbedInstance.ExpandedBounds = CalculateExpandedBounds(grabbedInstanceInfo.grabbedInstance.Instance);

            // If object is being tracked update new calibration pose at the current location
            if (grabbedInstanceInfo.grabbedInstance.IsCalibrated)
            {
                TrackedObjectBehavior.Calibrate(grabbedInstanceInfo.grabbedInstance);
            }

            StageDressing.Logger.Info($"Released! {grabbedInstanceInfo.grabbedInstance.Instance.name}");

            grabbedInstanceInfo = null;
        }

        //private void DrawBounds(Bounds b)
        //{

        //    var vectorArray = new Vector3[]
        //    {
        //        new Vector3(b.min.x, b.min.y, b.min.z),
        //        new Vector3(b.max.x, b.min.y, b.min.z),
        //        new Vector3(b.max.x, b.min.y, b.max.z),
        //        new Vector3(b.min.x, b.min.y, b.max.z),
        //        new Vector3(b.min.x, b.max.y, b.min.z),
        //        new Vector3(b.max.x, b.max.y, b.min.z),
        //        new Vector3(b.max.x, b.max.y, b.max.z),
        //        new Vector3(b.min.x, b.max.y, b.max.z)
        //    };

        //    this.lines.positionCount = 8;
        //    this.lines.SetPositions(vectorArray);
        //}

        private void Update()
        {
            // If there is nothing to grab anywhere, do nothing
            if (!this.grabbableInstances.Any()) return;

            // Check for trigger clicks first as it's debounced and will override simply having the trigger down
            if (PersistentSingleton<InputManager>.instance.GetRightTriggerClicked())
            {
                this.rightGrabbed = this.GrabInstance(PersistentSingleton<InputManager>.instance.RightController);
                return;
            }

            if (PersistentSingleton<InputManager>.instance.GetLeftTriggerClicked())
            {
                this.leftGrabbed = this.GrabInstance(PersistentSingleton<InputManager>.instance.LeftController);
                return;
            }

            // If there is no grabbed instance or grabbed device then we are done
            if (this.leftGrabbed == null && this.rightGrabbed == null) return;

            // Check for trigger being held down on the device
            if (this.rightGrabbed != null)
            {
                if (PersistentSingleton<InputManager>.instance.GetRightTriggerDown())
                {
                    // Trigger held so track instance position with the device
                    Pose? grabbedPose = TrackedDeviceManager.GetDevicePose(rightGrabbed.grabbedDevice);
                    if (grabbedPose == null) return;
                    this.rightGrabbed.grabbedInstance.Pose = TrackedDeviceManager.GetTrackedObjectPose(rightGrabbed.grabbedStartPose, rightGrabbed.grabbedDevicePose, grabbedPose.Value);
                    this.rightGrabbed.grabbedInstance.Instance.transform.position = this.rightGrabbed.grabbedInstance.Pose.position;
                    this.rightGrabbed.grabbedInstance.Instance.transform.rotation = this.rightGrabbed.grabbedInstance.Pose.rotation;

                    this.rightGrabbed.grabbedInstance.ExpandedBounds = CalculateExpandedBounds(this.rightGrabbed.grabbedInstance.Instance);
                    //DrawBounds(this.rightGrabbed.grabbedInstance.ExpandedBounds);
                }
                else
                {
                    ReleaseInstance(ref this.rightGrabbed);
                }
            }

            if (this.leftGrabbed != null)
            {
                if (PersistentSingleton<InputManager>.instance.GetLeftTriggerDown())
                {
                    // Trigger held so track instance position with the device
                    Pose? grabbedPose = TrackedDeviceManager.GetDevicePose(leftGrabbed.grabbedDevice);
                    if (grabbedPose == null) return;
                    this.leftGrabbed.grabbedInstance.Pose = TrackedDeviceManager.GetTrackedObjectPose(leftGrabbed.grabbedStartPose, leftGrabbed.grabbedDevicePose, grabbedPose.Value);
                    this.leftGrabbed.grabbedInstance.Instance.transform.position = this.leftGrabbed.grabbedInstance.Pose.position;
                    this.leftGrabbed.grabbedInstance.Instance.transform.rotation = this.leftGrabbed.grabbedInstance.Pose.rotation;

                    this.leftGrabbed.grabbedInstance.ExpandedBounds = CalculateExpandedBounds(this.leftGrabbed.grabbedInstance.Instance);
                    //DrawBounds(this.leftGrabbed.grabbedInstance.ExpandedBounds);
                }
                else
                {
                    ReleaseInstance(ref this.leftGrabbed);
                }
            }
        }
    }
}
