using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StageDressing.Models;
using System.Collections.Generic;
using System.Linq;

namespace StageDressing.UI
{
    [HotReload]
    public class InstanceListController : BSMLAutomaticViewController
    {

        #region SelectTracker Modal Members

        public string SelectedTrackerSerial { get; set; }
        private List<string> LoadedSerials;

        [UIComponent("SelectTrackerList")]
        public CustomListTableData trackerList;

        [UIAction("OnShowSelectTracker")]
        private void OnShowSelectTracker()
        {
            this.trackerList.tableView.ClearSelection();
            this.trackerList.data.Clear();

            TrackedDeviceManager.instance.LoadTrackedDevices();
            TrackedDeviceManager.instance.TrackedDevices.ForEach(t =>
            {
                string text = $"{t.serialNumber}: {t.manufacturer} {t.name}";
                var customCellInfo = new CustomListTableData.CustomCellInfo(text);
                this.trackerList.data.Add(customCellInfo);
            });

            // Save the list of serials for later reference
            this.LoadedSerials = TrackedDeviceManager.instance.TrackedDevices.Select(t => t.serialNumber).ToList();

            this.trackerList.tableView.ReloadData();
        }

        [UIAction("OnTrackerListCellSelected")]
        public void OnTrackerListCellSelected(TableView _, int row)
        {
            StageDressing.Logger.Info($"Selected Tracker: {this.SelectedTrackerSerial}");
            this.SelectedTrackerSerial = LoadedSerials[row];
        }

        [UIAction("OnTrackerSelectCancelled")]
        private void OnTrackerSelectCancelled()
        {
            this.SelectedTrackerSerial = null;
        }

        #endregion

        public PrefabData Prefab { get; private set; }
        public InstanceData Instance { get; private set; }


        [UIComponent("InstanceList")]
        public CustomListTableData instanceList;

        private string positionText;
        [UIValue("PositionText")]
        public string PositionText { get => this.positionText; set { this.positionText = value; this.NotifyPropertyChanged(nameof(this.PositionText)); } }

        private string rotationText;
        [UIValue("RotationText")]
        public string RotationText { get => this.rotationText; set { this.rotationText = value; this.NotifyPropertyChanged(nameof(this.RotationText)); } }

        private string scaleText;
        [UIValue("ScaleText")]
        public string ScaleText { get => this.scaleText; set { this.scaleText = value; this.NotifyPropertyChanged(nameof(this.ScaleText)); } }

        public void SetPrefab(PrefabData prefab)
        {
            this.Prefab = prefab;
            ReloadInstanceList();
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
            ReloadInstanceList();
        }

        private void ReloadInstanceList()
        {
            this.instanceList.tableView.ClearSelection();
            this.instanceList.data.Clear();

            if (this.Prefab != null)
            {
                this.Prefab.Instances.Select((i, n) => new CustomListTableData.CustomCellInfo($"Instance {n}")).ToList()
                    .ForEach(cell => this.instanceList.data.Add(cell));
            }

            this.instanceList.tableView.ReloadData();
        }

        [UIAction("OnInstanceSelected")]
        private void OnInstanceSelected(TableView _, int row)
        {
            var instance = this.Prefab.Instances[row];
            this.Instance = instance;

            var eulerRotation = instance.Pose.rotation.eulerAngles;
            this.PositionText = $"Position ({instance.Pose.position.x:0.00}, {instance.Pose.position.y:0.00}, {instance.Pose.position.z:0.00})";
            this.RotationText = $"Rotation ({eulerRotation.x:0.00}, {eulerRotation.y:0.00}, {eulerRotation.z:0.00})";
            this.ScaleText = $"Scale ({instance.Scale:0.00})";
        }

        [UIAction("OnCalibrate")]
        private void OnCalibrate()
        {
            if (this.Instance == null) return;
            TrackedObjectBehavior.Calibrate(this.Instance);
        }

        [UIAction("OnUnCalibrate")]
        private void OnUnCalibrate()
        {
            if (this.Instance == null) return;
            TrackedObjectBehavior.UnCalibrate(this.Instance);
        }
    }
}
