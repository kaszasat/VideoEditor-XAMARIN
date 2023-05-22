using Android.Content;
using Android.Graphics;
using Android.Media;
using System.IO;
using System.Threading.Tasks;
using VideoEditor.Droid.Model;
using VideoEditor.Model;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Helper))]

namespace VideoEditor.Droid.Model
{
    public class Helper : IHelper
    {
        public Task<ImageSource> generateThumbnail(Plugin.Media.Abstractions.MediaFile mediaFile)
        {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            Context context = Android.App.Application.Context;
            retriever.SetDataSource(context, Android.Net.Uri.Parse(mediaFile.Path));
            Bitmap bitmap = 
                retriever.GetFrameAtTime((long)100, Option.ClosestSync);
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                byte[] bitmapData = stream.ToArray();
                return Task.FromResult(ImageSource.FromStream(() => new MemoryStream(bitmapData)));
            }
            return null;
        }

        public Task<bool> copyFile(string from, string to)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> deleteFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> saveStreamToFileSystem(System.IO.Stream data, string filename)
        {
            //var variable = Java.Nio.FileNio.Paths.Get(Path.Combine(Android.OS.Environment.DirectoryMovies, "test.mp4"));
            //Java.Nio.FileNio.Files.Copy(videostream, Java.Nio.FileNio.Paths.Get(Path.Combine(Android.OS.Environment.DirectoryMovies, "test.mp4")), Java.Nio.FileNio.StandardCopyOption.ReplaceExisting);

#pragma warning disable CS0618 // Type or member is obsolete
            var documentsPath = $"{Android.OS.Environment.ExternalStorageDirectory}/{Android.OS.Environment.DirectoryMovies}";
#pragma warning restore CS0618 // Type or member is obsolete

            string filePath = System.IO.Path.Combine(documentsPath, filename);

            byte[] bArray = new byte[data.Length];

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fs.SetLength(0);
                using (data)
                {
                    data.Read(bArray, 0, (int)data.Length);
                }
                int length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
            data.Dispose();
            return Task.FromResult(true);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public Task<string> getSaveFileLocation() => Task.FromResult($"{Android.OS.Environment.ExternalStorageDirectory}/{Android.OS.Environment.DirectoryMovies}");
#pragma warning restore CS0618 // Type or member is obsolete

    }
}