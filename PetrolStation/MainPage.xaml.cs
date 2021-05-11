using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class MainPage : Page {

        public Panels CurrPanels { get; }
        public State CurrState { get; }

        public MainPage() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            App.CurrPages.Main = this;

            this.CurrPanels = new Panels();
            this.CurrState = new State();
            
            SetStartPanel();
        }

        public async void SetStartPanel() {
            await Stocks.LoadAsync();

            bool ok = false;
            foreach (var fuel in Stocks.FuelInfo.AllFuel) {
                if (fuel.AvailVolume >= 1) {
                    ok = true;
                    break;
                }
            }
            if (!ok) {
                SetDisabledPanel();
                return;
            }

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;

            IOMainFrame.Navigate(PanelTypes.Start);
        }

        public void SetDisabledPanel() {
            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;

            IOMainFrame.Navigate(PanelTypes.Disabled);
        }

        public void SetServicePanel() {
            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;

            IOMainFrame.Navigate(PanelTypes.Service);
        }

        public void SetVolumePanel() {
            backBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.Reserve = null;

            IOMainFrame.Navigate(PanelTypes.Volume);
        }

        public void SetPaymentPanel() {
            backBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            IOMainFrame.Navigate(PanelTypes.Payment);
        }

        public void SetFuelFillingPanel() {
            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Collapsed;

            IOMainFrame.Navigate(PanelTypes.FuelFilling);
        }

        private void backBtn_Click(object sender, RoutedEventArgs e) {
            var pt = IOMainFrame.CurrentSourcePageType;
            if (pt == PanelTypes.Volume) {
                SetStartPanel();
            }
            else if (pt == PanelTypes.Payment) {
                SetVolumePanel();
            }
            else {
                SetStartPanel();
            }
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e) {
            SetStartPanel();
        }

        private void serviceBtn_Click(object sender, RoutedEventArgs e) {
            SetServicePanel();
        }

        public class State {
            Stocks.FuelReserved.Reserve _reserve;
            public Stocks.FuelInfo.Fuel FuelChosen { get; set; }
            public Stocks.FuelReserved.Reserve Reserve {
                get {
                    return _reserve;
                }
                set {
                    if (value != _reserve) {
                        if (_reserve != null) {
                            _reserve.Remove();
                        }
                        _reserve = value;
                    }
                }
            }
        }

        public class Panels {
            public StartPanel Start { get; set; }
            public DisabledPanel Disabled { get; set; }
            public ServicePanel Service { get; set; }
            public VolumePanel Volume { get; set; }
            public PaymentPanel Payment { get; set; }
            public fuelFillingPanel FuelFilling { get; set; }
        }

        public class PanelTypes {
            public static Type Start = typeof(StartPanel);
            public static Type Disabled = typeof(DisabledPanel);
            public static Type Service = typeof(ServicePanel);
            public static Type Volume = typeof(VolumePanel);
            public static Type Payment = typeof(PaymentPanel);
            public static Type FuelFilling = typeof(fuelFillingPanel);
        }
    }
}
