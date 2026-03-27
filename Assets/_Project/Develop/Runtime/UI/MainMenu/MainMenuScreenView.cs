using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreenView : MonoBehaviour, IView
{
    public event Action StartGameButtonClicked;
    public event Action UpgradesButtonClicked;

    [field: SerializeField] public IconTextListView WalletView { get; private set; }
    [field: SerializeField] public Button StartGameButton { get; private set; }
    [field: SerializeField] public Button UpgradesButton { get; private set; }

    private void OnEnable()
    {
        StartGameButton.onClick.AddListener(OnStartGameButtonClicked);
        UpgradesButton.onClick.AddListener(OnUpgradesButtonClicked);
    }

    private void OnDisable()
    {
        StartGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
        UpgradesButton.onClick.RemoveListener(OnUpgradesButtonClicked);
    }

    private void OnStartGameButtonClicked()
    {
        StartGameButtonClicked?.Invoke();
    }

    private void OnUpgradesButtonClicked()
    {
        UpgradesButtonClicked?.Invoke();
    }
}
