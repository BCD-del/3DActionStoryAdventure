using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreenView : MonoBehaviour, IView
{
    public event Action StartGameButtonClicked;

    [field: SerializeField] public Button StartGameButton { get; private set; }

    private void OnEnable()
    {
        StartGameButton.onClick.AddListener(OnStartGameButtonClicked);
    }

    private void OnDisable()
    {
        StartGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
    }

    private void OnStartGameButtonClicked()
    {
        StartGameButtonClicked?.Invoke();
    }
}
