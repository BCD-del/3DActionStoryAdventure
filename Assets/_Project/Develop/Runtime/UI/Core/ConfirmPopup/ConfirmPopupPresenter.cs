using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Core.ConfirmPopup
{
    public class ConfirmPopupPresenter : PopupPresenterBase
    {
        private readonly ConfirmPopupView _view;
        private readonly Action ConfirmCallback;
        private readonly string _header;

        public ConfirmPopupPresenter(
            ConfirmPopupView view,
            ICoroutinesPerformer coroutinesPerformer, Action onConfirmButtonClicked, string header) : base(coroutinesPerformer)
        {
            _view = view;
            ConfirmCallback = onConfirmButtonClicked;
            _header = header;
        }

        protected override PopupViewBase PopupView => _view;

        public override void Initialize()
        {
            base.Initialize();

            _view.SetHeaderText(_header);
            _view.ConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        public override void Dispose()
        {
            base.Dispose();

            _view.ConfirmButton.onClick.RemoveListener(OnConfirmButtonClicked);
        }

        private void OnConfirmButtonClicked()
        {
            Debug.Log("Confirm button clicked");
            ConfirmCallback?.Invoke();
        }
    }
}
