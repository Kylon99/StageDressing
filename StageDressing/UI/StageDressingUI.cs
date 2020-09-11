using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using UnityEngine;

namespace StageDressing.UI
{
    public class StageDressingUI : MonoBehaviour
    {
        private ModMainFlowCoordinator mainFlowCoordinator;

        private void Awake()
        {
            MenuButton menuButton = new MenuButton(
                "Stage Dressing",
                "Add objects to the menu or game scene!", this.ShowModFlowCoordinator, true);
            MenuButtons.instance.RegisterButton(menuButton);
        }

        public void ShowModFlowCoordinator()
        {
            if (this.mainFlowCoordinator == null)
                this.mainFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<ModMainFlowCoordinator>();

            if (this.mainFlowCoordinator.IsBusy) return;

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(this.mainFlowCoordinator);
        }
    }
}
