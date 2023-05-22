using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoEditor.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public sealed partial class NoInternetPage : ContentPage
    {
        public NoInternetPage()
        {
            InitializeComponent();
            this.BindingContext = new ViewModel.NoInternetViewModel(this);
        }
    }
}