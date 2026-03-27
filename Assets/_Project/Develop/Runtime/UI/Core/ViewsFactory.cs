using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public class ViewsFactory
    {
        private readonly ResourcesAssetsLoader _resourcesAssetsLoader;

        private readonly Dictionary<string, string> _viewIDToResourcesPath = new Dictionary<string, string>()
        {
            {ViewIDs.StatsUpgradePopupView, "UI/StatsUpgradePopup/StatsUpgradePopupView" },
            {ViewIDs.UpgradableStatView, "UI/StatsUpgradePopup/UpgradableStatView" },

            {ViewIDs.CurrencyView, "UI/Wallet/CurrencyView" },

            {ViewIDs.MainMenuScreenView, "UI/MainMenu/MainMenuScreenView" },
            {ViewIDs.GameplayScreenView, "UI/Gameplay/GameplayScreenView" },

            {ViewIDs.ConfirmPopup, "UI/ConfirmPopup" },

            {ViewIDs.LevelsMenuPopup, "UI/LevelsMenuPopup/LevelsMenuPopup" },
            {ViewIDs.LevelTile, "UI/LevelsMenuPopup/LevelTile" },

            {ViewIDs.WinPopupView, "UI/Gameplay/ResultPopups/WinPopupView" },
            {ViewIDs.DefeatPopupView, "UI/Gameplay/ResultPopups/DefeatPopupView" },

            {ViewIDs.SimpleHealthBar, "UI/Gameplay/HealthBars/SimpleHealthBar" },
            {ViewIDs.MainHeroHealthBar, "UI/Gameplay/HealthBars/MainHeroHealthBar" },

            {ViewIDs.AbilitySelectView, "UI/Gameplay/AbilitiySelectPopup/SelectionAbilityView" },
            {ViewIDs.AbilitySelectPopupView, "UI/Gameplay/AbilitiySelectPopup/SelectionAbilityPopup" },
        };

        public ViewsFactory(ResourcesAssetsLoader resourcesAssetsLoader)
        {
            _resourcesAssetsLoader = resourcesAssetsLoader;
        }

        public TView Create<TView>(string viewID, Transform parent = null) where TView : MonoBehaviour, IView 
        {
            if (_viewIDToResourcesPath.TryGetValue(viewID, out string resourcePath) == false)
                throw new ArgumentException($"You didn't set resource path for {typeof(TView)}, searched id: {viewID}");

            GameObject prefab = _resourcesAssetsLoader.Load<GameObject>(resourcePath);
            GameObject instance = Object.Instantiate(prefab, parent);
            TView view = instance.GetComponent<TView>();

            if (view == null)
                throw new InvalidOperationException($"Not found {typeof(TView)} component of view instance");

            return view;
        }

        public void Release<TView>(TView view) where TView : MonoBehaviour, IView
        {
            Object.Destroy(view.gameObject);
        }
    }
}
