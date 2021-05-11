using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class VolumePanel : Page {
        const double MAX_VOLUME_ALLOWED = 100d;
        double _volume;
        Stocks.FuelInfo.Fuel _fuelChosen;

        public VolumePanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Volume = this;

            _fuelChosen = App.CurrPages.Main.CurrState.FuelChosen;

            fuelLabel.Text = _fuelChosen.Name;
            acceptBtn.IsEnabled = false;

            double maxVolume = getMaxVolume();
            fuelVolume.PlaceholderText = "от 1 до " + maxVolume;
            fuelVolume.MaxLength = maxVolume.ToString().Length + 3;  // x,yy

            totalCost.Text = "";
        }

        private double getMaxVolume() {
            double availVolume = _fuelChosen.AvailVolume;
            if (availVolume > MAX_VOLUME_ALLOWED) {
                return MAX_VOLUME_ALLOWED;
            }
            return availVolume;
        }

        private void fuelVolume_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            if (double.TryParse(sender.Text, out _volume)) {
                if (_volume < 1) {
                    acceptBtn.IsEnabled = false;
                    totalCost.Text = "";
                }
                else if (_volume <= getMaxVolume()) {
                    acceptBtn.IsEnabled = true;
                    totalCost.Text = Math.Round(_volume * _fuelChosen.Cost, 2) + " ₽";
                }
                else {
                    acceptBtn.IsEnabled = true;
                    totalCost.Text = "";
                }
            }
            else {
                acceptBtn.IsEnabled = false;
                totalCost.Text = "";
            }
        }

        private async void fuelVolume_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                if (acceptBtn.IsEnabled) {
                    await Accept();
                }
            }
        }

        private async void acceptBtn_Click(object sender, RoutedEventArgs e) {
            await Accept();
        }

        private async Task Accept() {
            try {
                _volume = double.Parse(fuelVolume.Text);
            } catch (Exception) {
                acceptBtn.IsEnabled = false;
                await Alert.ShowAsync(Alert.ERROR, $"Некорректное значение! ('{fuelVolume.Text}')");
                return;
            }
            if (_volume < 1) {
                await Alert.ShowAsync(Alert.ERROR, $"Некорректное значение! (разрешено от 1 до {MAX_VOLUME_ALLOWED})");
                return;
            }
            if (_volume > MAX_VOLUME_ALLOWED) {
                await Alert.ShowAsync(Alert.ERROR, $"Выбрано слишком много! (максимум {MAX_VOLUME_ALLOWED})");
                return;
            }

            double availVolume = _fuelChosen.AvailVolume;
            if (_volume > availVolume) {
                await Alert.ShowAsync(Alert.ERROR, $"К сожалению, выбранное количество недоступно! (максимум {availVolume})");
                fuelVolume.PlaceholderText = "от 1 до " + availVolume;
                return;
            }

            try {
                App.CurrPages.Main.CurrState.Reserve = new Stocks.FuelReserved.Reserve(
                    _fuelChosen,
                    _volume,
                    Math.Round(_volume * _fuelChosen.Cost, 2)
                );
            } catch (ArithmeticException) {
                await Alert.ShowAsync(Alert.ERROR, $"Извините, произошла ошибка даных...\nПопробуйте ввести значение заново.");
                fuelVolume.PlaceholderText = "от 1 до " + _fuelChosen.AvailVolume;
                return;
            }

            var reserve = App.CurrPages.Main.CurrState.Reserve;
            var answer = await Alert.AskAsync(
                Alert.CONFIRM,
                $"Вы выбрали {reserve.fuel.Origin.Name}, {reserve.volume} литров.\nИтого: {reserve.totalCost}₽"
            );
            if (answer != Alert.YES) {
                App.CurrPages.Main.CurrState.Reserve = null;  // with auto-removing
                return;
            }

            App.CurrPages.Main.SetPaymentPanel();
        }
    }
}
