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

            fuel92Name.Text = Stocks.FuelInfo.Fuel92.Name;
            fuel95Name.Text = Stocks.FuelInfo.Fuel95.Name;
            fuel98Name.Text = Stocks.FuelInfo.Fuel98.Name;
            fuelDName.Text = Stocks.FuelInfo.FuelD.Name;

            refreshData();
        }

        private void refreshData() {
            fuel92Volume.Text = (int)Stocks.FuelInfo.Fuel92.AvailVolume + " л";
            fuel95Volume.Text = (int)Stocks.FuelInfo.Fuel95.AvailVolume + " л";
            fuel98Volume.Text = (int)Stocks.FuelInfo.Fuel98.AvailVolume + " л";
            fuelDVolume.Text = (int)Stocks.FuelInfo.FuelD.AvailVolume + " л";

            fuel92Cost.Text = Stocks.FuelInfo.Fuel92.Cost + " руб/л";
            fuel95Cost.Text = Stocks.FuelInfo.Fuel95.Cost + " руб/л";
            fuel98Cost.Text = Stocks.FuelInfo.Fuel98.Cost + " руб/л";
            fuelDCost.Text = Stocks.FuelInfo.FuelD.Cost + " руб/л";

            if (Stocks.FuelInfo.Fuel92.AvailVolume < 1 || Stocks.FuelInfo.Fuel92.Cost < 0) {
                fuel92Btn.IsEnabled = false;
            } else fuel92Btn.IsEnabled = true;

            if (Stocks.FuelInfo.Fuel95.AvailVolume < 1 || Stocks.FuelInfo.Fuel95.Cost < 0) {
                fuel95Btn.IsEnabled = false;
            }
            else fuel95Btn.IsEnabled = true;

            if (Stocks.FuelInfo.Fuel98.AvailVolume < 1 || Stocks.FuelInfo.Fuel98.Cost < 0) {
                fuel98Btn.IsEnabled = false;
            }
            else fuel98Btn.IsEnabled = true;

            if (Stocks.FuelInfo.FuelD.AvailVolume < 1 || Stocks.FuelInfo.FuelD.Cost < 0) {
                fuelDBtn.IsEnabled = false;
            }
            else fuelDBtn.IsEnabled = true;
        }

        private void fuelBtn_Click(object sender, RoutedEventArgs e) {
            var fuelChosen = Stocks.FuelInfo.GetFuel((sender as Button).Tag as string);
            App.CurrPages.Main.CurrState.FuelChosen = fuelChosen;
            Debug.WriteLine("Selected fuel: " + fuelChosen.KeyName);
            App.CurrPages.Main.SetVolumePanel();
        }
    }
}
