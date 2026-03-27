using Assets._Project.Develop.Runtime.UI.Core;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.ResultPopups
{
    public class WinPopupView : PopupViewBase
    {
        public event Action ContinueClicked;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private List<Transform> _stars;

        public void SetTitle(string title) => _title.text = title;

        public void OnContinueClick() => ContinueClicked?.Invoke();

        protected override void ModifyShowAnimation(Sequence animation)
        {
            base.ModifyShowAnimation(animation);

            foreach (Transform star in _stars)
            {
                animation
                    .Append(star.DOScale(1, 0.3f).SetEase(Ease.OutBack).From(0))
                    .Join(star.DOLocalRotate(Vector3.forward * 360, 0.3f, RotateMode.LocalAxisAdd)
                            .SetEase(Ease.OutCubic)
                            .From(Vector3.zero));

                animation.AppendInterval(0.1f);
            }
        }
    }
}
