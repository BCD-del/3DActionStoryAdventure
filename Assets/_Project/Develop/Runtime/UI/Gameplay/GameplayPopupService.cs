using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.ResultPopups;
using Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayPopupService : PopupService
    {
        private readonly GameplayUIRoot _uiRoot;
        private readonly GameplayPresentersFactory _gameplayPresentersFactory;

        public GameplayPopupService(
            ViewsFactory viewsFactory,
            ProjectPresentersFactory projectPresentersFactory,
            GameplayUIRoot uiRoot,
            GameplayPresentersFactory gameplayPresentersFactory)
            : base(viewsFactory, projectPresentersFactory)
        {
            _uiRoot = uiRoot;
            _gameplayPresentersFactory = gameplayPresentersFactory;
        }

        protected override Transform PopupLayer => _uiRoot.PopupsLayer;

        public AbilitySelectPopupPresenter OpenAbilitySelectPopup(
           Entity entity,
           int level,
           Action closedCallback = null)
        {
            AbilitySelectPopupView view = ViewsFactory.Create<AbilitySelectPopupView>(ViewIDs.AbilitySelectPopupView, PopupLayer);

            AbilitySelectPopupPresenter popup = _gameplayPresentersFactory.CreateAbilitySelectPopupPresenter(view, entity, level);

            OnPopupCreated(popup, view, closedCallback);

            return popup;
        }

        public WinPopupPresenter OpenWinPopup()
        {
            WinPopupView view = ViewsFactory.Create<WinPopupView>(ViewIDs.WinPopupView, PopupLayer);

            WinPopupPresenter popup = _gameplayPresentersFactory.CreateWinPopupPresenter(view);

            OnPopupCreated(popup, view);

            return popup;
        }

        public DefeatPopupPresenter OpenDefeatPopup(Action closeCallback = null)
        {
            DefeatPopupView view = ViewsFactory.Create<DefeatPopupView>(ViewIDs.DefeatPopupView, PopupLayer);

            DefeatPopupPresenter popup = _gameplayPresentersFactory.CreateDefeatPopupPresenter(view);

            OnPopupCreated(popup, view, closeCallback);

            return popup;
        }
    }
}
