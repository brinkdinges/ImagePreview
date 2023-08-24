﻿using System.IO;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Resolvers
{
    internal class PackResolver : IImageResolver
    {
        private static readonly Regex _regex = new(@"(pack://application:[^/]+)?/[\w]+;component/(?<image>[^""]+\.(?<ext>png|gif|jpg|jpeg|ico))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetMatches(string lineText, out MatchCollection matches)
        {
            matches = null;

            if (lineText.IndexOf(";component/", StringComparison.OrdinalIgnoreCase) > -1)
            {
                matches = _regex.Matches(lineText);
                return true;
            }

            return false;
        }

        public async Task<string> GetAbsoluteUriAsync(ImageReference reference)
        {
            if (string.IsNullOrEmpty(reference?.RawImageString))
            {
                return null;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE dte = await VS.GetRequiredServiceAsync<DTE, DTE>();
            ProjectItem item = dte.Solution.FindProjectItem(reference.SourceFilePath);

            string projectRoot = item.ContainingProject?.GetRootFolder();
            return Path.GetFullPath(Path.Combine(projectRoot, reference.RawImageString.TrimStart('/')));
        }

        public async Task<BitmapSource> GetBitmapAsync(ImageReference result)
        {
            string absoluteFilePath = await result.Resolver.GetAbsoluteUriAsync(result);
            
            if (string.IsNullOrEmpty(absoluteFilePath) || !File.Exists(absoluteFilePath))
            {
                return null;
            }

            result.SetFileSize(new FileInfo(absoluteFilePath).Length);

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
            bitmap.UriSource = new Uri(absoluteFilePath);
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}
