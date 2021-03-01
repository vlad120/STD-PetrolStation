using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class VolumePanel : Page {
        int _num;
        const int MAX_VOLUME_ALLOWED = 100;

        public VolumePanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Volume = this;

            string fuelChosen = App.CurrPages.Main.CurrState.FuelChosen;
            if (fuelChosen == "D") {
                fuelLabel.Text = "Дизель";
            } else {
                fuelLabel.Text = "АИ-" + fuelChosen;
            }
            acceptBtn.IsEnabled = false;

            int maxVolume = getMaxVolume();
            if (maxVolume > MAX_VOLUME_ALLOWED) {
                maxVolume = MAX_VOLUME_ALLOWED;
            }
            fuelVolume.PlaceholderText = "от 1 до " + maxVolume;
        }

        private int getMaxVolume() {
            switch (App.CurrPages.Main.CurrState.FuelChosen) {
                case "92": return Stocks.Fuel.Volume92;
                case "95": return Stocks.Fuel.Volume95;
                case "98": return Stocks.Fuel.Volume98;
                case "D": return Stocks.Fuel.VolumeD;
                default: return 0;
            }
        }

        private void fuelVolume_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            acceptBtn.IsEnabled = int.TryParse(sender.Text, out _num);
        }

        private async void acceptBtn_Click(object sender, RoutedEventArgs e) {
            try {
                _num = int.Parse(fuelVolume.Text);
            } catch (Exception) {
                acceptBtn.IsEnabled = false;
                await Alert.ShowAsync(Alert.ERROR, $"Некорректное значение! ('{fuelVolume.Text}')");
                return;
            }
            if (_num < 1) {
                await Alert.ShowAsync(Alert.ERROR, $"Некорректное значение! (разрешено от 1 до {MAX_VOLUME_ALLOWED})");
                return;
            }
            if (_num > MAX_VOLUME_ALLOWED) {
                await Alert.ShowAsync(Alert.ERROR, $"Выбрано слишком много! (максимум {MAX_VOLUME_ALLOWED})");
                return;
            }

            int maxVolume = getMaxVolume();
            if (_num > getMaxVolume()) {
                await Alert.ShowAsync(Alert.ERROR, $"К сожалению, выбранное количество недоступно! (максимум {maxVolume})");
                return;
            }

            App.CurrPages.Main.CurrState.VolumeChosen = _num;
            Debug.WriteLine("Selected volume: " + _num);
            App.CurrPages.Main.SetPaymentPanel();
        }
    }
}
