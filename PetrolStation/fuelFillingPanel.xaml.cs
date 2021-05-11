using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class fuelFillingPanel : Page {

        private DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _timeOffset = new TimeSpan(0, 0, 0, 0, 50);

        private Stocks.FuelReserved.Reserve _reserve;

        public fuelFillingPanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.FuelFilling = this;

            _reserve = App.CurrPages.Main.CurrState.Reserve;

            _timer.Interval = _timeOffset;
            _timer.Tick += TimerNext;

            fuelLabel.Text = $"{_reserve.fuel.Origin.Name} ({_reserve.volume} л)";
            fillingStatus.Text = "Для начала заправки нажмите кнопку \"СТАРТ\"";

            startBtn.IsEnabled = true;
            pauseBtn.IsEnabled = false;
            stopBtn.IsEnabled = true;

            progressBar.Value = 0d;
        }

        public async void TimerNext(object sender, object e) {
            progressBar.Value += 0.5d;
            if (progressBar.Value > 99.4d) {
                _timer.Stop();
                await _reserve.Finish();
                await Alert.ShowAsync(Alert.NOTIFY, "Операция выполнена успешно!");
                App.CurrPages.Main.SetStartPanel();
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e) {
            _timer.Start();
            startBtn.IsEnabled = false;
            pauseBtn.IsEnabled = true;
            stopBtn.IsEnabled = false;
            fillingStatus.Text = "Подача топлива...";
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e) {
            _timer.Stop();
            startBtn.IsEnabled = true;
            pauseBtn.IsEnabled = false;
            stopBtn.IsEnabled = true;
            fillingStatus.Text = "Процесс приостановлен";
        }

        private async void stopBtn_Click(object sender, RoutedEventArgs e) {
            _timer.Stop();
            double k = progressBar.Value / 100d;
            double filled = Math.Round(_reserve.volume * k, 2);
            double remain = Math.Round(_reserve.totalCost * (1 - k), 2);

            await _reserve.Finish(filled);
            await Alert.ShowAsync(Alert.NOTIFY, $"Заправка завершена досрочно ({filled} из {_reserve.volume} литров)." +
                $"\nСредства в размере {remain} рублей будут возвращены на Вашу карту оплаты в течение 7 дней.");
            
            App.CurrPages.Main.SetStartPanel();
        }
    }
}
