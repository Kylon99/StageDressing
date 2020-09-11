using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;

namespace StageDressing.UI
{
    [HotReload]
    public class SceneCompositionViewController : BSMLAutomaticViewController
    {
        [UIComponent("GameObjectList")]
        public CustomCellListTableData gameObjectList;

        [UIValue("GameObjectListContents")]
        public List<object> gameObjectListContents = new List<object>();

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);

            if (firstActivation)
            {
                this.gameObjectList.tableView.ClearSelection();
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject1" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject2" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject3" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject4" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject5" });
                this.gameObjectListContents.Add(new GameObjectListItem { Name = "GameObject6" });
            }

            if (this.gameObjectList != null)
                this.gameObjectList.tableView.ReloadData();
        }


        [UIAction("OnGameObjectSelected")]
        private void OnGameObjectSelected(string obj)
        {
            StageDressing.Logger.Info("OnGameObjectSelected");
        }
    }

    public class GameObjectListItem
    {
        [UIValue("GameObjectName")]
        public string Name { get; set; }
    }
}
