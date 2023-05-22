using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoEditor.Model;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VideoEditor.ViewModel
{
    internal sealed class TrimViewModel : BaseViewModel
    {
        public string FilePath { get; set; }
        CancellationTokenSource cancellationTokenSource;

        public double Progress { get; set; }

        public string FileName { get; set; }
        public ICommand OpenFilePickerCommand { get; set; }
        public ICommand TrimCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public Logger Logger { get; set; }
        IHelper helper;
        public MediaElement Media { get; set; }
        public MediaFile MediaFile { get; set; }
        public RangeSlider RangeSlider { get; set; }
        public WrapperFunctions Wrapper { get; set; }
        public ClientSideFunctions Client { get; set; }
        public ScrollView ScrollView { get; set; }
        public Label ScrollViewLabel { get; set; }

        /// <summary>
        /// Vágás nézetmodell. Bemeneti értékként megkapja a médiaelemet, a tartománycsúszkát és a naplózó ablakhoz tartozó szövegdobozokat és scrollview-t.
        /// A támogatott operációs rendszerek alapján eldönti, hogy modell-ként ClientSideFunctions-t (iOS, Android) vagy WrapperFunctions-t (UWP) használ.
        /// </summary>
        public TrimViewModel(MediaElement mediaElement, RangeSlider trimRangeSlider, ScrollView scrollView, Label scrollViewLabel, Label labelLogText)
        {
            labelLogText.Text = "";
            ScrollViewLabel = scrollViewLabel;
            ScrollView = scrollView;
            ScrollViewLabel.PropertyChanged += ScrollViewLabel_PropertyChanged;
            Logger = Logger.Instance;
            Media = mediaElement;
            RangeSlider = trimRangeSlider;
            RangeSlider.IsEnabled = false;
            RangeSlider.IsVisible = false;

            Media.SeekCompleted += Media_SeekCompleted;

            OpenFilePickerCommand = new RelayCommand(OpenFilePicker);
            TrimCommand = new RelayCommand(Trim);
            CancelCommand = new RelayCommand(Cancel);

            if (Device.RuntimePlatform == Device.UWP)
            {
                Wrapper = WrapperFunctions.Instance;
            }
            else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                Client = ClientSideFunctions.Instance;
            }
            else
            {
                Log("Operating system not supported.");
                Application.Current.MainPage = new VideoEditor.View.OperatingSystemNotSupportedPage();
            }
        }

        /// <summary>
        /// Naplóérték frissülés esetén a naplózó ablak aljára görget.
        /// </summary>
        private async void ScrollViewLabel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Task.Delay(10);
            await ScrollView.ScrollToAsync(0, ScrollViewLabel.Height, true);
        }

        /// <summary>
        /// A cancellationtokensource értékét nullázzuk.
        /// </summary>
        private void Cancel()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                Log("Operation cancelled");
                cancellationTokenSource = null;
            }
            else
            {
                Log("You haven't started any operation");
            }
        }

        /// <summary>
        /// Implementáció közbeni kisérleti metódus melynek hibája az, hogy túl sokszor lenne meghívva, blokkolva a végrehajtást.
        /// </summary>
        private void Media_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Media.CurrentState == MediaElementState.Paused)
            {
                if (RangeSlider.LowerValue < RangeSlider.UpperValue)
                    RangeSlider.LowerValue = Media.Position.TotalSeconds;
            }
        }

        /// <summary>
        /// Médiaelem seek esetén tartománycsúszka alsó értékének szinkronizációja.
        /// </summary>
        private void Media_SeekCompleted(object sender, EventArgs e)
        {
            if (RangeSlider.LowerValue < RangeSlider.UpperValue)
            {
                if ((sender as MediaElement).Position.TotalSeconds < RangeSlider.UpperValue)
                {
                    RangeSlider.LowerValue = Media.Position.TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Vágás parancshoz szükséges feltételek ellenőrzése és vágás végrehajtása.
        /// </summary>
        private async void Trim()
        {
            if (cancellationTokenSource != null)
            {
                Log("Conversion is already in progress...");
                if (Device.RuntimePlatform == Device.UWP)
                {
                    Log("Wait for the conversion to finish or cancel the conversion.");
                }
                if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    Log("Wait for the server to respond or cancel the conversion request.");
                }
                return;
            }
            if (FileName == null || FilePath == null)
            {
                Log($"You did not set a file to trim.");
                return;
            }
            if (RangeSlider.LowerValue == RangeSlider.UpperValue)
            {
                Log("On the rangeslider upper and lower value can't be the same.");
                return;
            }

            Log($"Trimming {FileName} from {RangeSlider.LowerValue}, to {RangeSlider.UpperValue}");
            Progress = 0;
            cancellationTokenSource = new CancellationTokenSource();

            #region conversion
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (!WrapperFunctions.FFmpegIsDownloaded)
                {
                    Log("FFmpeg is not downloaded yet");
                    cancellationTokenSource = null;
                    return;
                }

                string inputfilename = Path.GetFileName(FilePath);
                string inputFolderName = Path.GetDirectoryName(FilePath);

                string outputfilename = $"{inputFolderName}\\trimmed_{inputfilename}";

                try
                {
                    WrapperFunctions.Conversion.OnProgress += (sender, args) =>
                    {
                        Progress = args.Duration.TotalSeconds / args.TotalLength.TotalSeconds;
                    };
                    await WrapperFunctions.TrimVideo(FilePath, outputfilename, RangeSlider.LowerValue, RangeSlider.UpperValue,
                        cancellationTokenSource.Token);
                    Progress = 1;
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }
                if (cancellationTokenSource != null)
                {
                    await CopyFileTo(outputfilename, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        $"trimmed.mp4"));
                    cancellationTokenSource = null;
                }
            }
            else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                try
                {
                    var current = Connectivity.NetworkAccess;

                    if (current == NetworkAccess.Internet)
                    {
                        await ClientSideFunctions.TrimVideo(MediaFile, RangeSlider.LowerValue, RangeSlider.UpperValue,
                            cancellationTokenSource.Token);
                    }
                    else
                    {
                        Application.Current.MainPage = new VideoEditor.View.NoInternetPage();
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                }
                Progress = 1;
            }
            #endregion
            cancellationTokenSource = null;

            Log("Trimming is done");
        }

        /// <summary>
        /// Kinaplóz string üzenetet a jelenlegi dátummal és idővel.
        /// </summary>
        public void Log(string message)
        {
            var longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Logger.LogText.Append($"\n{longtimestr} - {message}");
            Logger.OnPropertyChanged(nameof(Logger.LogText));
        }

        /// <summary>
        /// UWP platformon másolás megvalósítása.
        /// </summary>
        public async Task CopyFileTo(FileResult file, string destination)
        {
            try
            {
                Stream stream = await file.OpenReadAsync();

                using (FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                    stream.CopyTo(fileStream);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Android és iOS platformokon másolás megvalósítás.
        /// </summary>
        public async Task CopyFileTo(string file, string destination)
        {
            helper = DependencyService.Get<IHelper>();
            await helper.copyFile(file, destination);
        }

        /// <summary>
        /// Fájl kiválasztása és a szükséges adatok nézetben megjelenítése, médiaelem, tartománycsúszka
        /// </summary>
        private async void OpenFilePicker()
        {
            try
            {

                if (Device.RuntimePlatform == Device.UWP)
                {
                    FileResult file = await FilePicker.PickAsync(
                        new PickOptions
                        {
                            FileTypes = FilePickerFileType.Videos,
                            PickerTitle = "Pick a video file"
                        }
                        );
                    if (file == null)
                    {
                        return;
                    }
                    string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        $"{WrapperFunctions.RandomString(7)}_{file.FileName}");
                    try
                    {
                        await CopyFileTo(file, fileName);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                        return;
                    }
                    FilePath = fileName;
                    FileName = file.FileName;
                    Log($"File chosen to be trimmed is: {file.FileName}");
                    Log($"Path: {FilePath}");
                }

                if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    MediaFile = await CrossMedia.Current.PickVideoAsync();
                    if (MediaFile == null)
                    {
                        return;
                    }
                    FilePath = MediaFile.Path;
                    FileName = Path.GetFileName(MediaFile.Path);
                    Log($"File chosen to be trimmed is: {FilePath}");
                    Log($"Path: {FilePath}");
                }

                //Waiting because the binding isn't set for another X miliseconds
                while (Media.Duration == null)
                {
                    await Task.Delay(100);
                }
                RangeSlider.IsEnabled = true;
                RangeSlider.IsVisible = true;
                RangeSlider.MinimumValue = 0;
                RangeSlider.LowerValue = 0;
                RangeSlider.MaximumValue = Media.Duration.Value.TotalSeconds;
                RangeSlider.UpperValue = RangeSlider.MaximumValue;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }
    }
}
