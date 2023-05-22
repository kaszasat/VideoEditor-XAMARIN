using System.Threading.Tasks;
using Xamarin.Forms;

namespace VideoEditor.Model
{
    /// <summary>
    /// IHelper interface platformspecifikus metódusok deklarálása.
    /// </summary>
    public interface IHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        Task<bool> copyFile(string from, string to);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        Task<bool> deleteFile(string path);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        Task<bool> saveStreamToFileSystem(System.IO.Stream videostream, string filename);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        Task<ImageSource> generateThumbnail(Plugin.Media.Abstractions.MediaFile media);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        Task<string> getSaveFileLocation();
    }
}
