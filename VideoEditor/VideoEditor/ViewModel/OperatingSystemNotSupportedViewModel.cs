using VideoEditor.Model;
using Xamarin.Forms;

namespace VideoEditor.ViewModel
{
    internal sealed class OperatingSystemNotSupportedViewModel
    {
        public OperatingSystemNotSupportedViewModel(View.OperatingSystemNotSupportedPage operatingSystemNotSupportedPage)
        {
            QuitApplicationWithAlert(operatingSystemNotSupportedPage);
        }

        /// <summary>
        /// Kilép az alkalmazásból.
        /// </summary>
        private async void QuitApplicationWithAlert(View.OperatingSystemNotSupportedPage operatingSystemNotSupportedPage)
        {
            await operatingSystemNotSupportedPage.DisplayAlert("Loading error", "No Internet Connection. Please try again later.", "Quit");
            var closer = DependencyService.Get<ICloseApplication>();
            closer?.closeApplication();
        }
    }
}
