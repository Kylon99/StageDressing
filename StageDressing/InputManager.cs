using UnityEngine.XR;

namespace StageDressing
{
    /// <summary>
    /// This class manages the input system, most importantly helping to gate button presses
    /// on the controller, basically debouncing buttons.
    /// </summary>
    public class InputManager : PersistentSingleton<InputManager>
    {
        public InputDevice LeftController { get; private set; }
        public InputDevice RightController { get; private set; }
        private bool leftTriggerCanClick;
        private bool leftTriggerDown;
        private bool rightTriggerCanClick;
        private bool rightTriggerDown;
        private bool isPolling;


        /// <summary>
        /// Returns whether the Left Trigger is currently held down
        /// </summary>
        /// <returns>True if it his held, false if not</returns>
        public bool GetLeftTriggerDown()
        {
            return leftTriggerDown;
        }

        /// <summary>
        /// Returns whether the left trigger had been pressed previously.  If the
        /// trigger has not been released fully then it cannot be clicked again.
        /// </summary>
        /// <returns>Returns true if it was clicked, false if not</returns>
        public bool GetLeftTriggerClicked()
        {
            bool returnValue = false;
            if (leftTriggerCanClick && leftTriggerDown)
            {
                // Trigger was allowed to click and is now depressed enough
                returnValue = true;
                leftTriggerCanClick = false;  // Prevent further clicks
            }
            return returnValue;
        }

        /// <summary>
        /// Returns whether the Right Trigger is currently held down
        /// </summary>
        /// <returns>True if it his held, false if not</returns>
        public bool GetRightTriggerDown()
        {
            return rightTriggerDown;
        }

        /// <summary>
        /// Returns whether the right trigger had been pressed previously.  If the
        /// trigger has not been released fully then it cannot be clicked again.
        /// </summary>
        /// <returns>Returns true if it was clicked, false if not</returns>
        public bool GetRightTriggerClicked()
        {
            bool returnValue = false;
            if (rightTriggerCanClick && rightTriggerDown)
            {
                // Trigger was allowed to click and is now depressed enough
                returnValue = true;
                rightTriggerCanClick = false;  // Prevent further clicks
            }

            return returnValue;
        }

        /// <summary>
        /// Enables polling for button presses
        /// </summary>
        public void Enable()
        {
            isPolling = true;
        }

        /// <summary>
        /// Disables polling for buttom presses. For preserving performance when
        /// no button presses are desired.
        /// </summary>
        public void Disable()
        {
            isPolling = false;
        }

        #region MonoBehavior Methods

        private void Start()
        {
            this.LeftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            this.RightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            leftTriggerCanClick = true;
            rightTriggerCanClick = true;
            isPolling = true;
        }

        private void Update()
        {
            const float pulled = 0.75f;
            const float released = 0.2f;

            if (!isPolling) return;

            // Get the pull values of the trigger.  This will work even if the triggers have been assigned as buttons
            this.LeftController.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue);
            this.RightController.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue);

            // Register trigger pressed if passed pull value
            leftTriggerDown = leftTriggerValue > pulled;
            rightTriggerDown = rightTriggerValue > pulled;

            // Only allow a click if the trigger has been released enough
            if (leftTriggerValue < released) { leftTriggerCanClick = true; }
            if (rightTriggerValue < released) { rightTriggerCanClick = true; }
        }

        #endregion
    }
}
