using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using StageDressing.Models;
using System.Linq;

namespace StageDressing.UI
{
    [HotReload]
    public class PrefabsListController : BSMLAutomaticViewController
    {
        public SceneData Scene { get; private set; }
        public InstanceListController InstanceListView { get; private set; }

        [UIComponent("PrefabsList")]
        public CustomListTableData prefabsList;

        [UIComponent("InstanceList")]
        public CustomListTableData instanceList;

        public void SetScene(SceneData scene)
        {
            this.Scene = scene;
        }

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
            ReloadPrefabsList();
        }


        [UIAction("OnPrefabSelected")]
        private void OnPrefabSelected(TableView _, int row)
        {
            var prefab = this.Scene.Prefabs[row];
            InstanceListView.SetPrefab(prefab);
        }

        private void ReloadPrefabsList()
        {
            if (this.Scene == null) return;

            this.prefabsList.tableView.ClearSelection();
            this.prefabsList.data.Clear();

            this.Scene.Prefabs.Select(a => new CustomListTableData.CustomCellInfo(a.Name)).ToList()
                .ForEach(cell => this.prefabsList.data.Add(cell));

            this.prefabsList.tableView.ReloadData();
        }

        public void SetInstanceView(InstanceListController instanceListView)
        {
            this.InstanceListView = instanceListView;
        }
    }
}
