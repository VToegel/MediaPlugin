using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Android.Content;
using Plugin.Media.Abstractions;

namespace Plugin.Media
{
    /// <summary>
    /// 
    /// </summary>
    //TODO [Android.Runtime.Preserve(AllMembers = true)]
    public static class MediaFileExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DynamicDependency("GetMedia")]
        public static Task<MediaFile> GetMediaFileExtraAsync(this Intent self, Context context)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (context == null)
                throw new ArgumentNullException("context");

            var action = self.GetStringExtra("action");
            if (action == null)
                throw new ArgumentException("Intent was not results from MediaPicker", "self");

            //TODO var uri = (Android.Net.Uri)self.GetParcelableExtra("MediaFile");
            var uri = (Android.Net.Uri)self.GetParcelableExtra("MediaFile", Java.Lang.Class.FromType(typeof(Android.Net.Uri)));
            var isPhoto = self.GetBooleanExtra("isPhoto", false);
            //TODO var path = (Android.Net.Uri)self.GetParcelableExtra("path");
            var path = (Android.Net.Uri)self.GetParcelableExtra("path", Java.Lang.Class.FromType(typeof(Android.Net.Uri)));
            var saveToAlbum = false;
            try
            {
                //TODO saveToAlbum = (bool)self.GetParcelableExtra("album_save");
                saveToAlbum = (bool)self.GetParcelableExtra("album_save", Java.Lang.Class.FromType(typeof(bool)));
            }
            catch { }

            return MediaPickerActivity.GetMediaFileAsync(context, 0, action, isPhoto, ref path, uri, saveToAlbum)
                .ContinueWith(t => t.Result.ToTask()).Unwrap();
        }
    }

}