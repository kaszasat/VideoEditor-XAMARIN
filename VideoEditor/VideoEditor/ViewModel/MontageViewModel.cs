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
    internal sealed class MontageViewModel : ObservableObject
    {
        IHelper helper;
        public string FilePath { get; set; } = string.Empty;
        public Logger Logger { get; set; }
        public ICommand OpenFilePickerCommand { get; set; }
        public ICommand ClearFileListCommand { get; set; }
        public ICommand CreateMontageCommand { get; set; }
        public WrapperFunctions Wrapper { get; set; }
        public ICommand StateRefresh { get; }
        public ICommand StateReset { get; }
        public ICommand ItemDragged { get; }
        public ICommand ItemDraggedOver { get; }
        public ICommand ItemDragLeave { get; }
        public double Progress { get; set; }
        public ICommand CancelCommand { get; set; }

        public ScrollView ScrollView { get; set; }
        public Label ScrollViewLabel { get; set; }

        CancellationTokenSource cancellationTokenSource;
        public ClientSideFunctions Client { get; set; }
        public ICommand ItemDropped { get; }
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
        /// Naplóérték frissülés esetén a naplózó ablak aljára görget.
        /// </summary>
        private async void ScrollViewLabel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await Task.Delay(10);
            await ScrollView.ScrollToAsync(0, ScrollViewLabel.Height, true);
        }

        /// <summary>
        /// Montázs létrehozása nézetmodell. Bemeneti értékként megkapja a naplózó ablakhoz tartozó szövegdobozokat és scrollview-t.
        /// A támogatott operációs rendszerek alapján eldönti, hogy modell-ként ClientSideFunctions-t (iOS, Android) vagy WrapperFunctions-t (UWP) használ.
        /// </summary>
        public MontageViewModel(Label labelLogText, ScrollView scrollView, Label scrollViewLabel)
        {
            ScrollViewLabel = scrollViewLabel;
            ScrollView = scrollView;
            ScrollViewLabel.PropertyChanged += ScrollViewLabel_PropertyChanged;
            labelLogText.Text = "";

            Logger = Logger.Instance;
            CancelCommand = new RelayCommand(Cancel);

            OpenFilePickerCommand = new RelayCommand(OpenFilePicker);
            ClearFileListCommand = new RelayCommand(ClearFileList);
            CreateMontageCommand = new RelayCommand(CreateMontage);
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
        /// Kinaplóz string üzenetet a jelenlegi dátummal és idővel.
        /// </summary>
        public void Log(string message)
        {
            string longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Logger.LogText.Append($"\n{longtimestr} - {message}");
            Logger.OnPropertyChanged(nameof(Logger.LogText));
        }

        /// <summary>
        /// A montázs létrehozásáa parancshoz szükséges feltételek ellenőrzése és a montázs létrehozásának végrehajtása.
        /// </summary>
        private async void CreateMontage()
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

            if (GroupedItems.Count > 0)
            {
                cancellationTokenSource = new CancellationTokenSource();
                Progress = 0;
                var files = GroupedItems[0];
                Log("Creating montage...");

                if (Device.RuntimePlatform == Device.UWP)
                {
                    if (!WrapperFunctions.FFmpegIsDownloaded)
                    {
                        Log("FFmpeg is not downloaded yet");
                        cancellationTokenSource = null;
                        return;
                    }
                    string inputFolderName = Path.GetDirectoryName(files[0].FullPath);

                    WrapperFunctions.Conversion = FFmpeg.Conversions.New();

                    string destination = $"{inputFolderName}\\{WrapperFunctions.RandomString(10)}.mp4";

                    try
                    {
                        WrapperFunctions.Conversion.OnProgress += (sender, args) =>
                        {
                            Progress = args.Duration.TotalSeconds / args.TotalLength.TotalSeconds;
                        };
                        await WrapperFunctions.CreateMontage(files, destination, cancellationTokenSource.Token);
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
                        await CopyFileTo(destination, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                            $"montage.mp4"));
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
                            await ClientSideFunctions.CreateMontage(files, cancellationTokenSource.Token);
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
                Log("Done");
            }
            else
                Log("You haven't added even a single picture to create a montage with.");
            cancellationTokenSource = null;

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
        /// UWP platformon másolás megvalósítása.
        /// </summary>
        public async void CopyFileTo(FileResult file, string destination)
        {
            Stream stream = await file.OpenReadAsync();

            using (FileStream fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write))
                stream.CopyTo(fileStream);
        }

        /// <summary>
        /// Képfájl kiválasztása és a szükséges adatok nézetben megjelenítése
        /// </summary>
        public async void OpenFilePicker()
        {
            try
            {
                if (Device.RuntimePlatform == Device.UWP)
                {
                    FileResult file = await FilePicker.PickAsync(
                    new PickOptions
                    {
                        FileTypes = FilePickerFileType.Images,
                        PickerTitle = "Pick an image file"
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
                        CopyFileTo(file, fileName);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                        return;
                    }


                    ImageSource _thumbnail = ImageSource.FromFile(fileName);

                    Items.Add(new ItemViewModel { Category = "Image order", Title = file.FileName, FullPath = fileName, Thumbnail = _thumbnail });

                    GroupedItems = Items
                        .GroupBy(i => i.Category)
                        .Select(g => new ItemsGroupViewModel(g.Key, g))
                        .ToObservableCollection();
                }
                else if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                {
                    MediaFile image = await CrossMedia.Current.PickPhotoAsync();
                    if (image == null)
                        return;
                    ImageSource _thumbnail = ImageSource.FromFile(image.Path);
                    Items.Add(new ItemViewModel
                    {
                        Category = "Clip order",
                        Title = Path.GetFileName(image.Path),
                        FullPath = image.Path,
                        Thumbnail = _thumbnail,
                        MediaFile = image
                    });

                    GroupedItems = Items
                        .GroupBy(i => i.Category)
                        .Select(g => new ItemsGroupViewModel(g.Key, g))
                        .ToObservableCollection();
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

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
