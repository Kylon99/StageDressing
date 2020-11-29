using BS_Utils.Utilities;
using IPA;
using StageDressing.Models;
using StageDressing.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StageDressing
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class StageDressing
    {
        public static IPA.Logging.Logger Logger { get; private set; }

        private const string assemblyName = "StageDressing";
        public static readonly string stageAssetsPath = Path.Combine(Application.dataPath, "../CustomStageAssets/");

        public TrackedObjectBehavior trackedMenuObjectBehavior;
        private TrackedObjectBehavior trackedGameObjectBehavior;
        private GrabbableBehavior grabbableMenuBehavior;
        private GrabbableBehavior grabbableGameBehavior;
        private StageDressingUI stageDressingUI;

        public static string AssemblyName => assemblyName;

        public static List<Shader> VRMShaders { get; private set; }

        [Init]
        public StageDressing(IPA.Logging.Logger logger)
        {
            StageDressing.Logger = logger;
        }

        [OnStart]
        public void OnStart()
        {
            PersistentSingleton<InputManager>.TouchInstance();
            PersistentSingleton<TrackedDeviceManager>.TouchInstance();

            if (this.stageDressingUI == null) this.stageDressingUI = new GameObject(nameof(StageDressingUI)).AddComponent<StageDressingUI>();

            PersistentSingleton<StageManager>.TouchInstance();
            StageManager.instance.LoadConfiguration();

            TrackedDeviceManager.instance.StageDressing = this;
            StageManager.instance.LoadPrefabs();

            BSEvents.OnLoad();
            BSEvents.menuSceneLoaded += this.MenuSceneLoaded;
            BSEvents.lateMenuSceneLoadedFresh += this.LateMenuSceneLoadedFresh;
            BSEvents.gameSceneLoaded += this.GameSceneLoaded;
        }

        private void MenuSceneLoaded()
        {
            StageDressing.Logger.Info($"MenuSceneLoaded");
            TrackedDeviceManager.instance.LoadTrackedDevices();

            StageManager.instance.LoadPrefabs();
            StageManager.instance.RebuildMenuInstances();
        }

        private void LateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO obj)
        {
            StageDressing.Logger.Info($"LateMenuSceneLoadedFresh");

            TrackedDeviceManager.instance.LoadTrackedDevices();
            StageManager.instance.LoadPrefabs();
            StageManager.instance.RebuildMenuInstances();

            if (this.trackedMenuObjectBehavior == null) this.trackedMenuObjectBehavior = new GameObject(nameof(TrackedObjectBehavior)).AddComponent<TrackedObjectBehavior>();
            this.trackedMenuObjectBehavior.LoadTrackedInstances();

            if (this.grabbableMenuBehavior == null) this.grabbableMenuBehavior = new GameObject(nameof(GrabbableBehavior)).AddComponent<GrabbableBehavior>();
            this.grabbableMenuBehavior.LoadGrabbableInstances();

            // Debug logging of existing tracked devices
            //var trackedDevices = new List<InputDevice>();
            //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.TrackedDevice, trackedDevices);
            //Logger.Info("########## Tracked Devices");
            //Logger.Info(string.Join("\r\n", trackedDevices.Select(t => $"{t.manufacturer} {t.name} {t.serialNumber} valid: {t.isValid} characteristics: {t.characteristics}")));

            //List<InputDevice> handControllers = new List<InputDevice>();
            //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, handControllers);
            //Logger.Info("########## Controller Hand Devices");
            //Logger.Info(string.Join("\r\n", handControllers.Select(t => $"{t.manufacturer} {t.name} {t.serialNumber} valid: {t.isValid} characteristics: {t.characteristics}")));

            //List<InputDevice> leftControllers = new List<InputDevice>();
            //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, leftControllers);
            //Logger.Info("########## Left Devices");
            //Logger.Info(string.Join("\r\n", leftControllers.Select(t => $"{t.manufacturer} {t.name} {t.serialNumber} valid: {t.isValid} characteristics: {t.characteristics}")));

            //List<InputDevice> rightControllers = new List<InputDevice>();
            //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, rightControllers);
            //Logger.Info("########## Right Devices");
            //Logger.Info(string.Join("\r\n", rightControllers.Select(t => $"{t.manufacturer} {t.name} {t.serialNumber} valid: {t.isValid} characteristics: {t.characteristics}")));
        }

        private void GameSceneLoaded()
        {
            StageDressing.Logger.Info($"GameSceneLoaded");

            TrackedDeviceManager.instance.LoadTrackedDevices();
            StageManager.instance.CreateGameInstances();

            if (this.trackedGameObjectBehavior == null) this.trackedGameObjectBehavior = new GameObject(nameof(TrackedObjectBehavior)).AddComponent<TrackedObjectBehavior>();
            this.trackedGameObjectBehavior.LoadTrackedInstances(gameScene: true);

            if (this.grabbableGameBehavior == null) this.grabbableGameBehavior = new GameObject(nameof(GrabbableBehavior)).AddComponent<GrabbableBehavior>();
            this.grabbableGameBehavior.LoadGrabbableInstances(gameScene: true);
        }
    }
}
