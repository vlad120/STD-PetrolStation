using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Threading;

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

            Stocks.FuelReserved.StartCheckingThread();
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


public class Stocks {
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
    }

    public static async Task SaveAsync() {
        try {
            StorageFile file = await _localFolder.CreateFileAsync(_settingsFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(Fuel));
            Debug.WriteLine("Stocks saved, fuel: " + Fuel.ToString());
        } catch (FileLoadException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        } catch (FileNotFoundException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        }
    }

    public class FuelInfo {
        public double Volume92 { get; set; } = 0d;
        public double Volume95 { get; set; } = 0d;
        public double Volume98 { get; set; } = 0d;
        public double VolumeD { get; set; } = 0d;
        public double Cost92 { get; set; } = 0d;
        public double Cost95 { get; set; } = 0d;
        public double Cost98 { get; set; } = 0d;
        public double CostD { get; set; } = 0d;

        public override string ToString() {
            return $"92=({Volume92}, {Cost92}); 95=({Volume95}, {Cost95}); " +
                $"98=({Volume98}, {Cost98}); D=({VolumeD}, {CostD});";
        }

        public double getAvailVolume(string t) {
            switch (t) {
                case "92": return Fuel.Volume92 - FuelReserved.Volume92;
                case "95": return Fuel.Volume95 - FuelReserved.Volume95;
                case "98": return Fuel.Volume98 - FuelReserved.Volume98;
                case "D": return Fuel.VolumeD - FuelReserved.VolumeD;
                default: return 0d;
            }
        }

        public double getCost(string t) {
            switch (t) {
                case "92": return Fuel.Cost92;
                case "95": return Fuel.Cost95;
                case "98": return Fuel.Cost98;
                case "D": return Fuel.CostD;
                default: return 0d;
            }
        }
    }

    public class FuelReserved {
        public static double Volume92 { get; private set; } = 0d;
        public static double Volume95 { get; private set; } = 0d;
        public static double Volume98 { get; private set; } = 0d;
        public static double VolumeD { get; private set; } = 0d;

        private static List<Reserve> _reserveList = new List<Reserve>();
        private static Thread _checkingThread;

        const int RESERVE_TIME_MS = 10_000;  // 5 min (10 sec) + max 30s

        public static void StartCheckingThread() {
            _checkingThread = new Thread(new ThreadStart(AutoRemoveOldReserves));
            _checkingThread.Start();
        }

        public static void AutoRemoveOldReserves() {
            DateTime now;
            while (true) {
                Thread.Sleep(30_000);  // every 30s
                now = DateTime.Now;
                while (Stocks.FuelReserved._reserveList.Count > 0 && Stocks.FuelReserved._reserveList[0].isExpired()) {
                    RemoveReserve(0);
                }
            }
        }

        private static void RemoveReserve(int index) {
            Reserve reserve = _reserveList[index];
            double vol = reserve.volume;
            switch (reserve.t) {
                case "92": {
                    Fuel.Volume92 += vol;
                    Volume92 -= vol;
                    if (Fuel.Volume92 < 0) Fuel.Volume92 = 0;
                    break;
                }
                case "95": {
                    Fuel.Volume95 += vol;
                    Volume95 -= vol;
                    if (Fuel.Volume95 < 0) Fuel.Volume95 = 0;
                    break;
                }
                case "98": {
                    Fuel.Volume98 += vol;
                    Volume98 -= vol;
                    if (Fuel.Volume98 < 0) Fuel.Volume98 = 0;
                    break;
                }
                case "D": {
                    Fuel.VolumeD += vol;
                    VolumeD -= vol;
                    if (Fuel.VolumeD < 0) Fuel.VolumeD = 0;
                    break;
                }
            }
        }

        public class Reserve {
            public readonly DateTime dateCreated;
            public readonly string t;
            public readonly double volume;
            public readonly double totalCost;

            public Reserve(string t, double volume, double totalCost) {
                switch (t) {
                    case "92": {
                        if (volume < 0 || volume > Fuel.Volume92) {
                            throw new ArithmeticException();
                        }
                        Fuel.Volume92 -= volume;
                        FuelReserved.Volume92 += volume;
                        break;
                    }
                    case "95": {
                        if (volume < 0 || volume > Fuel.Volume95) {
                            throw new ArithmeticException();
                        }
                        Fuel.Volume95 -= volume;
                        FuelReserved.Volume95 += volume;
                        break;
                    }
                    case "98": {
                        if (volume < 0 || volume > Fuel.Volume98) {
                            throw new ArithmeticException();
                        }
                        Fuel.Volume98 -= volume;
                        FuelReserved.Volume98 += volume;
                        break;
                    }
                    case "D": {
                        if (volume < 0 || volume > Fuel.VolumeD) {
                            throw new ArithmeticException();
                        }
                        Fuel.VolumeD -= volume;
                        FuelReserved.VolumeD += volume;
                        break;
                    }
                }
                this.dateCreated = DateTime.Now;
                this.t = t;
                this.volume = volume;
                this.totalCost = totalCost;
                FuelReserved._reserveList.Add(this);
                Debug.WriteLine("New reserve: " + this.ToString());
            }

            public override string ToString() {
                return $"{dateCreated}, {t}, {volume}L, {totalCost}Rub";
            }

            public DateTime dateEnd {
                get {
                    return dateCreated + TimeSpan.FromMilliseconds(RESERVE_TIME_MS);
                }
            }

            public bool isExpired() {
                return (dateEnd < DateTime.Now);
            }

            public void Remove() {
                int index = FuelReserved._reserveList.IndexOf(this);
                if (index != -1) {
                    FuelReserved.RemoveReserve(index);
                }
            }
        }
    }
}


public class Alert {
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
