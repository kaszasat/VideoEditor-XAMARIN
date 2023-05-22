using VideoEditor.ViewModel;
using Xamarin.Forms;

namespace VideoEditor.View
{
    public sealed partial class MusicPage : ContentPage
    {
        public MusicPage()
        {
            InitializeComponent();
            this.BindingContext = new MusicViewModel(mediaElement, MusicRangeSlider, LabelLogText, ScrollView, ScrollViewLabel);
        }
    }
}