using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioUnit;
using Foundation;
using MobileCoreServices;
using Photos;
using PhotosUI;
using Plugin.Media.Abstractions;
using UIKit;

namespace Plugin.Media.iOS
{
    public class MediaPickerService
    {
        //Source: https://developer.apple.com/videos/play/wwdc2020/10652

        public static TaskCompletionSource<List<MediaFile>> CompletionSource;

        

        public class PickerDelegate : PHPickerViewControllerDelegate
        {
            public StoreCameraMediaOptions MediaOptions { get; set; } = new StoreCameraMediaOptions();

            public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results)
            {
                // multiple 'DidFinishPicking' calls can be made while processing selections - stop that nonsense here
                if (picker.IsBeingDismissed)
                    return;

                //Dismiss Picker
                picker.DismissViewController(true, null);

                try
                {
                    List<MediaFile> MediaFiles = new List<MediaFile>();


                    foreach (var result in results)
                    {
                        var taskCompletionSource = new TaskCompletionSource<MediaFile>();

                        // Convert PHPickerResult Images to MediaFiles (path and stream handler)
                        result.ItemProvider.LoadFileRepresentation(UTType.Image, (item, error) =>
                        {
                            if (error != null || item == null) return;

                            //Set Result
                            taskCompletionSource.SetResult(GetMediaFile(item, MediaImplementation.TypeImage));
                        });


                        // Convert PHPickerResult Videos to MediaFiles (path and stream handler)
                        result.ItemProvider.LoadFileRepresentation(UTType.Movie, (item, error) =>
                        {
                            if (error != null || item == null) return;

                            //Set Result
                            taskCompletionSource.SetResult(GetMediaFile(item, MediaImplementation.TypeMovie));
                        });
                        
                        MediaFiles.Add(taskCompletionSource.Task.Result);
                    }

                    CompletionSource.SetResult(MediaFiles);
                }
                catch (Exception ex) { 
                    Console.WriteLine(ex);
                    CompletionSource.SetResult(null); 
                }
            }

            /// <summary>
            /// Copy original file data so that we preserve exif/metadata
            /// </summary>
            /// <param name="src"></param>
            /// <returns></returns>
            MediaFile GetMediaFile(NSUrl src, string mediaType)
            {
                // this seems to be the only working method to interact with this NSURL (other direct means fail)
                NSData data = NSData.FromUrl(src);

                // build in-app destination directory that we have access to
                var dst = MediaPickerDelegate.GetOutputPath(mediaType, 
                    MediaOptions.Directory ?? "temp",
                    MediaOptions.Name, src.PathExtension);

                // create new file in the destination directory with the lovely data
                NSFileManager fileManager = new NSFileManager();
                bool success = fileManager.CreateFile(dst, data, new NSFileAttributes());

                // cleanup remains of NSData (hopefully hasn't killed memory)
                data?.Dispose();
                data = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Default);

                if (success)
                {
                    return new MediaFile(dst, () => File.OpenRead(dst));
                }
                else
                {
                    return new MediaFile("", null);
                }
            }
        }
    }
}
