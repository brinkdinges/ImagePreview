﻿using System.IO;
using ImagePreview.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagePreview.Test
{
    [TestClass]
    public class FileResolverTest
    {
        private FileImageResolver _resolver;
        //private readonly DirectoryInfo _folder = new DirectoryInfo("../../Images/");

        [TestInitialize]
        public void Setup()
        {
            _resolver = new FileImageResolver();
        }

        [DataTestMethod]
        [DataRow("foo.png", "foo.png")]
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
        [DataRow(@"d:\test.png<", @"d:\test.png")]
        [DataRow(@"D:\test.png)", @"D:\test.png")]
        [DataRow(@"c:\folder\test.png]", @"c:\folder\test.png")]
        public void Absolute(string path, string match)
        {
            _resolver.TryGetMatches(path, out System.Text.RegularExpressions.MatchCollection matches);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual(match, matches[0].Groups["image"].Value);
        }
    }
}
