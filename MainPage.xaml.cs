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
                fuel.Volume98 < 1 && fuel.Volume100 < 1) {
                SetDisabledPanel();
                return;
            }

            IOMainFrame.Navigate(PanelTypes.Start);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.VolumeChosen = 0;
            CurrState.TotalCost = 0;
        }

        public void SetDisabledPanel() {
            IOMainFrame.Navigate(PanelTypes.Disabled);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = false;
            serviceBtn.Visibility = Visibility.Visible;

            CurrState.FuelChosen = null;
            CurrState.VolumeChosen = 0;
            CurrState.TotalCost = 0;
        }

        public void SetServicePanel() {
            IOMainFrame.Navigate(PanelTypes.Service);

            backBtn.IsEnabled = false;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.FuelChosen = null;
            CurrState.VolumeChosen = 0;
            CurrState.TotalCost = 0;
        }

        public void SetVolumePanel() {
            IOMainFrame.Navigate(PanelTypes.Volume);

            backBtn.IsEnabled = true;
            homeBtn.IsEnabled = true;
            serviceBtn.Visibility = Visibility.Collapsed;

            CurrState.VolumeChosen = 0;
            CurrState.TotalCost = 0;
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
            public string FuelChosen { get; set; }
            public int VolumeChosen { get; set; }
            public double TotalCost { get; set; } 

            public void Clear() {
                FuelChosen = null;
                VolumeChosen = 0;
                TotalCost = 0d;
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
