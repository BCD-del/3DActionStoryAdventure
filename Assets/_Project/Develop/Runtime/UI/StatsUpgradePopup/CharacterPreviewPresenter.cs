using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using UnityEngine.SceneManagement;

namespace Assets._Project.Develop.Runtime.UI.StatsUpgradePopup
{
    public class CharacterPreviewPresenter : IPresenter
    {
        private SceneLoaderService _sceneLoader;
        private ICoroutinesPerformer _coroutinesPerformer;

        public CharacterPreviewPresenter(SceneLoaderService sceneLoader, ICoroutinesPerformer coroutinePerformer)
        {
            _sceneLoader = sceneLoader;
            _coroutinesPerformer = coroutinePerformer;
        }

        public void Initialize()
        {
            _coroutinesPerformer.StartPerform(_sceneLoader.LoadAsync(Scenes.CharacterPreviewScene, LoadSceneMode.Additive));
        }

        public void Dispose()
        {
            _coroutinesPerformer.StartPerform(_sceneLoader.UnloadAsync(Scenes.CharacterPreviewScene));
        }
    }
}