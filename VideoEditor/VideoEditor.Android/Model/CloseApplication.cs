
using VideoEditor.Droid.Model;
using VideoEditor.Model;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApplication))]

namespace VideoEditor.Droid.Model
{
    internal class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}