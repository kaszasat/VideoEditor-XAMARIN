using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoEditor.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class OperatingSystemNotSupportedPage : ContentPage
    {
        public OperatingSystemNotSupportedPage()
        {
            InitializeComponent();
            this.BindingContext = new ViewModel.OperatingSystemNotSupportedViewModel(this);
        }
    }
}