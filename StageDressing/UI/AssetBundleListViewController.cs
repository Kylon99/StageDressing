using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StageDressing.Models;
using System;
using System.Linq;

namespace StageDressing.UI
{
    [HotReload]
    public class AssetBundleListViewController : BSMLAutomaticViewController
    {
        [UIComponent("AssetBundleList")]
        public CustomListTableData assetBundleList;

        [UIComponent("PrefabList")]
        public CustomListTableData prefabList;

        public string SelectedAssetBundle { get; set; }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

            this.ReloadAssetBundleList();
        }

        [UIAction("OnAssetBundleSelected")]
        private void OnAssetBundleSelected(TableView _, int row)
        {
            this.SelectedAssetBundle = this.assetBundleList.data[row].text;
        }

        [UIAction("OnReload")]
        private void OnReload()
        {
            this.ReloadAssetBundleList();
        }

        [UIAction("OnShowPrefabList")]
        private void OnShowPrefabList()
        {
            if (String.IsNullOrWhiteSpace(this.SelectedAssetBundle)) return;

            var prefabNames = StageManager.instance.GetAllPrefabNames(this.SelectedAssetBundle);
            if (prefabNames == null) return;

            this.prefabList.data.Clear();
            prefabNames.Select(a => new CustomListTableData.CustomCellInfo(a)).ToList()
                .ForEach(cell => this.prefabList.data.Add(cell));
            this.prefabList.tableView.ReloadData();
        }

        [UIAction("OnCreateScene")]
        private void OnCreateScene()
        {
            StageDressing.Logger.Info("OnCreateScene");
        }

        private void ReloadAssetBundleList()
        {
            this.assetBundleList.tableView.ClearSelection();
            this.assetBundleList.data.Clear();
            var assetBundleNames = StageManager.instance.GetAllAssetBundles();

            assetBundleNames.Select(a => new CustomListTableData.CustomCellInfo(a)).ToList()
                .ForEach(cell => this.assetBundleList.data.Add(cell));

            if (this.assetBundleList != null)
                this.assetBundleList.tableView.ReloadData();
        }
    }
}
