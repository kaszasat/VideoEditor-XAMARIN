using VideoEditor.ViewModel;
using Xamarin.Forms;

namespace VideoEditor.View
{
    public sealed partial class MontagePage : ContentPage
    {
        public MontagePage()
        {
            InitializeComponent();
            this.BindingContext = new MontageViewModel(LabelLogText, ScrollView, ScrollViewLabel);
        }
    }
}