using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PetrolStation {
    sealed partial class App : Application {

        private Pages _currPages;
        private Frame _rootFrame;
        private bool _windowSizeError = false;

        public App() {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this._currPages = new Pages();
        }

        public static Pages CurrPages {
            get {
                return ((App)App.Current)._currPages;
            }
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs e) {
            this._rootFrame = Window.Current.Content as Frame;

            if (_rootFrame == null) {
                _rootFrame = new Frame();
                _rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                Window.Current.Content = _rootFrame;
            }

            if (e.PrelaunchActivated == false) {
                if (_rootFrame.Content == null) {
                    _rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }

            _rootFrame.SizeChanged += OnSizeChanged;

            ApplicationView.PreferredLaunchViewSize = new Size(900, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            if (e.NewSize.Width < 900 || e.NewSize.Height < 600) {
                if (!_windowSizeError) {
                    (Window.Current.Content as Frame).Navigate(typeof(WindowSizeError));
                    _windowSizeError = true;
                }
            }
            else if (_windowSizeError) {
                (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                _windowSizeError = false;
            }

        }

        public class Pages {
            public MainPage Main { get; set; }
            public WindowSizeError WinSizeErr { get; set; }
        }
    }
}


class Stocks {
    private static StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
    private static string _settingsFileName = "stocks.json";

    public static FuelInfo Fuel { get; private set; }

    public static async Task LoadAsync() {
        Fuel = null;
        try {
            StorageFile file = await _localFolder.GetFileAsync(_settingsFileName);
            Fuel = JsonConvert.DeserializeObject<FuelInfo>(await FileIO.ReadTextAsync(file));
        } catch (Exception) { }

        if (Fuel == null) {
            Fuel = new FuelInfo();
        }
        System.Diagnostics.Debug.WriteLine(_localFolder);
    }

    public static async Task SaveAsync() {
        try {
            StorageFile file = await _localFolder.CreateFileAsync(_settingsFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(Fuel));
        } catch (FileLoadException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        } catch (FileNotFoundException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        }
    }

    public class FuelInfo {
        public int Volume92 { get; set; } = 0;
        public int Volume95 { get; set; } = 0;
        public int Volume98 { get; set; } = 0;
        public int Volume100 { get; set; } = 0;
        public double Cost92 { get; set; } = 0d;
        public double Cost95 { get; set; } = 0d;
        public double Cost98 { get; set; } = 0d;
        public double Cost100 { get; set; } = 0d;
    }
}


class Alert {
    private static Queue<ContentDialog> _queue = new Queue<ContentDialog>();

    public const string NOTIFY = "Уведомление";
    public const string ERROR = "Ошибка!";
    public const string ATTENTION = "Внимение!";
    public const string CONFIRM = "Подтверждение";

    public const AskResult YES = AskResult.YES;
    public const AskResult NO = AskResult.NO;
    public const AskResult CANCEL = AskResult.CANCEL;

    public static async Task ShowAsync(string title, string content) {
        foreach (ContentDialog d in _queue) {
            if ((string)d.Title == title && (string)d.Content == content) {
                return;
            }
        }
        ContentDialog dialog = new ContentDialog() {
            Title = title,
            Content = content,
            CloseButtonText = "Ок",
            Tag = 0,
        };
        dialog.Closed += DialogClosed;
        _queue.Enqueue(dialog);
        if (_queue.Count == 1) {
            await _queue.Peek().ShowAsync();
        }
    }

    public static async Task<AskResult> AskAsync(string title, string content, bool cancelBtn = false) {
        ContentDialog dialog = new ContentDialog() {
            Title = title,
            Content = content,
            PrimaryButtonText = "Да",
            SecondaryButtonText = "Нет",
            Tag = 1,
        };
        if (cancelBtn) {
            dialog.CloseButtonText = "Отмена";
        }
        dialog.Closed += DialogClosed;
        _queue.Enqueue(dialog);
        while (_queue.Peek() != dialog) {
            await Task.Delay(500); ;  // 0.5s
        }
        ContentDialogResult answer = await dialog.ShowAsync();
        switch (answer) {
            case ContentDialogResult.Primary: return YES;
            case ContentDialogResult.Secondary: return NO;
            default: return CANCEL;
        }
    }

    private static void DialogClosed(ContentDialog dialog, ContentDialogClosedEventArgs args) {
        _queue.Dequeue();
        if (_queue.Count > 0) {
            if ((int)_queue.Peek().Tag == 0) {  // 0 - Show, 1 - Ask
                _ = _queue.Peek().ShowAsync();
            }
        }
    }

    public enum AskResult {
        YES,
        NO,
        CANCEL,
    }
}
