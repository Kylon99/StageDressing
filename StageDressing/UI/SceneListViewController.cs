using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StageDressing.Models;
using System.Linq;

namespace StageDressing.UI
{
    [HotReload]
    public class SceneListViewController : BSMLAutomaticViewController
    {
        public ModMainFlowCoordinator MainFlowCoordinator { get; private set; }
        public PrefabsListController PrefabsListView { get; private set; }

        public Models.SceneData SelectedScene { get; set; }

        [UIComponent("SceneList")]
        public CustomListTableData sceneList;

        //[UIComponent("SceneNameKeyboard")]
        //private ModalKeyboard sceneNameKeyboard;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
            ReloadSceneList();
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
            }
        }

        private bool followRoomAdjust;
        [UIValue("FollowRoomAdjust")]
        public bool FollowRoomAdjust
        {
            get => this.followRoomAdjust;
            set
            {
                this.followRoomAdjust = value;
                this.SelectedScene.FollowRoomAdjust = value;
                this.parserParams.EmitEvent("followRoomAdjustGet");
            }
        }

        [UIAction("OnSceneNameRenamed")]
        private void OnSceneNameRenamed(string obj)
        {
            StageDressing.Logger.Info("OnSceneNameRenamed");
        }

        [UIAction("OnSceneSelected")]
        private void OnSceneSelected(TableView _, int row)
        {
            this.SelectedScene = StageManager.instance.Configuration.Scenes.ElementAt(row);
            this.SelectedSceneName = this.SelectedScene.Name;
            this.ShowInMenu = this.SelectedScene.ShowInMenu;
            this.ShowInGame = this.SelectedScene.ShowInGame;
        }

        [UIAction("OnSaveAll")]
        private void OnSaveAll()
        {
            StageDressing.Logger.Info("OnNewScene");
            StageManager.instance.SaveConfiguration();
        }

        [UIAction("OnReloadAll")]
        private void OnReloadAll()
        {
            StageDressing.Logger.Info("OnReloadAll");
            StageManager.instance.LoadConfiguration();
            this.ReloadSceneList();

            StageManager.instance.LoadPrefabs();
            StageManager.instance.RebuildMenuInstances();
        }

        [UIAction("OnEdit")]
        private void OnEdit()
        {
            if (this.SelectedScene == null) return;
            this.PrefabsListView.SetScene(this.SelectedScene);
            this.MainFlowCoordinator.ShowSceneEdit(this.SelectedScene);
        }

        private void ReloadSceneList()
        {
            this.sceneList.tableView.ClearSelection();
            this.sceneList.data.Clear();
            StageManager.instance.Configuration.Scenes.ForEach(s => this.sceneList.data.Add(new CustomListTableData.CustomCellInfo(s.Name)));
            this.sceneList.tableView.ReloadData();
        }

        public void SetMainFlowCoordinator(ModMainFlowCoordinator modMainFlowCoordinator)
        {
            this.MainFlowCoordinator = modMainFlowCoordinator;
        }

        public void SetPrefabsListView(PrefabsListController prefabListView)
        {
            this.PrefabsListView = prefabListView;
        }

#pragma warning disable CS0649
        [UIParams]
        BSMLParserParams parserParams;
#pragma warning restore CS0649
    }
}
