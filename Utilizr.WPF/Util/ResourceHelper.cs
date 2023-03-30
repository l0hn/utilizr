using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Utilizr.Info;
using Utilizr.Logging;

namespace Utilizr.WPF.Util
{
    public static class ResourceHelper
    {
        private static string _resourceDir = "resources";
        /// <summary>
        /// custom resource path, directories separated by / i.e. resources/v1
        /// </summary>
        public static string ResourceDir
        {
            get => _resourceDir;
            set => _resourceDir = value.Trim().Trim('/');
        }

        private static bool? _inDesignMode;

        /// <summary>
        /// Returns the URI for the specified resourceKey
        /// </summary>
        /// <param name="resourceKey">Image name including extension, e.g. foobar.png</param>
        /// <returns></returns>
        public static Uri GetImageUri(string resourceKey)
        {
            CheckDesignMode();

            Uri uri = _inDesignMode == true // Nullable bool
                ? new Uri($"../../{_resourceDir}/{resourceKey}", UriKind.Relative)
                : new Uri($"pack://siteoforigin:,,,/{_resourceDir}/{resourceKey}");

            return uri;
        }


        /// <summary>
        /// Get an image from the resources folder
        /// </summary>
        /// <param name="resourceKey">Image name including extension, e.g. foobar.png</param>
        /// <returns></returns>
        public static BitmapFrame? GetImageSource(string resourceKey)
        {
            try
            {
                CheckDesignMode();

                var bitmapFrame = ImageCache.Get(resourceKey);
                if (bitmapFrame != null)
                    return bitmapFrame;

                var uri = GetImageUri(resourceKey);

                //IMPORTANT!!! never return a BitmapImage.. doing so will lock image files and prevent application auto updates from working.
                return BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                //IMPORTANT!!!
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Failed to load resource for key {resourceKey}");
#if DEBUG
                if (_inDesignMode != true)
                    throw;
#endif
                return GetDefaultImagePlaceholder();
            }
        }

        public static byte[] GetImageBytes(string resourceKey)
        {
            var bitmapFrame = GetImageSource(resourceKey);
            using var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(bitmapFrame);
            encoder.Save(ms);
            return ms.ToArray();
        }

        public static string GetImageBase64(string resourceKey)
        {
            var bytes = GetImageBytes(resourceKey);
            return Convert.ToBase64String(bytes, Base64FormattingOptions.None);
        }

        /// <summary>
        /// Gets the default image placeholder to be used when images are not found
        /// </summary>
        /// <returns>Default placeholder image</returns>
        private static BitmapFrame? GetDefaultImagePlaceholder()
        {
            try
            {
                return null; //for some reason trying to return the below image is causing some dll load errors on some end user machines.
                //IMPORTANT!!! never return a BitmapImage.. doing so will lock image files and prevent application auto updates from working.
//                Uri uri = new Uri("pack://application:,,,/Utilizr.WPF;component/resources/default_image_placeholder.png", UriKind.Absolute);
//                return BitmapFrame.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                //IMPORTANT!!!
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Failed to load image placeholder resource");
                throw;
            }
        }

        /// <summary>
        /// Returns a URI of the absolute file path.
        /// </summary>
        /// <param name="resourceKey">Name of the video, including file extension.</param>
        /// <param name="resourcePath">Resource subfolder path relative to the main application. Default is /resources subfolder.</param>
        /// <returns></returns>
        public static Uri? GetVideoSource(string resourceKey, string? resourcePath = null)
        {
            resourcePath ??= "resources";

            try
            {
                var file = Path.Combine(AppInfo.AppDirectory, resourcePath, resourceKey);
                return new Uri(file);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Failed to load resource for key {resourcePath}/{resourceKey}");
#if DEBUG
                if (_inDesignMode != true)
                    throw;
#endif
                return null; // placeholder
            }
        }

