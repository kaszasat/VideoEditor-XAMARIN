using VideoEditor.ViewModel;
using Xamarin.Forms;


namespace VideoEditor.View
{
    public sealed partial class TrimPage : ContentPage
    {
        public TrimPage()
        {
            InitializeComponent();
            TrimViewModel trimViewModel = new TrimViewModel(mediaElement, TrimRangeSlider, ScrollView, ScrollViewLabel, LabelLogText);
            this.BindingContext = trimViewModel;
        }
    }
}