using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoEditor.Model;
using Xabe.FFmpeg;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace VideoEditor.ViewModel
{
    internal sealed class EffectViewModel : BaseViewModel
    {
        public string FilePath { get; set; }
        public Logger Logger { get; set; }
        public string Video { get; set; }
        public MediaFile VideoFile { get; set; }
        public ICommand OpenVideoPickerCommand { get; set; }
        public ICommand AddEffectToVideoCommand { get; set; }
        public WrapperFunctions Wrapper { get; set; }
        public ClientSideFunctions Client { get; set; }
        public RangeSlider EffectRangeSlider { get; set; }
        public Xamarin.Forms.Picker PickerInstance { get; set; }
        public double Progress { get; set; }
        public MediaElement Media { get; set; }

        public ICommand CancelCommand { get; set; }
        CancellationTokenSource cancellationTokenSource;
        public ScrollView ScrollView { get; set; }
        public Label ScrollViewLabel { get; set; }

        IHelper helper;

        /// <summary>
        /// Hatás hozzáadása nézetmodell. Bemeneti értékként megkapja a médiaelemet, a lenyíló listát, a tartománycsúszkát és a naplózó ablakhoz tartozó szövegdobozokat és scrollview-t.
        /// A támogatott operációs rendszerek alapján eldönti, hogy modell-ként ClientSideFunctions-t (iOS, Android) vagy WrapperFunctions-t (UWP) használ.
        /// </summary>
        public EffectViewModel(RangeSlider effectRangeSlider, Xamarin.Forms.Picker pickerInstance,
            MediaElement mediaElement, Label labelLogText, ScrollView scrollView, Label scrollViewLabel)
        {
            ScrollView = scrollView;
            ScrollViewLabel = scrollViewLabel;
            ScrollViewLabel.PropertyChanged += ScrollViewLabel_PropertyChanged;

            labelLogText.Text = "";
            PickerInstance = pickerInstance;
            PickerInstance.SelectedIndex = new Random().Next(PickerInstance.Items.Count);
            EffectRangeSlider = effectRangeSlider;
            EffectRangeSlider.IsEnabled = false;
            EffectRangeSlider.IsVisible = false;

            CancelCommand = new RelayCommand(Cancel);

            Logger = Logger.Instance;
            OpenVideoPickerCommand = new RelayCommand(OpenVideoPicker);
            AddEffectToVideoCommand = new RelayCommand(AddEffectToVideo);

            Media = mediaElement;
            Media.SeekCompleted += Media_SeekCompleted;

            if (Device.RuntimePlatform == Device.UWP)
            {
                Wrapper = WrapperFunctions.Instance;
            }
            else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                Client = ClientSideFunctions.Instance;
                helper = DependencyService.Get<IHelper>();
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
        private async void ScrollViewLabel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Task.Delay(10);
            await ScrollView.ScrollToAsync(0, ScrollViewLabel.Height, true);
        }

        /// <summary>
        /// Médiaelem seek esetén tartománycsúszka alsó értékének szinkronizációja.
        /// </summary>
        private void Media_SeekCompleted(object sender, EventArgs e)
        {
            if (EffectRangeSlider.LowerValue < EffectRangeSlider.UpperValue)
            {
                if ((sender as MediaElement).Position.TotalSeconds < EffectRangeSlider.UpperValue)
                {
                    EffectRangeSlider.LowerValue = Media.Position.TotalSeconds;
                }
            }
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
        /// Videófájl kiválasztása és a szükséges adatok nézetben megjelenítése, médiaelem, tartománycsúszka
        /// </summary>
        private async void OpenVideoPicker()
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

                    Video = file.FileName;
                    FilePath = fileName;
                }
                else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    VideoFile = await CrossMedia.Current.PickVideoAsync();
                    if (VideoFile == null)
                    {
                        return;
                    }
                    Video = Path.GetFileName(VideoFile.Path);
                    FilePath = VideoFile.Path;
                }

                while (Media.Duration == null)
                {
                    await Task.Delay(100);
                }
                double videoMaximumValue = Media.Duration.Value.TotalSeconds;

                EffectRangeSlider.IsEnabled = true;
                EffectRangeSlider.IsVisible = true;
                EffectRangeSlider.MaximumValue = videoMaximumValue;
                EffectRangeSlider.UpperValue = videoMaximumValue;
                EffectRangeSlider.LowerValue = 0;
                Log("Loading video...");
                Log("Done");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// A hatás hozzáadása videóhoz parancshoz szükséges feltételek ellenőrzése és a hatás hozzáadásának végrehajtása.
        /// </summary>
        private async void AddEffectToVideo()
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
            if (PickerInstance.SelectedItem == null)
            {
                Log("You haven't selected an effect.");
                return;
            }
            if (FilePath == null)
            {
                Log("You haven't selected a video.");
                return;
            }
            Log("Adding effect to video...");

            #region conversion
            cancellationTokenSource = new CancellationTokenSource();
            Progress = 0;
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (!WrapperFunctions.FFmpegIsDownloaded)
                {
                    Log("FFmpeg is not downloaded yet");
                    cancellationTokenSource = null;
                    return;
                }
                WrapperFunctions.Conversion = FFmpeg.Conversions.New();
                string inputFolderName = Path.GetDirectoryName(FilePath);

                string outputfilename = $"{inputFolderName}\\{WrapperFunctions.RandomString(10)}.mp4";

                try
                {
                    WrapperFunctions.Conversion.OnProgress += (sender, args) =>
                    {
                        Progress = args.Duration.TotalSeconds / args.TotalLength.TotalSeconds;
                    };
                    await WrapperFunctions.AddEffectToVideo(
                        PickerInstance.SelectedItem.ToString(),
                        FilePath,
                        outputfilename,
                        EffectRangeSlider.LowerValue,
                        EffectRangeSlider.UpperValue,
                        cancellationTokenSource.Token);
                    Progress = 1;
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Log(ex.Message);
                    return;
                }
                if (cancellationTokenSource != null)
                {
                    await CopyFileTo(outputfilename, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), $"effect.mp4"));
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
                        await ClientSideFunctions.AddEffectToVideo(
                            PickerInstance.SelectedItem.ToString(),
                            VideoFile,
                            EffectRangeSlider.LowerValue,
                            EffectRangeSlider.UpperValue,
                            cancellationTokenSource.Token
                        );
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

            Log("Done");
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
    }
}