        static ResourceHelper()
        {
            //################################################################################################
            //THIS TEST EXISTS TO ENSURE YOU DO NOT DEVIATE FROM BITMAPFRAME TO SOME OTHER TYPE OF IMAGE SOURCE
            //DOING SO WILL BREAK THE AUTOMATIC UPDATE FEATURE OF THIS APPLICATION
            //################################################################################################
            var testImageMethodReturn = typeof (ResourceHelper).GetMethod(nameof(GetImageSource))!.ReturnType;
            if (testImageMethodReturn != typeof(BitmapFrame))
            {
                throw new ApplicationException($"Type of {nameof(GetImageSource)} must be BitmapFrame");
            }
            var testIconMethodReturn = typeof(ResourceHelper).GetMethod(nameof(GetIconSource))!.ReturnType;
            if (testIconMethodReturn != typeof(BitmapFrame))
            {
                throw new ApplicationException($"Type of {nameof(GetIconSource)} must be BitmapFrame");
            }
            //################################################################################################
            //################################################################################################
            //################################################################################################
        }

        public static BitmapFrame? GetIconSource(string resourceKey)
        {
            try
            {
                CheckDesignMode();

                var result = ImageCache.Get(resourceKey);
                if (result != null)
                    return result;

                if (_inDesignMode == true)
                {
                    var uri = new Uri($"../../{_resourceDir}/{resourceKey}", UriKind.Relative);
                    using var stream = Application.GetResourceStream(uri).Stream;
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                else
                {
                    var path = Path.Combine(AppInfo.AppDirectory, _resourceDir);
                    path = Path.Combine(path, resourceKey);
                    using var stream = File.Open(path, FileMode.Open);
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
            catch (IOException)
            {
#if DEBUG
                if (_inDesignMode != true)
                    throw; // Make sure we have all images during development, no typos, etc
#endif
                //return the default placeholder image to prevent the application crashing when images are not found
                return GetDefaultImagePlaceholder();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Failed to load resource for key {resourceKey}");
                throw;
            }
        }

        static void CheckDesignMode()
        {
            _inDesignMode ??= (bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue);
        }

        /// <summary>
        /// Returns the resource defined within the app's, or the given FrameworkElement's, resource dictionary.
        /// If <see cref="throwOnError"/> = false, returns default(T) instead of throwing exception.
        /// </summary>
        /// <typeparam name="T">Type of object for the given resource key ($"{className}{propertyName}")</typeparam>
        /// <param name="className">Class name in which the property was declared</param>
        /// <param name="propertyName">Property name in which to retrieve</param>
        /// <param name="toSearch">Find the resource within the given FrameworkElement, rather than Application.Current.</param>
        /// <param name="throwOnError">Raises exception for true, otherwise returns default(T)</param>
        /// <returns></returns>
        public static T? GetDictionaryDefined<T>(string className, string propertyName, FrameworkElement? toSearch = null, bool throwOnError = false)
        {
            return GetDictionaryDefined<T>($"{className}{propertyName}", toSearch, throwOnError);
        }

        /// <summary>
        /// Returns the resource defined within the app's, or the given FrameworkElement's, resource dictionary.
        /// If <see cref="throwOnError"/> = false, returns default(T) instead of throwing exception.
        /// </summary>
        /// <typeparam name="T">Type of object for the given resource key ($"{className}{propertyName}")</typeparam>
        /// <param name="resourceKey">The key for the resource to retrieve</param>
        /// <param name="toSearch">Find the resource within the given FrameworkElement, rather than Application.Current.</param>
        /// <param name="throwOnError">Raises exception for true, otherwise returns default(T)</param>
        /// <returns></returns>
        public static T? GetDictionaryDefined<T>(string resourceKey, FrameworkElement? toSearch = null, bool throwOnError = false)
        {
            try
            {
                return toSearch == null
                    ? (T)Application.Current.FindResource(resourceKey)
                    : (T)toSearch.FindResource(resourceKey);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"Failed to dictionary defined resource for key {resourceKey}. Has the key changed?");

                if (throwOnError)
                    throw;
            }

            return default;
        }
    }
}