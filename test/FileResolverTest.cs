﻿using System.IO;
using System.Threading.Tasks;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;

namespace ImagePreview.Test
{
    [TestClass]
    public class FileResolverTest
    {
        private FileImageResolver _resolver;
        private readonly DirectoryInfo _folder = new DirectoryInfo("../../Images/");

        [TestInitialize]
        public void Setup()
        {
            _resolver = new FileImageResolver();
        }

        [DataTestMethod]
        [DataRow("foo.png", "foo.png")]
        [DataRow("before foo.png after", "foo.png")]
        [DataRow("~/foo.png", "/foo.png")]
        [DataRow("(foo.png)", "foo.png")]
        [DataRow(">foo.png<", "foo.png")]
        [DataRow("[foo.png]", "foo.png")]
        [DataRow("bar/foo.png", "bar/foo.png")]
        [DataRow("/bar/foo.png", "/bar/foo.png")]
        [DataRow("../bar/foo.png", "../bar/foo.png")]
        public void Relative(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow(@"c:\test.png", @"c:\test.png")]
        [DataRow(@"before c:\test.png after", @"c:\test.png")]
        [DataRow(@"c:/test.png", @"c:/test.png")]
        [DataRow(@"d:\test.png<", @"d:\test.png")]
        [DataRow(@"D:\test.png)", @"D:\test.png")]
        [DataRow(@"c:\folder\test.png]", @"c:\folder\test.png")]
        [DataRow(@"c:\folder/test.png]", @"c:\folder/test.png")]
        public void Absolute(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }

        [DataTestMethod]
        [DataRow(@"c:\test.png", ImageFormat.PNG)]
        [DataRow(@"test.png", ImageFormat.PNG)]
        [DataRow(@"foo \test.png bar", ImageFormat.PNG)]
        [DataRow(@"test.svg", ImageFormat.SVG)]
        [DataRow(@"test.ico", ImageFormat.ICO)]
        [DataRow(@"test.jpg", ImageFormat.JPG)]
        [DataRow(@"test.jpeg", ImageFormat.JPG)]
        [DataRow(@"test.gif", ImageFormat.GIF)]
        public void ImageFormatType(string path, ImageFormat format)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(format, matches[0].GetImageFormat());
        }

        [TestMethod]
        public void GetImageReference()
        {
            Span span = new Span(11, 8);
            string codeFile = Path.Combine(_folder.FullName, "test.cs");
            string pngPath = Path.ChangeExtension(codeFile, ".png");
            _resolver.TryGetMatches(pngPath, out System.Text.RegularExpressions.MatchCollection matches);
            ImageReference result = new ImageReference ( _resolver, span, matches[0], codeFile);

            Assert.AreEqual(pngPath, result.RawImageString);
            Assert.AreEqual(span, result.Span);
        }


        [Ignore]
        [DataTestMethod]
        [DataRow("test.png", 266)]
        [DataRow("test.svg", 1553)]
        public async Task GetBitmapAsync(string file, long fileSize)
        {
            string png = Path.Combine(_folder.FullName, file);
            _resolver.TryGetMatches(png, out System.Text.RegularExpressions.MatchCollection matches);
            ImageReference result = new ImageReference(_resolver, new Span(), matches[0], null);
            System.Windows.Media.Imaging.BitmapSource bitmap = await _resolver.GetBitmapAsync(result);

            Assert.IsNotNull(bitmap);
            Assert.AreEqual(fileSize, result.FileSize);
        }
    }
}
