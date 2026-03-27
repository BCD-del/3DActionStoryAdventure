using Assets._Project.Develop.Runtime.UI.Core;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultPopups
{
    public class DefeatPopupView : PopupViewBase
    {
        public event Action ContinueClicked;
        public event Action RestartClicked;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;

        public void SetTitle(string title) => _title.text = title;

        public void OnContinueClick() => ContinueClicked?.Invoke();

        protected override void OnPreShow()
        {
            base.OnPreShow();

            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _restartButton.onClick.AddListener(OnRestartButtonClicked);
        }

        protected override void OnPreHide()
        {
            base.OnPreHide();

            _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            _restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            _restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }

        private void OnRestartButtonClicked()
        {
            RestartClicked?.Invoke();
        }

        private void OnContinueButtonClicked()
        {
            ContinueClicked?.Invoke();
        }
    }
}
