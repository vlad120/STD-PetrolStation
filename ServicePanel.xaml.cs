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
        }

        private async void loadData() {
            await Stocks.LoadAsync();
            var fuel = Stocks.Fuel;

            volume92.Text = fuel.Volume92.ToString();
            volume95.Text = fuel.Volume95.ToString();
            volume98.Text = fuel.Volume98.ToString();
            volumeD.Text = fuel.VolumeD.ToString();

            cost92.Text = fuel.Cost92.ToString();
            cost95.Text = fuel.Cost95.ToString();
            cost98.Text = fuel.Cost98.ToString();
            costD.Text = fuel.CostD.ToString();
        }

        private async void acceptBtn_Click(object sender, RoutedEventArgs e) {
            TextBox[] volumeTextBoxes = { volume92, volume95, volume98, volumeD };
            TextBox[] costTextBoxes = { cost92, cost95, cost98, costD };
            int[] volumes = new int[4];
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
                    volumes[i] = int.Parse(volumeTextBoxes[i].Text);
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
                var fuel = Stocks.Fuel;
                fuel.Volume92 = volumes[0];
                fuel.Volume95 = volumes[1];
                fuel.Volume98 = volumes[2];
                fuel.VolumeD = volumes[3];
                fuel.Cost92 = costs[0];
                fuel.Cost95 = costs[1];
                fuel.Cost98 = costs[2];
                fuel.CostD = costs[3];
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
