using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StageDressing.Models;
using System.Collections.Generic;
using System.Linq;

namespace StageDressing.UI
{
    [HotReload]
    public class SceneListViewController : BSMLAutomaticViewController
    {
        public ModMainFlowCoordinator MainFlowCoordinator { get; set; }

#pragma warning disable CS0649
        [UIParams]
        BSMLParserParams parserParams;
#pragma warning restore CS0649


        #region SelectTracker Members

        public string SelectedTrackerSerial { get; set; }
        private List<string> LoadedSerials;

        [UIComponent("SelectTrackerList")]
        public CustomListTableData trackerList;

        [UIAction("OnShowSelectTracker")]
        private void OnShowSelectTracker()
        {
            StageDressing.Logger.Info($"selectTrackerList null? {this.trackerList == null}");
            StageDressing.Logger.Info($"selectTrackerList.tableView null? {this.trackerList.tableView == null}");
            StageDressing.Logger.Info($"selectTrackerList.data null? {this.trackerList.data == null}");

            this.trackerList.tableView.ClearSelection();
            this.trackerList.data.Clear();

            PersistentSingleton<TrackedDeviceManager>.instance.LoadTrackedDevices();
            PersistentSingleton<TrackedDeviceManager>.instance.TrackedDevices.ForEach(t =>
            {
                string text = $"{t.serialNumber}: {t.manufacturer} {t.name}";
                var customCellInfo = new CustomListTableData.CustomCellInfo(text);
                this.trackerList.data.Add(customCellInfo);
            });

            // Save the list of serials for later reference
            this.LoadedSerials = PersistentSingleton<TrackedDeviceManager>.instance.TrackedDevices.Select(t => t.serialNumber).ToList();

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

        [UIComponent("SceneList")]
        public CustomCellListTableData sceneList;

        [UIValue("SceneListContents")]
        public List<object> sceneListContents = new List<object>();

        public Models.SceneInfo SelectedScene { get; private set; }

        //[UIComponent("SceneNameKeyboard")]
        //private ModalKeyboard sceneNameKeyboard;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

            if (firstActivation)
            {
                this.sceneList.tableView.ClearSelection();
                PersistentSingleton<StageManager>.instance.Configuration.Scenes.ForEach(s =>
                {
                    this.sceneListContents.Add(new SceneListItem { Name = s.Name, Data = s });
                });
            }

            if (this.sceneList != null)
                this.sceneList.tableView.ReloadData();
        }

        private string selectedSceneName;
        [UIValue("SelectedSceneName")]
        public string SelectedSceneName
        {
            get => this.selectedSceneName;
            set { this.selectedSceneName = value; this.NotifyPropertyChanged(nameof(this.SelectedSceneName)); }
        }

        private bool showInMenu;
        [UIValue("ShowInMenu")]
        public bool ShowInMenu
        {
            get => this.showInMenu;
            set
            {
                this.showInMenu = value;
                this.SelectedScene.ShowInMenu = value;
                this.parserParams.EmitEvent("showInMenuGet");
                PersistentSingleton<StageManager>.instance.LoadPrefabs();
                PersistentSingleton<StageManager>.instance.RebuildMenuInstances();
            }
        }

        private bool showInGame;
        [UIValue("ShowInGame")]
        public bool ShowInGame
        {
            get => this.showInGame;
            set
            {
                this.showInGame = value;
                this.SelectedScene.ShowInGame = value;
                this.parserParams.EmitEvent("showInGameGet");
                PersistentSingleton<StageManager>.instance.LoadPrefabs();
                PersistentSingleton<StageManager>.instance.RebuildMenuInstances();
            }
        }

        [UIAction("OnSceneNameRenamed")]
        private void OnSceneNameRenamed(string obj)
        {
            StageDressing.Logger.Info("OnSceneNameRenamed");
        }

        [UIAction("OnSceneSelected")]
        private void OnSceneSelected(TableView sender, SceneListItem item)
        {
            this.SelectedScene = item.Data;
            this.SelectedSceneName = this.SelectedScene.Name;
            this.ShowInMenu = this.SelectedScene.ShowInMenu;
            this.ShowInGame = this.SelectedScene.ShowInGame;
        }

        [UIAction("OnNewScene")]
        private void OnNewScene()
        {
            StageDressing.Logger.Info("OnNewScene");
            PersistentSingleton<StageManager>.instance.SaveConfiguration();
        }

        [UIAction("OnCalibrate")]
        private void OnCalibrate()
        {
            StageDressing.Logger.Info("OnCalibrate");
            PersistentSingleton<TrackedDeviceManager>.instance.StageDressing.trackedMenuObjectBehavior.Calibrate();
        }

        [UIAction("OnUnCalibrate")]
        private void OnUnCalibrate()
        {
            StageDressing.Logger.Info("OnUnCalibrate");
            PersistentSingleton<TrackedDeviceManager>.instance.StageDressing.trackedMenuObjectBehavior.UnCalibrate();
        }

        [UIAction("OnEdit")]
        private void OnEdit()
        {
            StageDressing.Logger.Info("OnEdit");
            this.MainFlowCoordinator.ShowSceneEdit();
        }
    }

    public class SceneListItem
    {
        [UIValue("SceneName")]
        public string Name { get; set; }

        public Models.SceneInfo Data { get; set; }
    }
}
