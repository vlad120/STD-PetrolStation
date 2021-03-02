using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class StartPanel : Page {
        public StartPanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Start = this;

            refreshData();
        }

        private void refreshData() {
            var fuel = Stocks.Fuel;

            fuel92VolumeBlock.Text = fuel.Volume92 - Stocks.FuelReserved.Volume92 + " л";
            fuel95VolumeBlock.Text = fuel.Volume95 - Stocks.FuelReserved.Volume95 + " л";
            fuel98VolumeBlock.Text = fuel.Volume98 - Stocks.FuelReserved.Volume98 + " л";
            fuelDVolumeBlock.Text = fuel.VolumeD - Stocks.FuelReserved.VolumeD + " л";

            fuel92CostBlock.Text = fuel.Cost92 + " руб/л";
            fuel95CostBlock.Text = fuel.Cost95 + " руб/л";
            fuel98CostBlock.Text = fuel.Cost98 + " руб/л";
            fuelDCostBlock.Text = fuel.CostD + " руб/л";

            if (fuel.Volume92 < 1) fuel92Btn.IsEnabled = false;
            else fuel92Btn.IsEnabled = true;

            if (fuel.Volume95 < 1) fuel95Btn.IsEnabled = false;
            else fuel95Btn.IsEnabled = true;

            if (fuel.Volume98 < 1) fuel98Btn.IsEnabled = false;
            else fuel98Btn.IsEnabled = true;

            if (fuel.VolumeD < 1) fuelDBtn.IsEnabled = false;
            else fuelDBtn.IsEnabled = true;
        }

        private void fuelBtn_Click(object sender, RoutedEventArgs e) {
            string fuelChosen = (sender as Button).Tag as string;
            App.CurrPages.Main.CurrState.FuelChosen = fuelChosen;
            Debug.WriteLine("Selected fuel: " + fuelChosen);
            App.CurrPages.Main.SetVolumePanel();
        }
    }
}
