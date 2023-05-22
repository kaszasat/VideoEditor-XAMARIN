using VideoEditor.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace VideoEditor.ViewModel
{
    internal sealed class NoInternetViewModel
    {
        public NoInternetViewModel(View.NoInternetPage noInternetPage)
        {
            QuitApplicationWithAlert(noInternetPage);
        }

        /// <summary>
        /// Ellenőrzi, hogy van-e internet, ha van, akkor betölt az eredeti AppShell megvalósítással.
        /// Ha nincs internet akkor rekurzív módon meghívja önmagát, hogy újra ellenőrizze az internetkapcsolatot, vagy kilép.
        /// </summary>
        private async void QuitApplicationWithAlert(View.NoInternetPage noInternetPage)
        {
            bool result = await noInternetPage.DisplayAlert("Loading error", "No Internet Connection. Please try again later.", "Restart application", "Quit");
            if (!result)
            {
                var closer = DependencyService.Get<ICloseApplication>();
                closer?.closeApplication();
            }
            else
            {
                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    Application.Current.MainPage = new VideoEditor.View.AppShell();
                }
                else
                {
                    QuitApplicationWithAlert(noInternetPage);
                }
            }
        }
    }
}
