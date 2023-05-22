using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using VideoEditor.iOS.Model;
using VideoEditor.Model;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Helper))]

namespace VideoEditor.iOS.Model
{
    internal class Helper : IHelper
    {
        public async Task<bool> copyFile(string from, string to)
        {
            NSFileManager fileManager = new NSFileManager();
            NSError error = new NSError(new NSString("error"), 1);
            return await Task.Run(() => fileManager.Copy(from, to, out error));
        }

        public async Task<bool> deleteFile(string path)
        {
            NSFileManager fileManager = new NSFileManager();
            NSError error = new NSError(new NSString("error"), 1);
            return await Task.Run(() => fileManager.Remove(path, out error));
        }

        public Task<string> getSaveFileLocation() => Task.FromResult(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        public Task<bool> saveStreamToFileSystem(Stream data, string filename)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string filePath = Path.Combine(documentsPath, filename);

            byte[] bArray = new byte[data.Length];
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (data)
                {
                    data.Read(bArray, 0, (int)data.Length);
                }
                int length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
            return Task.FromResult(true);
        }

        Task<Xamarin.Forms.ImageSource> IHelper.generateThumbnail(MediaFile media)
        {
            AVAssetImageGenerator imageGenerator = new AVAssetImageGenerator(AVAsset.FromUrl((new NSUrl(media.Path))))
            {
                AppliesPreferredTrackTransform = true
            };
            CGImage cgImage = imageGenerator.CopyCGImageAtTime(new CMTime(1000, 1000000), out CMTime actualTime, out NSError error);
            return Task.FromResult(ImageSource.FromStream(() => new UIImage(cgImage).AsPNG().AsStream()));
        }
    }
}