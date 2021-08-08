using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsoAdv.Metadata.Test
{
    using EsoAdv.Metadata.Model;
    using EsoAdv.Metadata.Parser;

    [TestClass]
    public class FileParserTest
    {
        private string testdataFolder;

        [TestInitialize]
        public void Setup()
        {
            testdataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Elder Scrolls Online\live");
        }

        [TestMethod]
        public void TestParse()
        {
            var metadatafile = Path.Combine(testdataFolder, @"AddOns\AUI\AUI.txt");
            Assert.IsTrue(File.Exists(metadatafile), "Metadata file does not exist");
            var metadata = FileParser.ParseManifestFile(metadatafile);
            Assert.IsNotNull(metadata, "Must not be null");
            // Assert.IsNotNull(metadata.Path, "Path must not be null");
            Assert.IsNotNull(metadata.Title, "Title", "Must have title");
            Assert.AreEqual(metadata.Author, "|c87ddf2Sensi|r");
            Assert.AreEqual(metadata.Title, @"|c77ee02Advanced UI|r || Version: 3.81");
            CollectionAssert.Contains(metadata.ProvidedFiles, "Templates.xml");
        }

        [TestMethod]
        public void TestParseFolder()
        {
            var addons = FileParser.ParseFolder(testdataFolder);
            Assert.IsNotNull(addons);
            Assert.IsFalse(addons.Count == 0, "Must not be empty");
            var auiAddon = addons.GetAddonsByName("AUI").FirstOrDefault();
            Assert.IsNotNull(auiAddon, "Must find AUI");
            var auiFdAddon = addons.GetAddonsByName("AUI_FightData").FirstOrDefault();
            Assert.IsNotNull(auiFdAddon, "Must find AUI");
            var auiFdparent = addons.GetParentAddon(auiFdAddon);
            Assert.IsNotNull(auiFdparent, "Must have parent");
            Assert.AreSame(auiFdparent, auiAddon, "AUI should be the parent of AUI_FightData");
        }

        [TestMethod]
        public void TestGenerateDot()
        {
            var addons = FileParser.ParseFolder(testdataFolder);
            using var tw = File.CreateText("dependencies.dot");
            tw.WriteLine("digraph dependencies {");
            foreach (var addon in addons.Items)
            {
                tw.WriteLine($"\"{addon.Name}\" [label=\"{addon.Title}\"];");
                foreach (var depSpec in addon.DependsOn)
                {
                    var satisfyingAddon = addons.Items.Where(ao => ao.SatisfiesVersion(depSpec)).FirstOrDefault();
                    if (satisfyingAddon != null)
                    {
                        tw.WriteLine($"\"{satisfyingAddon.Name}\" -> \"{addon.Name}\" [color=red];");
                    }
                    else
                    {
                        tw.WriteLine($"\"{depSpec}\" -> \"{addon.Name}\" [color=red,line=dotted];");
                    }
                }
                foreach (var depSpec in addon.OptionalDependsOn)
                {
                    tw.WriteLine($"\"{depSpec}\" -> \"{addon.Name}\" [color=blue];");
                    var satisfyingAddon = addons.Items.Where(ao => ao.SatisfiesVersion(depSpec)).FirstOrDefault();
                    if (satisfyingAddon != null)
                    {
                        tw.WriteLine($"\"{satisfyingAddon.Name}\" -> \"{addon.Name}\" [color=blue];");
                    }
                }
            }
            tw.WriteLine("}");
            tw.Flush();
        }

        [TestMethod]
        public void TestGenerateReport()
        {
            var addonCollection = FileParser.ParseFolder(testdataFolder);
            var issues = addonCollection.Analyze();
            var reportFile = "addons_report.txt";
            using var tw = new StreamWriter(reportFile);
            tw.WriteLine("# Issues found");
            foreach (Issue issue in issues)
            {
                tw.WriteLine("{0} - {1}: {2}", issue.AddOnRef, issue.Severity, issue.Message);
            }
            tw.WriteLine("- End of Report -");
        }
    }
}
