using Assets._Project.Develop.Runtime.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IconTextView : MonoBehaviour, IView
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _icon;

    public void SetText(string value)
    {
        gameObject.SetActive(true);
        _text.text = value;
    }

    public void SetIcon(Sprite sprite)
    {
        _icon.sprite = sprite;
    }
}
