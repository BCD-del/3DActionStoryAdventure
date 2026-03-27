using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Core.ConfirmPopup
{
    public class ConfirmPopupView : PopupViewBase
    {
        [SerializeField] private TMP_Text _headerText;

        [field: SerializeField] public Button ConfirmButton { get; private set; }
        [field: SerializeField] public Button DeclineButton { get; private set; }

        public void SetHeaderText(string value) => _headerText.text = value;

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            animation
                .Append(_headerText.transform
                    .DOScale(1, 0.2f)
                    .From(0)
                    .SetEase(Ease.OutSine));
        }
    }
}
