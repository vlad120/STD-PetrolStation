using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class VolumePanel : Page {
        const double MAX_VOLUME_ALLOWED = 100d;
        double _num;
        string _fuelChosen;

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

            _fuelChosen = App.CurrPages.Main.CurrState.FuelChosen;

            double maxVolume = getMaxVolume();
            fuelVolume.PlaceholderText = "от 1 до " + maxVolume;
            fuelVolume.MaxLength = maxVolume.ToString().Length + 3;  // x,yy

            totalCost.Text = "";
        }

        private double getMaxVolume() {
            double availVolume = Stocks.Fuel.getAvailVolume(_fuelChosen);
            if (availVolume > MAX_VOLUME_ALLOWED) {
                return MAX_VOLUME_ALLOWED;
            }
            return availVolume;
        }

        private void fuelVolume_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            if (double.TryParse(sender.Text, out _num)) {
                if (_num < 1) {
                    acceptBtn.IsEnabled = false;
                    totalCost.Text = "";
                }
                else if (_num <= getMaxVolume()) {
                    acceptBtn.IsEnabled = true;
                    totalCost.Text = Math.Round(_num * Stocks.Fuel.getCost(_fuelChosen), 2) + " ₽";
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

            double availVolume = Stocks.Fuel.getAvailVolume(_fuelChosen);
            if (_num > availVolume) {
                await Alert.ShowAsync(Alert.ERROR, $"К сожалению, выбранное количество недоступно! (максимум {availVolume})");
                fuelVolume.PlaceholderText = "от 1 до " + availVolume;
                return;
            }

            try {
                App.CurrPages.Main.CurrState.Reserve = new Stocks.FuelReserved.Reserve(
                    App.CurrPages.Main.CurrState.FuelChosen,
                    _num,
                    Math.Round(_num * Stocks.Fuel.getCost(_fuelChosen), 2)
                );
            } catch (ArithmeticException) {
                await Alert.ShowAsync(Alert.ERROR, $"Извините, произошла ошибка даных...\nПопробуйте ввести значение заново.");
                fuelVolume.PlaceholderText = "от 1 до " + Stocks.Fuel.getAvailVolume(_fuelChosen);
                return;
            }

            App.CurrPages.Main.SetPaymentPanel();
        }
    }
}
