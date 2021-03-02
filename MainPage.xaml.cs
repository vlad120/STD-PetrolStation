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
            var fuel = Stocks.Fuel;

            if (fuel.Volume92 < 1 && fuel.Volume95 < 1 &&
                fuel.Volume98 < 1 && fuel.VolumeD < 1) {
                SetDisabledPanel();
                return;
            }

            IOMainFrame.Navigate(PanelTypes.Start);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;
        }

        public void SetDisabledPanel() {
            IOMainFrame.Navigate(PanelTypes.Disabled);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;
        }

        public void SetServicePanel() {
            IOMainFrame.Navigate(PanelTypes.Service);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.FuelChosen = null;
            CurrState.Reserve = null;
        }

        public void SetVolumePanel() {
            IOMainFrame.Navigate(PanelTypes.Volume);

            backBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.Reserve = null;
        }

        public void SetPaymentPanel() {
            IOMainFrame.Navigate(PanelTypes.Payment);

            backBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;
        }

        private void backBtn_Click(object sender, RoutedEventArgs e) {
            var pt = IOMainFrame.CurrentSourcePageType;
            if (pt == PanelTypes.Volume) {
                SetStartPanel();
            }
            else if (pt == PanelTypes.Payment) {
                SetVolumePanel();
            }
            // ...
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
            public string FuelChosen { get; set; }
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
            // ...
        }

        public class PanelTypes {
            public static Type Start = typeof(StartPanel);
            public static Type Disabled = typeof(DisabledPanel);
            public static Type Service = typeof(ServicePanel);
            public static Type Volume = typeof(VolumePanel);
            public static Type Payment = typeof(PaymentPanel);
            // ...
        }
    }
}
