using VideoEditor.Model;
using VideoEditor.UWP.Model;
using Xamarin.Forms;


[assembly: Xamarin.Forms.Dependency(typeof(CloseApplication))]

namespace VideoEditor.UWP.Model
{
    internal class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            Application.Current.Quit();
        }
    }
}
