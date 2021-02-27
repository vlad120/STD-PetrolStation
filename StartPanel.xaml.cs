using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class StartPanel : Page {
        public StartPanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Start = this;

            check();
        }

        private void check() {

        }

        private void fuelBtn_Click(object sender, RoutedEventArgs e) {

        }
    }
}
