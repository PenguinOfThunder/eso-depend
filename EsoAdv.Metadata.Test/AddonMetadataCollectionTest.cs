using System;
using System.IO;
using System.Linq;
using EsoAdv.Metadata.Model;
using EsoAdv.Metadata.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsoAdv.Metadata.Test
{

    [TestClass]
    public class AddonMetadataCollectionTest
    {
        private string testdataFolder = @"C:\Users\tore2\Documents\Elder Scrolls Online\live\AddOns";

        [TestMethod]
        public void TestAnalyze()
        {
            var aoColl = FileParser.ParseFolder(testdataFolder);
            var issues = aoColl.Analyze();
            Assert.IsNotNull(issues, "Collection must not be null");
            Assert.IsTrue(issues.Any(), "Collection must not be empty");
            CollectionAssert.AllItemsAreNotNull(issues, "No items may be null");
        }

    }
}