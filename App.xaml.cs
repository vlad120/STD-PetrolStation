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

    public static async Task LoadAsync() {
        FuelInfo.Null();
        try {
            StorageFile file = await _localFolder.GetFileAsync(_settingsFileName);
            var data = JObject.Parse(await FileIO.ReadTextAsync(file));
            if (data.ContainsKey("fuel")) {
                foreach (JObject obj in (JArray)data["fuel"]) {
                    foreach (FuelInfo.Fuel fuel in FuelInfo.AllFuel) {
                        if (fuel.TryLoadJSON(obj)) {
                            break;
                        }
                    }
                }
            }
        } catch (Exception) { }
    }

    public static async Task SaveAsync() {
        try {
            JObject rootObj = new JObject();
            rootObj.Add("fuel", FuelInfo.AsJSON());

            StorageFile file = await _localFolder.CreateFileAsync(_settingsFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(rootObj));

            Debug.WriteLine("Stocks saved, fuel: " + FuelInfo.AsString());
        }
        catch (FileLoadException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        }
        catch (FileNotFoundException) {
            await Alert.ShowAsync(Alert.ERROR, "Произошла ошибка доступа к файлу. Настройки не сохранены!");
        }
    }

    public class FuelInfo {
        public static Fuel Fuel92 { get; } = new Fuel("92", "АИ-92");
        public static Fuel Fuel95 { get; } = new Fuel("95", "АИ-95");
        public static Fuel Fuel98 { get; } = new Fuel("98", "АИ-98");
        public static Fuel FuelD { get; } = new Fuel("D", "Дизель");

        public static Fuel[] AllFuel = new Fuel[] { Fuel92, Fuel95, Fuel98, FuelD };

        public static Fuel GetFuel(string keyName) {
            foreach (Fuel fuel in AllFuel) {
                if (fuel.KeyName == keyName) {
                    return fuel;
                }
            }
            throw new KeyNotFoundException();
        }

        public static void Null() {
            foreach (Fuel fuel in AllFuel) {
                fuel.Null();
            }
        }

        public static string AsString() {
            var sw = new StringWriter();
            int last = AllFuel.Length - 1;
            Fuel f;
            for (int i = 0; i <= last; ++i) {
                f = AllFuel[i];
                sw.Write($"{f.KeyName}={{{f.Volume}L, {f.Cost}Rub}}");
                if (i != last) {
                    sw.Write(", ");
                }
            }
            return sw.ToString();
        }

        public static JArray AsJSON() {
            JArray array = new JArray();
            foreach (Fuel fuel in AllFuel) {
                array.Add(fuel.AsJSON());
            }
            return array;
        }

        public class Fuel {
            public string KeyName { get; private set; }
            public string Name { get; private set; }
            public double Cost { get; set; } = -1d;
            public double Volume { get; set; } = 0d;

            public Fuel(string keyName, string name, double cost = 0d, double volume = 0d) {
                KeyName = keyName;
                Name = name;
                Cost = cost;
                Volume = volume;
            }

            public double AvailVolume {
                get { return Volume - FuelReserved.GetFuel(KeyName).Volume; }
            }

            public void Null() {
                Cost = -1d;
                Volume = 0;
            }

            public JObject AsJSON() {
                JObject obj = new JObject();
                obj.Add("key_name", KeyName);
                obj.Add("cost", Cost);
                obj.Add("volume", Volume);
                return obj;
            }

            public bool TryLoadJSON(JObject obj) {
                try {
                    if (obj.ContainsKey("key_name")) {
                        if ((string)obj["key_name"] != KeyName) {
                            return false;
                        }
                    }
                } catch (Exception) {
                    return false;
                }
                try {
                    if (obj.ContainsKey("cost")) Cost = (double)obj["cost"];
                }
                catch (Exception) { }
                try {
                    if (obj.ContainsKey("volume")) Volume = (double)obj["volume"];
                }
                catch (Exception) { }
                return true;
            }
        }
    }

    public class FuelReserved {

        const int RESERVE_TIME_MS = 10_000;  // 5 min (10 sec) + max 30s

        public static Fuel Fuel92 { get; } = new Fuel(FuelInfo.Fuel92);
        public static Fuel Fuel95 { get; } = new Fuel(FuelInfo.Fuel95);
        public static Fuel Fuel98 { get; } = new Fuel(FuelInfo.Fuel98);
        public static Fuel FuelD { get;} = new Fuel(FuelInfo.FuelD);

        public static Fuel[] AllFuel = new Fuel[] { Fuel92, Fuel95, Fuel98, FuelD };

        private static List<Reserve> _reserveList = new List<Reserve>();
        private static Thread _checkingThread;

        public static Fuel GetFuel(string keyName) {
            foreach (Fuel fuel in AllFuel) {
                if (fuel.KeyName == keyName) {
                    return fuel;
                }
            }
            throw new KeyNotFoundException();
        }

        public static void StartCheckingThread() {
            _checkingThread = new Thread(new ThreadStart(AutoRemoveOldReserves));
            _checkingThread.Start();
        }

        public static void AutoRemoveOldReserves() {
            while (true) {
                Thread.Sleep(30_000);  // every 30s
                while (_reserveList.Count > 0 && _reserveList[0].isExpired()) {
                    RemoveReserve(0);
                }
            }
        }

        private static void RemoveReserve(int index) {
            Reserve reserve = _reserveList[index];
            reserve.fuel.Volume -= reserve.volume;
        }

        public class Fuel {
            private double _volume;
            public string KeyName { get; private set; }
            public double Volume {
                get { return _volume; }
                set {
                    if (value < 0) _volume = 0;
                    else _volume = value;
                }
            }
            public Fuel(FuelInfo.Fuel fuel, double volume = 0d) {
                KeyName = fuel.KeyName;
                Volume = volume;
            }

            public FuelInfo.Fuel Origin {
                get { return FuelInfo.GetFuel(KeyName); }
            }
        }

        public class Reserve {
            public readonly DateTime dateCreated;
            public readonly Fuel fuel;
            public readonly double volume;
            public readonly double totalCost;

            public Reserve(FuelInfo.Fuel fuelOrigin, double volume, double totalCost) {
                if (volume < 0 || volume > fuelOrigin.Volume) {
                    throw new ArithmeticException();
                }
                this.dateCreated = DateTime.Now;
                this.fuel = GetFuel(fuelOrigin.KeyName);
                this.volume = volume;
                this.totalCost = totalCost;

                fuel.Volume += volume;
                FuelReserved._reserveList.Add(this);
                Debug.WriteLine("New reserve: " + this.ToString());
            }

            public override string ToString() {
                return $"{fuel.KeyName}={{{volume}L, {totalCost}Rub, {dateCreated}}}";
            }

            public DateTime dateEnd {
                get { return dateCreated + TimeSpan.FromMilliseconds(RESERVE_TIME_MS); }
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

            public async Task Finish() {
                await Finish(volume);
            }

            public async Task Finish(double volume) {
                var fuelOrigin = fuel.Origin;
                if (fuelOrigin.Volume > volume) {
                    fuelOrigin.Volume -= volume;
                }
                else {
                    fuelOrigin.Volume = 0d;
                }
                await Stocks.SaveAsync();
                Remove();
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
