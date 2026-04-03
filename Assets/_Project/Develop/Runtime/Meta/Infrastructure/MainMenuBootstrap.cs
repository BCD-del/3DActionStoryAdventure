using Assets._Project.Develop.Infrastructure;
using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Meta.Features; //.ShipUpgrades;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets._Project.Develop.Runtime.Utilites; //.DataProviders;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using System.Collections;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Meta.Infrastructure
{
    public class MainMenuBootstrap : SceneBootstrap
    {
        private DIContainer _container;
        private ICoroutinesPerformer _coroutinesPerformer;
      //  private PlayerDataProvider _playerDataProvider;

        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            _container = container;

            MainMenuContextRegistrations.Process(_container);
        }

        public override IEnumerator Initialize()
        {
            Debug.Log("Main menu scene init");

            //_playerDataProvider = _container.Resolve<PlayerDataProvider>();
            _coroutinesPerformer = _container.Resolve<ICoroutinesPerformer>();

            yield break;
        }

        public override void Run()
        {
            Debug.Log("Run main menu bootstrap");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
               // _coroutinesPerformer.StartPerform(_playerDataProvider.SaveAsync());
                Debug.Log("Data is saved");
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
               // _container.Resolve<PlayerMainShipDataProvider>().MaxHealth++;
                Debug.Log("max hp increased don't forget press f2 to save");
            }
        }
    }
}
