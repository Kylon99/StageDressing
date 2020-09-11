using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;

namespace StageDressing.UI
{
    [HotReload]
    public class AssetBundleListViewController : BSMLAutomaticViewController
    {
        [UIComponent("AssetBundleList")]
        public CustomCellListTableData assetBundleList;

        [UIValue("AssetBundleListContents")]
        public List<object> assetBundleListContents = new List<object>();

        [UIComponent("GameObjectList")]
        public CustomCellListTableData gameObjectList;

        [UIValue("GameObjectListContents")]
        public List<object> gameObjectListContents = new List<object>();


        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

            if (firstActivation)
            {
                this.assetBundleList.tableView.ClearSelection();
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle1" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle2" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle3" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle4" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle5" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle6" });
                this.assetBundleListContents.Add(new AssetBundleListItem { Name = "AssetBundle7" });

                this.gameObjectList.tableView.ClearSelection();
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject1" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject2" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject3" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject4" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject5" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject6" });
                this.gameObjectListContents.Add(new AssetBundleGameObjectListItem { Name = "GameObject7" });
            }

            if (this.assetBundleList != null)
                this.assetBundleList.tableView.ReloadData();

            if (this.gameObjectList != null)
                this.gameObjectList.tableView.ReloadData();

        }

        [UIAction("OnAssetBundleSelected")]
        private void OnAssetBundleSelected()
        {
            StageDressing.Logger.Info("OnAssetBundleSelected");
        }

        [UIAction("OnAddGameObjects")]
        private void OnAddGameObjects()
        {
            StageDressing.Logger.Info("OnAddGameObjects");
        }
    }

    public class AssetBundleListItem
    {
        [UIValue("AssetBundleName")]
        public string Name { get; set; }
    }

    public class AssetBundleGameObjectListItem
    {
        [UIValue("AssetBundleGameObjectList")]
        public string Name { get; set; }
    }
}
