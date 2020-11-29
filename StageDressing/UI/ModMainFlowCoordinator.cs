using BeatSaberMarkupLanguage;
using HMUI;
using StageDressing.Models;

namespace StageDressing.UI
{
    public class ModMainFlowCoordinator : FlowCoordinator
    {
        // Main Stage Dressing Views
        private SceneListViewController sceneListView;
        private AssetBundleListViewController assetBundleView;

        // On Scene Edit Views
        private PrefabsListController prefabListView;
        private InstanceListController instanceListView;

        public bool IsBusy { get; set; }

        public void ShowSceneEdit(SceneData selectedScene)
        {
            this.IsBusy = true;
            this.title = "Edit Scene";
            this.ReplaceTopViewController(this.prefabListView);
            this.SetLeftScreenViewController(null);
            this.SetRightScreenViewController(this.instanceListView);
            this.IsBusy = false;
        }

        private void Awake()
        {
            this.prefabListView = BeatSaberUI.CreateViewController<PrefabsListController>();
            this.instanceListView = BeatSaberUI.CreateViewController<InstanceListController>();
            this.prefabListView.SetInstanceView(this.instanceListView);

            this.sceneListView = BeatSaberUI.CreateViewController<SceneListViewController>();
            this.sceneListView.SetMainFlowCoordinator(this);
            this.sceneListView.SetPrefabsListView(this.prefabListView);
            this.assetBundleView = BeatSaberUI.CreateViewController<AssetBundleListViewController>();

        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation)
            {
                this.title = "Stage Dressing";
                this.showBackButton = true;
            }
            this.IsBusy = true;
            this.ProvideInitialViewControllers(this.sceneListView, this.assetBundleView);
            this.IsBusy = false;
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (this.IsBusy) return;

            if (topViewController == this.sceneListView)
            {
                BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
            }

            if (topViewController == this.prefabListView)
            {
                this.title = "Stage Dressing";
                this.ReplaceTopViewController(this.sceneListView);
                this.SetLeftScreenViewController(this.assetBundleView);
                this.SetRightScreenViewController(null);
            }
        }
    }
}
