using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace PetrolStation {
    public sealed partial class WindowSizeError : Page {
        public WindowSizeError() {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            App.CurrPages.WinSizeErr = this;
        }
    }
}
