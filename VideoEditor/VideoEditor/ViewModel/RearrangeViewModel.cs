using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoEditor.Model;
using VideoEditor.ViewModel.Helper;
using Xabe.FFmpeg;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace VideoEditor.ViewModel
{
    internal sealed class RearrangeViewModel : ObservableObject
    {
        public string FilePath { get; set; } = string.Empty;
        public Logger Logger { get; set; }
        CancellationTokenSource cancellationTokenSource;
        public ICommand OpenFilePickerCommand { get; set; }
        public ICommand ClearFileListCommand { get; set; }
        public ICommand RearrangeCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public double Progress { get; set; }
        public WrapperFunctions Wrapper { get; set; }
        public ClientSideFunctions Client { get; set; }
        IHelper helper;
        public ScrollView ScrollView { get; set; }
        public Label ScrollViewLabel { get; set; }
        public ICommand StateRefresh { get; }
        public ICommand StateReset { get; }
        public ICommand ItemDragged { get; }
        public ICommand ItemDraggedOver { get; }
        public ICommand ItemDragLeave { get; }
        public ICommand ItemDropped { get; }

        /// <summary>
        /// Újrarendezés  nézetmodell. Bemeneti értékként megkapja a naplózó ablakhoz tartozó szövegdobozokat és scrollview-t.
        /// A támogatott operációs rendszerek alapján eldönti, hogy modell-ként ClientSideFunctions-t (iOS, Android) vagy WrapperFunctions-t (UWP) használ.
        /// </summary>
        public RearrangeViewModel(Label labelLogText, ScrollView scrollView, Label scrollViewLabel)
        {
            ScrollView = scrollView;
            ScrollViewLabel = scrollViewLabel;
            ScrollViewLabel.PropertyChanged += ScrollViewLabel_PropertyChanged;

            labelLogText.Text = "";
            Logger = Logger.Instance;
            OpenFilePickerCommand = new RelayCommand(OpenFilePicker);
            ClearFileListCommand = new RelayCommand(ClearFileList);
            RearrangeCommand = new RelayCommand(Rearrange);
            CancelCommand = new RelayCommand(Cancel);
            StateRefresh = new Command(OnStateRefresh);
            StateReset = new Command(OnStateReset);
            ItemDragged = new Command<ItemViewModel>(OnItemDragged);
            ItemDraggedOver = new Command<ItemViewModel>(OnItemDraggedOver);
            ItemDragLeave = new Command<ItemViewModel>(OnItemDragLeave);
            ItemDropped = new Command<ItemViewModel>(i => OnItemDropped(i));
            ResetItemsState();

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
        private async void ScrollViewLabel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        /// Az újrarendezés parancshoz szükséges feltételek ellenőrzése és az újrarendezés végrehajtása.
        /// </summary>
        private async void Rearrange()
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
            if (GroupedItems.Count == 0)
            {
                Log("You have to select at least two videos to rearrange.");
                return;
            }
            if ((GroupedItems[0]).Count() < 2)
            {
                Log("You have to select at least two videos to rearrange.");
                return;
            }
            Log("Rearranging...");
            cancellationTokenSource = new CancellationTokenSource();
            Progress = 0;

            var files = GroupedItems[0];

            #region conversion
            if (Device.RuntimePlatform == Device.UWP)
            {
                if (!WrapperFunctions.FFmpegIsDownloaded)
                {
                    Log("FFmpeg is not downloaded yet");
                    cancellationTokenSource = null;
                    return;
                }
                WrapperFunctions.Conversion = FFmpeg.Conversions.New();

                string inputFolderName = Path.GetDirectoryName(files[0].FullPath);

                string outputfilename = $"{inputFolderName}\\output_rearranged_test.mp4";
                try
                {
                    WrapperFunctions.Conversion.OnProgress += (sender, args) =>
                    {
                        Progress = args.Duration.TotalSeconds / args.TotalLength.TotalSeconds;
                    };
                    await WrapperFunctions.Rearrange(files, outputfilename, cancellationTokenSource.Token);
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
                    await CopyFileTo(outputfilename, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                        $"rearranged.mp4"));
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
                        await ClientSideFunctions.Rearrange(files, cancellationTokenSource.Token);
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

            Log("Rearrange is done");
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

        /// <summary>
        /// Listaelemek ürítése parancs
        /// </summary>
        public void ClearFileList() => ResetItemsState();

        /// <summary>
        /// Videófájl kiválasztása és a szükséges adatok nézetben megjelenítése
        /// </summary>
        public async void OpenFilePicker()
        {
            try
            {
                Log("Loading video...");
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

                    ImageSource _thumbnail = ImageSource.FromFile((await WrapperFunctions.GetThumbnail(fileName)));

                    Items.Add(new ItemViewModel { Category = "Clip order", Title = file.FileName, FullPath = fileName, Thumbnail = _thumbnail });

                    GroupedItems = Items
                        .GroupBy(i => i.Category)
                        .Select(g => new ItemsGroupViewModel(g.Key, g))
                        .ToObservableCollection();
                }
                else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    MediaFile media = await CrossMedia.Current.PickVideoAsync();

                    if (media == null)
                    {
                        return;
                    }

                    helper = DependencyService.Get<IHelper>();

                    ImageSource thumbnail = await helper.generateThumbnail(media);
                    if (thumbnail == null)
                    {
                        Items.Add(new ItemViewModel
                        {
                            Category = "Clip order",
                            Title = Path.GetFileName(media.Path),
                            FullPath = media.Path,
                            MediaFile = media
                        });
                    }
                    else
                    {
                        Items.Add(new ItemViewModel
                        {
                            Category = "Clip order",
                            Title = Path.GetFileName(media.Path),
                            FullPath = media.Path,
                            MediaFile = media
                        ,
                            Thumbnail = thumbnail
                        });
                    }


                    GroupedItems = Items
                        .GroupBy(i => i.Category)
                        .Select(g => new ItemsGroupViewModel(g.Key, g))
                        .ToObservableCollection();
                }
                Log("Done");
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        /// <summary>
        /// Kinaplóz string üzenetet a jelenlegi dátummal és idővel.
        /// </summary>
        public void Log(string message)
        {
            string longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Logger.LogText.Append($"\n{longtimestr} - {message}");
            Logger.OnPropertyChanged(nameof(Logger.LogText));
        }

        private ObservableCollection<ItemViewModel> _items = new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        private ObservableCollection<ItemsGroupViewModel> _groupedItems = new ObservableCollection<ItemsGroupViewModel>();
        public ObservableCollection<ItemsGroupViewModel> GroupedItems
        {
            get { return _groupedItems; }
            set { SetProperty(ref _groupedItems, value); }
        }

        /// <summary>
        /// Listaelemek értékváltozása parancs.
        /// </summary>
        private void OnStateRefresh() => OnPropertyChanged(nameof(Items));

        /// <summary>
        /// Listaelemek visszaállítása parancs.
        /// </summary>
        private void OnStateReset() => ResetItemsState();

        /// <summary>
        /// Listaelemek mozgatásának megkezdése parancs.
        /// </summary>
        private void OnItemDragged(ItemViewModel item) => Items.ForEach(i => i.IsBeingDragged = item == i);

        /// <summary>
        /// Listaelemek mozgatásának befejezése parancs.
        /// </summary>
        private void OnItemDraggedOver(ItemViewModel item) => Items.ForEach(i => i.IsBeingDraggedOver = item == i && item != _items.FirstOrDefault(x => x.IsBeingDragged));


        /// <summary>
        /// Listaelemek mozgatásánánál mozgatott elem kilép a listából parancs.
        /// </summary>
        private void OnItemDragLeave(ItemViewModel item) => Items.ForEach(i => i.IsBeingDraggedOver = false);

        /// <summary>
        /// Lista újrarendezése amikor a listaelem új pozícióra lett dobva.
        /// </summary>
        private Task OnItemDropped(ItemViewModel item)
        {
            ItemViewModel itemToMove = _items.First(i => i.IsBeingDragged);
            ItemViewModel itemToInsertBefore = item;

            if (itemToMove == null || itemToInsertBefore == null || itemToMove == itemToInsertBefore)
                return Task.CompletedTask;
            ItemsGroupViewModel categoryToMoveFrom = GroupedItems.First(g => g.Contains(itemToMove));
            categoryToMoveFrom.Remove(itemToMove);

            ItemsGroupViewModel categoryToMoveTo = GroupedItems.First(g => g.Contains(itemToInsertBefore));
            int insertAtIndex = categoryToMoveTo.IndexOf(itemToInsertBefore);
            itemToMove.Category = categoryToMoveFrom.Name;
            categoryToMoveTo.Insert(insertAtIndex, itemToMove);
            itemToMove.IsBeingDragged = false;
            itemToInsertBefore.IsBeingDraggedOver = false;
            Log($"Moved \"{itemToMove?.Title}\" to target index {insertAtIndex}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Lista visszaállítása.
        /// </summary>
        private void ResetItemsState()
        {
            Items.Clear();
            GroupedItems = Items
                .GroupBy(i => i.Category)
                .Select(g => new ItemsGroupViewModel(g.Key, g))
                .ToObservableCollection();
        }
    }
}
