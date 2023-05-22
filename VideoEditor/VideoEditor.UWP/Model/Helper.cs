using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VideoEditor.Model;
using VideoEditor.UWP.Model;
using Windows.Storage;
using Windows.Storage.Pickers;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Helper))]

namespace VideoEditor.UWP.Model
{
    public class Helper : IHelper
    {
        public Task<ImageSource> generateThumbnail(MediaFile media)
        {
            throw new NotImplementedException();
        }

        public Task<string> getSaveFileLocation()
        {
            throw new NotImplementedException();
        }

        public Task<bool> saveStreamToFileSystem(Stream videostream, string filename)
        {
            throw new NotImplementedException();
        }

        async Task<bool> IHelper.copyFile(string from, string to)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(from);
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };

            if (from.ToLower().EndsWith("mkv"))
            {
                savePicker.FileTypeChoices.Add("MKV", new List<string>() { ".mkv" });
            }
            if (from.ToLower().EndsWith("mp4"))
            {
                savePicker.FileTypeChoices.Add("MP4", new List<string>() { ".mp4" });
            }
            savePicker.SuggestedFileName = Path.GetFileNameWithoutExtension(to);

            StorageFile newFile = await savePicker.PickSaveFileAsync();

            if (newFile != null)
            {
                await file.CopyAndReplaceAsync(newFile);
                return true;
            }
            else
            {
                return false;
            }
        }

        async Task<bool> IHelper.deleteFile(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            await file.DeleteAsync();
            return true;
        }
    }
}
