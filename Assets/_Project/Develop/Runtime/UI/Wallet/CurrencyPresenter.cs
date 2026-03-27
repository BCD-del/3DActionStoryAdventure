using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;

namespace Assets._Project.Develop.Runtime.UI.Wallet
{
    public class CurrencyPresenter : IPresenter
    {
        private readonly IReadOnlyVariable<int> _currency;
        private readonly CurrencyTypes _currencyTypes;
        private readonly CurrencyIconsConfig _currencyIconsConfig;

        private readonly IconTextView _view;

        private IDisposable _disposable;

        public CurrencyPresenter(IReadOnlyVariable<int> currency, 
            CurrencyTypes currencyTypes, 
            CurrencyIconsConfig currencyIconsConfig, 
            IconTextView view)
        {
            _currency = currency;
            _currencyTypes = currencyTypes;
            _currencyIconsConfig = currencyIconsConfig;
            _view = view;
        }

        public IconTextView View => _view;

        public void Initialize()
        {
            UpdateValue(_currency.Value);

            _disposable = _currency.Subscribe(OnCurrencyChanged);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private void OnCurrencyChanged(int arg1, int newValue) => UpdateValue(newValue);

        private void UpdateValue(int value)
        {
            string name = _currencyIconsConfig.GetSpriteNameFor(_currencyTypes);
            _view.SetText($"<space=-30><voffset=42><size=48><sprite name=\"{name}\"><size=32><voffset=30><space=-10>{value}");
        }
    }
}
