using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.StatsUpgrade;
using Assets._Project.Develop.Runtime.UI;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.StatsUpgradePopup;
using Assets._Project.Develop.Runtime.UI.Wallet;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using System.Collections.Generic;

namespace Assets.CourseGame.Develop.MainMenu.StatsUpgradeFeature
{
    public class StatsUpgradePopupPresenter : PopupPresenterBase
    {
        private readonly StatsUpgradePopupView _view;
        private readonly ViewsFactory _viewsFactory;
        private readonly ProjectPresentersFactory _projectPresentersFactory;
        private readonly StatsUpgradeService _statsUpgradeService;

        private List<UpgradableStatPresenter> _upgradableStatPresenters = new();
        private WalletPresenter _walletPresenter;
        private CharacterPreviewPresenter _characterPreviewPresenter;

        public StatsUpgradePopupPresenter(
            ICoroutinesPerformer coroutinesPerformer,
            StatsUpgradePopupView view,
            ProjectPresentersFactory projectPresentersFactory,
            StatsUpgradeService statsUpgradeService,
            ViewsFactory viewsFactory) : base(coroutinesPerformer)
        {
            _view = view;
            _projectPresentersFactory = projectPresentersFactory;
            _statsUpgradeService = statsUpgradeService;
            _viewsFactory = viewsFactory;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetTitle("UPGRADE YOUR STATS");

            _walletPresenter = _projectPresentersFactory.CreateWalletPresenter(_view.CurrencyListView);
            _walletPresenter.Initialize();

            _characterPreviewPresenter = _projectPresentersFactory.CreateCharacterPreviewPresenter();
            _characterPreviewPresenter.Initialize();

            foreach (StatTypes statType in _statsUpgradeService.UpgradeableStats)
            {
                UpgradableStatView upgradableStatView = _viewsFactory.Create<UpgradableStatView>(ViewIDs.UpgradableStatView);
                _view.UpgradableStatListView.Add(upgradableStatView);

                UpgradableStatPresenter upgradableStatPresenter = _projectPresentersFactory.CreateUpgradableStatPresenter(upgradableStatView, statType);
                _upgradableStatPresenters.Add(upgradableStatPresenter);
                upgradableStatPresenter.Initialize();
            }
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            foreach (UpgradableStatPresenter presenter in _upgradableStatPresenters)
                presenter.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (UpgradableStatPresenter presenter in _upgradableStatPresenters)
            {
                presenter.Dispose();
                _view.UpgradableStatListView.Remove(presenter.View);
                _viewsFactory.Release(presenter.View);
            }

            _upgradableStatPresenters.Clear();

            _walletPresenter.Dispose();
            _characterPreviewPresenter.Dispose();
        }
    }
}