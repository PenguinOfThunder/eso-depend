using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EsoAdv.Metadata.Analyzer;
using EsoAdv.Metadata.Model;
using EsoAdv.Metadata.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsoAdv.Metadata.Test
{

    [TestClass]
    public class AddonMetadataCollectionTest
    {
        private string testdataFolder = "./testdata";

        [TestMethod]
        public async Task TestAnalyze()
        {
            var aoColl = await AddOnCollectionParser.ParseFolderAsync(testdataFolder);
            var analyzer = new AddonMetadataAnalyzer(new AnalyzerSettings());
            var issues = analyzer.Analyze(aoColl);
            Assert.IsNotNull(issues, "Collection must not be null");
            Assert.IsTrue(issues.Any(), "Collection must not be empty");
            CollectionAssert.AllItemsAreNotNull(issues, "No items may be null");
        }

    }
}