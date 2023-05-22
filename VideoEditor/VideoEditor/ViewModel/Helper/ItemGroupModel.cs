using Plugin.Media.Abstractions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace VideoEditor.ViewModel.Helper
{
    public sealed class ItemsGroupViewModel : ObservableCollection<ItemViewModel>
    {
        public string Name { get; set; }

        public ItemsGroupViewModel(string name, IEnumerable<ItemViewModel> items) : base(items)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Eltárolja a szükséges adatokat, mint a kategóriát, (több idővonalas megvalósítás esetén lett volna ennek értelme, de továbbfejlesztési lehetőségként nyitva áll)
    /// nevet, útvonalat, thumbnailt (miniatűrt), azt, hogy éppen mozgatva van-e az adott felületi elem és a ClientSideFunctions miatt a MediaFile-t.
    /// </summary>
    public sealed class ItemViewModel : ObservableObject
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string FullPath { get; set; }
        public MediaFile MediaFile { get; set; }
        public ImageSource Thumbnail { get; set; }

        private bool _isBeingDragged;
        public bool IsBeingDragged
        {
            get { return _isBeingDragged; }
            set { SetProperty(ref _isBeingDragged, value); }
        }

        private bool _isBeingDraggedOver;
        public bool IsBeingDraggedOver
        {
            get { return _isBeingDraggedOver; }
            set { SetProperty(ref _isBeingDraggedOver, value); }
        }
    }
}
