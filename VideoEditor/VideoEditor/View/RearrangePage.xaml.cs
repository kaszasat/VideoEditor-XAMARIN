using VideoEditor.ViewModel;
using Xamarin.Forms;

namespace VideoEditor.View
{
    public sealed partial class RearrangePage : ContentPage
    {
        public RearrangePage()
        {
            InitializeComponent();
            this.BindingContext = new RearrangeViewModel(LabelLogText, ScrollView, ScrollViewLabel);
        }
    }
}