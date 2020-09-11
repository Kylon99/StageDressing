using BeatSaberMarkupLanguage;
using HMUI;

namespace StageDressing.UI
{
    public class ModMainFlowCoordinator : FlowCoordinator
    {
        private SceneListViewController sceneListView;
        private SceneCompositionViewController sceneCompositionView;
        private AssetBundleListViewController assetBundleView;

        public bool IsBusy { get; set; }

        public void ShowSceneEdit()
        {
            this.IsBusy = true;
            this.ReplaceTopViewController(this.sceneCompositionView);
            this.SetLeftScreenViewController(this.assetBundleView);
            this.SetRightScreenViewController(null);
            this.IsBusy = false;
        }

        private void Awake()
        {
            this.sceneListView = BeatSaberUI.CreateViewController<SceneListViewController>();
            this.sceneListView.MainFlowCoordinator = this;
            this.sceneCompositionView = BeatSaberUI.CreateViewController<SceneCompositionViewController>();
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
            this.ProvideInitialViewControllers(this.sceneListView);
            this.IsBusy = false;
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            if (this.IsBusy) return;

            if (topViewController == this.sceneListView)
            {
                BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
            }

            if (topViewController == this.sceneCompositionView)
            {
                this.ReplaceTopViewController(this.sceneListView);
                this.SetLeftScreenViewController(null);
                this.SetRightScreenViewController(null);
            }
        }
    }
}
