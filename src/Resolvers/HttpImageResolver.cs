﻿using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(https?:|ftp:)?//[\w/\-?=%.\\ ]+\.(png|gif|jpg|jpeg|ico)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public Task<ImageResult> GetImageAsync(Span span, string value, string filePat)
        {
            string absoluteUrl = GetFullUrl(value);
            return Task.FromResult(new ImageResult(span, absoluteUrl));
        }

        public static string GetFullUrl(string rawFilePath)
        {
            if (string.IsNullOrEmpty(rawFilePath))
            {
                return null;
            }

            rawFilePath = rawFilePath.Trim('\'', '"', '~');

            if (rawFilePath.StartsWith("//", StringComparison.Ordinal))
            {
                rawFilePath = "http:" + rawFilePath;
            }

            return Uri.TryCreate(rawFilePath, UriKind.Absolute, out Uri result) ? result.OriginalString : null;
        }

        public Task<BitmapSource> GetBitmapAsync(ImageResult result)
        {
            TaskCompletionSource<BitmapSource> tcs = new();
            BitmapImage bitmap = new(new Uri(result.RawImageString));

            if (bitmap.IsDownloading)
            {
                bitmap.DownloadCompleted += (s, e) => tcs.SetResult(bitmap);
                bitmap.DownloadFailed += (s, e) => tcs.SetException(e.ErrorException);
            }
            else
            {
                tcs.SetResult(bitmap);
            }

            return tcs.Task;
        }
    }
}
