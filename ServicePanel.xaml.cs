using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class ServicePanel : Page {

        const string ADMIN_PASSWORD = "admin";
        static Brush redBorderBrush = new SolidColorBrush(Colors.Red);
        static Brush defaultBorderBrush;

        public ServicePanel() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            App.CurrPages.Main.CurrPanels.Service = this;

            serviceContent.Visibility = Visibility.Collapsed;
            defaultBorderBrush = passwordBox.BorderBrush;

            fuel92Name.Text = Stocks.FuelInfo.Fuel92.Name + ':';
            fuel95Name.Text = Stocks.FuelInfo.Fuel95.Name + ':';
            fuel98Name.Text = Stocks.FuelInfo.Fuel98.Name + ':';
            fuelDName.Text = Stocks.FuelInfo.FuelD.Name + ':';
        }

        private async void loadData() {
            await Stocks.LoadAsync();

            volume92.Text = Stocks.FuelInfo.Fuel92.Volume.ToString();
            volume95.Text = Stocks.FuelInfo.Fuel95.Volume.ToString();
            volume98.Text = Stocks.FuelInfo.Fuel98.Volume.ToString();
            volumeD.Text = Stocks.FuelInfo.FuelD.Volume.ToString();

            cost92.Text = Stocks.FuelInfo.Fuel92.Cost.ToString();
            cost95.Text = Stocks.FuelInfo.Fuel95.Cost.ToString();
            cost98.Text = Stocks.FuelInfo.Fuel98.Cost.ToString();
            costD.Text = Stocks.FuelInfo.FuelD.Cost.ToString();
        }

        private async void acceptBtn_Click(object sender, RoutedEventArgs e) {
            TextBox[] volumeTextBoxes = { volume92, volume95, volume98, volumeD };
            TextBox[] costTextBoxes = { cost92, cost95, cost98, costD };
            double[] volumes = new double[4];
            double[] costs = new double[4];

            foreach (var box in volumeTextBoxes) {
                box.BorderBrush = defaultBorderBrush;
            }
            foreach (var box in costTextBoxes) {
                box.BorderBrush = defaultBorderBrush;
            }

            bool ok = true;
            for (int i = 0; i < 4; ++i) {
                try {
                    volumes[i] = double.Parse(volumeTextBoxes[i].Text);
                } catch (Exception) {
                    volumeTextBoxes[i].BorderBrush = redBorderBrush;
                    ok = false;
                }
            }
            for (int i = 0; i < 4; ++i) {
                try {
                    costs[i] = double.Parse(costTextBoxes[i].Text);
                } catch (Exception) {
                    costTextBoxes[i].BorderBrush = redBorderBrush;
                    ok = false;
                }
            }

            if (ok) {
                Stocks.FuelInfo.Fuel92.Volume = volumes[0];
                Stocks.FuelInfo.Fuel92.Cost = costs[0];

                Stocks.FuelInfo.Fuel95.Volume = volumes[1];
                Stocks.FuelInfo.Fuel95.Cost = costs[1];

                Stocks.FuelInfo.Fuel98.Volume = volumes[2];
                Stocks.FuelInfo.Fuel98.Cost = costs[2];

                Stocks.FuelInfo.FuelD.Volume = volumes[3];
                Stocks.FuelInfo.FuelD.Cost = costs[3];

                await Stocks.SaveAsync();
                await Alert.ShowAsync(Alert.NOTIFY, "Данные сохранены!");
            }
        }

        private void passwordBox_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args) {
            if (sender.Password.Equals(ADMIN_PASSWORD)) {
                passwordBox.Visibility = Visibility.Collapsed;
                serviceContent.Visibility = Visibility.Visible;
                loadData();
            }
        }
    }
}
