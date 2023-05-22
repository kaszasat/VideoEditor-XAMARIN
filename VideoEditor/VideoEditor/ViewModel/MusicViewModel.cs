using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
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
    internal sealed class MusicViewModel : BaseViewModel
    {
        public string VideoFilePath { get; set; }
        public string AudioFilePath { get; set; }
        public string Video { get; set; }
        public double VideoDuration { get; set; }
        public double AudioDuration { get; set; }
        public MediaFile VideoFile { get; set; }
        public string Music { get; set; }
        public Logger Logger { get; set; }
        public MediaElement Media { get; set; }
        public ICommand OpenVideoPickerCommand { get; set; }
        public ICommand OpenMusicPickerCommand { get; set; }
        public ICommand AddAudioToVideoCommand { get; set; }
        public WrapperFunctions Wrapper { get; set; }
        public ClientSideFunctions Client { get; set; }
        public RangeSlider RangeSlider { get; set; }
        FileResult Audiofile { get; set; }
        IHelper helper;
        public double Progress { get; set; }

        public ICommand CancelCommand { get; set; }
        CancellationTokenSource cancellationTokenSource;

        public Label ScrollViewLabel { get; set; }
        public ScrollView ScrollView { get; set; }


        /// <summary>
        /// Zene és hang hozzáadása nézetmodell. Bemeneti értékként megkapja a médiaelemet, a tartománycsúszkát és a naplózó ablakhoz tartozó szövegdobozokat és scrollview-t.
        /// A támogatott operációs rendszerek alapján eldönti, hogy modell-ként ClientSideFunctions-t (iOS, Android) vagy WrapperFunctions-t (UWP) használ.
        /// </summary>
        public MusicViewModel(MediaElement mediaElement, RangeSlider rangeSlider, Label labelLogText, ScrollView scrollView, Label scrollViewLabel)
        {
            ScrollView = scrollView;
            ScrollViewLabel = scrollViewLabel;
            ScrollViewLabel.PropertyChanged += ScrollViewLabel_PropertyChanged;
            labelLogText.Text = "";
            Media = mediaElement;
            Logger = Logger.Instance;
            OpenVideoPickerCommand = new RelayCommand(OpenVideoPicker);
            OpenMusicPickerCommand = new RelayCommand(OpenAudioPicker);
            AddAudioToVideoCommand = new RelayCommand(AddAudioToVideo);

            CancelCommand = new RelayCommand(Cancel);
            RangeSlider = rangeSlider;
            RangeSlider.IsEnabled = false;
            RangeSlider.IsVisible = false;
            RangeSlider.LowerValueChanged += RangeSlider_LowerValueChanged;
            RangeSlider.UpperValueChanged += RangeSlider_UpperValueChanged;
            Media.SeekCompleted += Media_SeekCompleted;
            RangeSlider.LowerValueChanged += RangeSlider_LowerValueChanged;

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
            if (RangeSlider.LowerValue < RangeSlider.UpperValue)
            {
                if ((sender as MediaElement).Position.TotalSeconds < RangeSlider.UpperValue)
                {
                    RangeSlider.LowerValue = Media.Position.TotalSeconds;
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
        /// Megnézi, hogy a tartománycsúszkán nem lépheti túl a videóhoz képest a hang hossza azt a hosszot mint amekkora.
        /// </summary>
        private void RangeSlider_UpperValueChanged(object sender, EventArgs e)
        {
            if (AudioDuration != 0)
            {
                if (RangeSlider.UpperValue - RangeSlider.LowerValue > AudioDuration)
                {
                    RangeSlider.LowerValue = RangeSlider.UpperValue - AudioDuration;
                }
            }
        }

        /// <summary>
        /// Megnézi, hogy a tartománycsúszkán nem lépheti túl a videóhoz képest a hang hossza azt a hosszot mint amekkora.
        /// </summary>
        private void RangeSlider_LowerValueChanged(object sender, EventArgs e)
        {
            if (0 != AudioDuration)
            {
                if (RangeSlider.UpperValue - RangeSlider.LowerValue > AudioDuration)
                {
                    RangeSlider.UpperValue = RangeSlider.LowerValue + AudioDuration;
                }
            }
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

                    Log("Loading video...");
                    Video = file.FileName;
                    VideoFilePath = fileName;
                    RangeSlider.IsVisible = true;
                    RangeSlider.IsEnabled = true;
                    RangeSlider.MinimumValue = 0;
                }
                else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    VideoFile = await CrossMedia.Current.PickVideoAsync();
                    if (VideoFile == null)
                    {
                        return;
                    }
                    Video = Path.GetFileName(VideoFile.Path);
                    VideoFilePath = VideoFile.Path;
                    RangeSlider.IsEnabled = true;
                    RangeSlider.IsVisible = true;
                    RangeSlider.MinimumValue = 0;
                    Log("Loading video...");
                }

                while (Media.Duration == null)
                {
                    await Task.Delay(100);
                }
                VideoDuration = Media.Duration.Value.TotalSeconds;

                RangeSlider.MaximumValue = VideoDuration;
                RangeSlider.LowerValue = 0;
                RangeSlider.UpperValue = VideoDuration;
                RangeSlider.LowerValue += 0.01;
                RangeSlider.LowerValue -= 0.01;
                Log("Done");

            }
            catch (Exception ex)
            {
                RangeSlider.MaximumValue = 1;
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Hangfájl kiválasztása és a szükséges adatok nézetben megjelenítése, médiaelem, tartománycsúszka
        /// </summary>
        private async void OpenAudioPicker()
        {
            try
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    FileResult file = await FilePicker.PickAsync(
                        new PickOptions
                        {
                            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                              {
                                                  { DevicePlatform.UWP, new[] { ".mp3" } }
                              }),
                            PickerTitle = "Pick an audio file"
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

                    Log("Loading audio...");
                    Music = file.FileName;
                    AudioFilePath = fileName;
                    RangeSlider.IsEnabled = true;
                    AudioDuration = await WrapperFunctions.GetAudioLength(AudioFilePath);
                    Log("Done");
                    if (AudioDuration != 0)
                    {
                        RangeSlider.LowerValue += 0.01;
                        RangeSlider.LowerValue -= 0.01;
                    }
                }
                else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    Audiofile = await FilePicker.PickAsync(
                    new PickOptions
                    {
                        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                          {
                                 { DevicePlatform.iOS, new[] { "public.mp3" } },
                                 { DevicePlatform.Android, new[] { "audio/mpeg" } },
                          }),
                        PickerTitle = "Pick an audio file"
                    }
                    );
                    if (Audiofile == null)
                    {
                        return;
                    }
                    Music = Audiofile.FileName;
                    AudioFilePath = Audiofile.FullPath;

                    RangeSlider.IsEnabled = true;
                    Log("Loading audio...");
                    Log("Done");
                    if (AudioDuration != 0)
                    {
                        RangeSlider.LowerValue += 0.01;
                        RangeSlider.LowerValue -= 0.01;
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// A hang hozzáadása videóhoz parancshoz szükséges feltételek ellenőrzése és hang hozzáadásának végrehajtása.
        /// </summary>
        private async void AddAudioToVideo()
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
            if (VideoFilePath == null)
            {
                Log("You haven't selected a video.");
                return;
            }
            if (AudioFilePath == null)
            {
                Log("You haven't selected an audio file.");
                return;
            }
            if (RangeSlider.LowerValue == RangeSlider.UpperValue)
            {
                Log("On the rangeslider upper and lower value can't be the same.");
                return;
            }
            Log("Adding audio to video...");

            #region converison
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
                string inputfilename = Path.GetFileName(VideoFilePath);
                string inputFolderName = Path.GetDirectoryName(VideoFilePath);
                string outputfilename = $"{inputFolderName}\\music_test_{inputfilename}.mp4";
                try
                {
                    WrapperFunctions.Conversion.OnProgress += (sender, args) =>
                    {
                        Progress = args.Duration.TotalSeconds / args.TotalLength.TotalSeconds;
                    };
                    await WrapperFunctions.AddAudioToVideo(VideoFilePath, AudioFilePath,
                        outputfilename, RangeSlider.LowerValue, RangeSlider.UpperValue, cancellationTokenSource.Token);
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
                    await CopyFileTo(outputfilename, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
                        , $"video_with_added_audio.mp4"));
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
                        await ClientSideFunctions.AddAudioToVideo(
                            VideoFile, Audiofile.FileName, await Audiofile.OpenReadAsync(),
                            RangeSlider.LowerValue, RangeSlider.UpperValue, cancellationTokenSource.Token);
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

        /// <summary>
        /// UWP platformon másolás megvalósítása.
        /// </summary>
        public async Task CopyFileTo(FileResult file, string destination)
        {
            Stream stream = await file.OpenReadAsync();

            using (FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Android és iOS platformokon másolás megvalósítás.
        /// </summary>
        public async Task CopyFileTo(string file, string destination)
        {
            helper = DependencyService.Get<IHelper>();
            await helper.copyFile(file, destination);
        }
    }
}
