using System.Threading;
using VideoEditor.iOS.Model;
using VideoEditor.Model;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApplication))]

namespace VideoEditor.iOS.Model
{
    internal class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            Thread.CurrentThread.Abort();
        }
    }
}