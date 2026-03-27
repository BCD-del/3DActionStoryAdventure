using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.UI.LevelsMenuPopup
{
    public class LevelsMenuPopupPresenter : PopupPresenterBase
    {
        private const string TitleName = "Выберите режим игры";

        private readonly ConfigsProviderService _configProviderService;
        private readonly ProjectPresentersFactory _presentersFactory;
        private readonly ViewsFactory _viewsFactory;

        private readonly LevelsMenuPopupView _view;

        private readonly List<LevelTilePresenter> _levelTilePresenters = new();

        public LevelsMenuPopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            ConfigsProviderService configProviderService,
            ProjectPresentersFactory presentersFactory,
            ViewsFactory viewsFactory, 
            LevelsMenuPopupView view) : base(coroutinesPerformer)
        {
            _configProviderService = configProviderService;
            _presentersFactory = presentersFactory;
            _viewsFactory = viewsFactory;
            _view = view;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle(TitleName);

            LevelsListConfig levelsListConfig = _configProviderService.GetConfig<LevelsListConfig>();

            for (int i = 0; i < levelsListConfig.Levels.Count; i++)
            {
                LevelTileView levelTileView = _viewsFactory.Create<LevelTileView>(ViewIDs.LevelTile);

                _view.LevelTilesListView.Add(levelTileView);

                GameplayInputArgs inputArgs = new GameplayInputArgs(levelsListConfig.Levels[i].LevelNumber, levelsListConfig.Levels[i].ContactTriggerSpawnPointPosition);

                LevelTilePresenter levelTilePresenter = _presentersFactory
                    .CreateLevelTilePresenter(levelTileView, inputArgs, levelsListConfig.Levels[i]);

                levelTilePresenter.Initialize();

                _levelTilePresenters.Add(levelTilePresenter);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (LevelTilePresenter levelTilePresenter in _levelTilePresenters)
            {
                _view.LevelTilesListView.Remove(levelTilePresenter.View);
                _viewsFactory.Release(levelTilePresenter.View);
                levelTilePresenter.Dispose();
            }

            _levelTilePresenters.Clear();
        }

        protected override void OnPreShow()
        {
            base.OnPreShow();

            foreach (LevelTilePresenter levelTilePresenter in _levelTilePresenters)
                levelTilePresenter.Subscribe();
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            foreach (LevelTilePresenter levelTilePresenter in _levelTilePresenters)
                levelTilePresenter.Unsubscribe();
        }
    }
}
