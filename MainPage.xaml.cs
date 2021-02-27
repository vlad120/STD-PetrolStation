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
        }

        public void SetPanel(System.Type typeOfPage) {
            IOMainFrame.Navigate(typeOfPage);
        }

        public void SetPrevPanel() {
            try {
                IOMainFrame.GoBack();
            } catch (COMException) {}
        }

        public void ClearNavigateCache() {
            int cs = IOMainFrame.CacheSize;
            IOMainFrame.CacheSize = 0;
            IOMainFrame.CacheSize = cs;
        }

        private void serviceBtn_Click(object sender, RoutedEventArgs e) {

        }

        private void backBtn_Click(object sender, RoutedEventArgs e) {
            SetPrevPanel();
        }

        private void homeBtn_Click(object sender, RoutedEventArgs e) {
            SetPanel(typeof(StartPanel));
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
            // ...
        }
    }
}
