using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public class PopupAnimationsCreator
    {
        public static Sequence CreateShowAnimation(
            CanvasGroup body, 
            Image anticlicker, 
            PopupAnimationTypes animationType, 
            float anticlickerMaxAlpha)
        {
            switch (animationType)
            {
                case PopupAnimationTypes.None:
                    return DOTween.Sequence();

                case PopupAnimationTypes.Fade:
                    return DOTween.Sequence()
                        .Append(anticlicker
                            .DOFade(anticlickerMaxAlpha, 0.2f)
                            .From(0))
                        .Join(body
                            .DOFade(1, 0.3f)
                            .From(0));

                case PopupAnimationTypes.Expand:
                    return DOTween.Sequence()
                          .Append(anticlicker
                            .DOFade(endValue: anticlickerMaxAlpha, duration: 0.2f)
                            .From(0))
                .          Join(body.transform
                            .DOScale(endValue: 1, duration: 0.5f)
                            .From(0)
                            .SetEase(Ease.OutBack)
                        );

                default:
                    throw new ArgumentException(nameof(animationType));
            }
        }

        public static Sequence CreateHideAnimation(
            CanvasGroup body,
            Image anticlicker,
            PopupAnimationTypes animationType,
            float anticlickerMaxALpha)
        {
            return DOTween.Sequence();
        }
    }
}
