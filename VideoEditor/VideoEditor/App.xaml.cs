using Xamarin.Essentials;
using Xamarin.Forms;

namespace VideoEditor
{
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                MainPage = new VideoEditor.View.AppShell();
            }
            else
            {
                MainPage = new VideoEditor.View.NoInternetPage();
            }

        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
