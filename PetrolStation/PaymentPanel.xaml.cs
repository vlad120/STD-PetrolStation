using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class PaymentPanel : Page {

        private DispatcherTimer _timer = new DispatcherTimer();
        private TimeSpan _timeLeft = new TimeSpan(0, 1, 0);
        private TimeSpan _timeOffset = new TimeSpan(0, 0, 1);
        private TimeSpan _timeEnd = new TimeSpan(0, 0, 0);

        private Stocks.FuelReserved.Reserve _reserve;
        private double _balance;

        public PaymentPanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Payment = this;

            _reserve = App.CurrPages.Main.CurrState.Reserve;

            orderStruct.Text = $"{_reserve.fuel.Origin.Name} ({_reserve.volume} л)";
            totalCost.Text = _reserve.totalCost + " руб.";

            paymentTimer.Text = "";
            _timer.Interval = _timeOffset;
            _timer.Tick += TimerNext;
            _timer.Start();
        }

        public async void TimerNext(object sender, object e) {
            if (!this.IsLoaded) {
                _timer.Stop();
            }
            if (_timeLeft == _timeEnd) {
                _timer.Stop();
                await Alert.ShowAsync(Alert.ATTENTION, "Время оплаты истекло!");
                App.CurrPages.Main.SetStartPanel();
                return;
            }
            paymentTimer.Text = _timeLeft.Minutes.ToString().PadLeft(2, '0') + ':' +
                _timeLeft.Seconds.ToString().PadLeft(2, '0');
            _timeLeft -= _timeOffset;
        }

        private async void Accept() {
            if (!double.TryParse(cardBalance.Text, out _balance)) {
                acceptBtn.IsEnabled = false;
                await Alert.ShowAsync(Alert.ERROR, $"Некорректное значение баланса! ('{cardBalance.Text}')");
                return;
            }
            if (_balance < _reserve.totalCost) {
                acceptBtn.IsEnabled = false;
                await Alert.ShowAsync(Alert.ERROR, $"На карте недостаточно средств!");
                return;
            }
            _timer.Stop();
            await Alert.ShowAsync(Alert.NOTIFY, $"Оплата произведена успешно.\nОстаток на карте: " + (_balance - _reserve.totalCost));
            App.CurrPages.Main.SetFuelFillingPanel();
        }

        private void cardBalance_TextChanged(object sender, TextChangedEventArgs e) {
            string text = cardBalance.Text;
            if (double.TryParse(text, out _balance)) {
                acceptBtn.IsEnabled = true;
                int index = text.IndexOf(',');
                if (index != -1 && text.Length - index > 3) {
                    cardBalance.Text = Math.Round(_balance, 2).ToString();
                    cardBalance.SelectionStart = text.Length;
                }
            }
            else {
                acceptBtn.IsEnabled = false;
            }
        }

        private void cardBalance_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                if (acceptBtn.IsEnabled) {
                    Accept();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Accept();
        }
    }
}
