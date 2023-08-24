﻿using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagePreview.Helpers;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class HttpImageResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(?<image>(https?:|ftp:)?//[\w/\-?=%.\\]+\.(?<ext>png|gif|jpg|jpeg|ico|svg))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(".png", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".gif", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".ico", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".svg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) > -1 ||
                lineText.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public Task<string> GetAbsoluteUriAsync(ImageReference reference)
        {
            if (string.IsNullOrEmpty(reference?.RawImageString))
            {
                return Task.FromResult<string>(null);
            }

            string rawFilePath = reference.RawImageString.Trim('\'', '"', '~');

            if (rawFilePath.StartsWith("//", StringComparison.Ordinal))
            {
                rawFilePath = "http:" + rawFilePath;
            }

            return Uri.TryCreate(rawFilePath, UriKind.Absolute, out Uri result) ? Task.FromResult(result.OriginalString) : Task.FromResult<string>(null);
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            using (HttpClient client = new())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(await GetAbsoluteUriAsync(result));
                result.SetFileSize(imageBytes.Length);

                if (result.RawImageString.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    string filePath = Path.GetTempFileName();
                    File.WriteAllBytes(filePath, imageBytes);
                    return SvgHelper.GetBitmapFromSvgFile(filePath);
                }
                else
                {
                    using (MemoryStream ms = new(imageBytes, 0, imageBytes.Length))
                    {
                        BitmapImage bitmap = new();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();

                        return bitmap;
                    }
                }
            }
        }
    }
}
