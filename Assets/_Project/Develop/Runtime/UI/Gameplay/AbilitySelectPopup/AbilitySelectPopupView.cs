using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View
{
    public class AbilitySelectPopupView : PopupViewBase
    {
        public event Action SelectButtonClicked;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _selectAbilityText;

        [SerializeField] private Button _selectButton;

        [SerializeField] private SelectableAbilityListView _abilityListView;

        public SelectableAbilityListView AbilityListView => _abilityListView;

        private void OnEnable()
        {
            _selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDisable()
        {
            _selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }

        public void SetTitle(string title) => _title.text = title;

        public void SelectButtonOn() => _selectButton.gameObject.SetActive(true);

        public void SelectButtonOff() => _selectButton.gameObject.SetActive(false);

        public void SetAdditionalText(string additionalText) => _selectAbilityText.text = additionalText;

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            animation.Append(_abilityListView.Show());
        }

        protected override void ModifyHideAnimation(Sequence animation)
        {
            base.ModifyHideAnimation(animation);

            animation.Append(_abilityListView.Hide());
        }

        private void OnSelectButtonClicked() => SelectButtonClicked?.Invoke();
    }
}