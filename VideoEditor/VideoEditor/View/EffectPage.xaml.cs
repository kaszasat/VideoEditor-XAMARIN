using VideoEditor.ViewModel;
using Xamarin.Forms;

namespace VideoEditor.View
{
    public sealed partial class EffectPage : ContentPage
    {
        public EffectPage()
        {
            InitializeComponent();
            this.BindingContext = new EffectViewModel(EffectRangeSlider, PickerInstance, mediaElement, LabelLogText, ScrollView, ScrollViewLabel);
        }
    }
}