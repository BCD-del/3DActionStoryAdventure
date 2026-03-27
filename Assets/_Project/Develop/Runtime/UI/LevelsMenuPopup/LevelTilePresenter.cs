using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Meta.Features.LevelsProgression;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;

namespace Assets._Project.Develop.Runtime.UI.LevelsMenuPopup
{
    public class LevelTilePresenter : ISubscribePresenter
    {
        private readonly SceneSwitcherService _sceneSwitcher;
        private readonly ICoroutinesPerformer _coroutinesPerformer;
        private readonly GameplayInputArgs _inputArgs;
        private readonly LevelTileView _view;
        private readonly LevelConfig _config;
        private readonly LevelsProgressionService _levelsService;

        public LevelTilePresenter(
            SceneSwitcherService sceneSwitcher, 
            ICoroutinesPerformer coroutinesPerformer, 
            GameplayInputArgs inputArgs, 
            LevelTileView view,
            LevelConfig config,
            LevelsProgressionService levelsService)
        {
            _sceneSwitcher = sceneSwitcher;
            _coroutinesPerformer = coroutinesPerformer;
            _inputArgs = inputArgs;
            _view = view;
            _config = config;
            _levelsService = levelsService;
        }

        public LevelTileView View => _view;

        public void Initialize()
        {
            _view.Init(_config.LevelName, _config.LevelIcon);

            if (_levelsService.CanPlay(_config.LevelNumber))
            {
                if (_levelsService.IsLevelCompleted(_config.LevelNumber))
                    _view.SetComplete();
                else
                    _view.SetActive();
            }
            else
            {
                _view.SetBlock();
            }
        }

        public void Dispose()
        {
            _view.Clicked -= OnViewClicked;
        }


        public void Subscribe()
        {
            _view.Clicked += OnViewClicked;
        }

        public void Unsubscribe()
        {
            _view.Clicked -= OnViewClicked;
        }

        private void OnViewClicked()
        {
            _coroutinesPerformer
                .StartPerform(_sceneSwitcher.ProcessingSwitchTo(Scenes.Gameplay, _inputArgs));
        }
    }
}
