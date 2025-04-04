using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EsoAdv.Metadata.Analyzer;
using EsoAdv.Metadata.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsoAdv.Metadata.Test;

[TestClass]
public class FileParserTest
{
    private readonly string testdataFolder = "testdata";

    [TestMethod]
    public async Task TestParse()
    {
        var metadatafile = Path.Combine(testdataFolder, @"AddOns/AUI/AUI.txt");
        Assert.IsTrue(File.Exists(metadatafile), "Metadata file `{0}` does not exist", metadatafile);
        var metadata = await ManifestParser.ParseManifestFileAsync(metadatafile);
        Assert.IsNotNull(metadata, "Must not be null");
        // Assert.IsNotNull(metadata.Path, "Path must not be null");
        Assert.IsNotNull(metadata.Title, "Title", "Must have title");
        Assert.AreEqual("|c87ddf2Sensi|r", metadata.Author, "Author name mismatch");
        Assert.AreEqual(@"|c77ee02Advanced UI|r || Version: 3.86", metadata.Title, "Title mismatch");
        CollectionAssert.Contains(metadata.ProvidedFiles, "Templates.xml");
    }

    [TestMethod]
    public async Task TestParseFolder()
    {
        var addons = await AddOnCollectionParser.ParseFolderAsync(testdataFolder);
        Assert.IsNotNull(addons);
        Assert.IsFalse(addons.Count == 0, "Must not be empty");
        var auiAddon = addons.GetAddonsByName("AUI").FirstOrDefault();
        Assert.IsNotNull(auiAddon, "Must find AUI");
        var auiFdAddon = addons.GetAddonsByName("AUI_FightData").FirstOrDefault();
        Assert.IsNotNull(auiFdAddon, "Must find AUI_FightData");
        var auiFdParent = addons.GetParentAddon(auiFdAddon);
        Assert.IsNotNull(auiFdParent, "Must have parent");
        Assert.AreSame(auiFdParent, auiAddon, "AUI should be the parent of AUI_FightData");
    }

    [TestMethod]
    public async Task TestGenerateDot()
    {
        var addons = await AddOnCollectionParser.ParseFolderAsync(testdataFolder);
        using var tw = File.CreateText("dependencies.dot");
        tw.WriteLine("digraph dependencies {");
        foreach (var addon in addons.Items)
        {
            tw.WriteLine($"\"{addon.Name}\" [label=\"{addon.Title}\"];");
            foreach (var depSpec in addon.DependsOn)
            {
                var satisfyingAddon = addons.Items.Where(ao => ao.SatisfiesVersion(depSpec)).FirstOrDefault();
                if (satisfyingAddon != null)
                    tw.WriteLine($"\"{satisfyingAddon.Name}\" -> \"{addon.Name}\" [color=red];");
                else
                    tw.WriteLine($"\"{depSpec}\" -> \"{addon.Name}\" [color=red,line=dotted];");
            }

            foreach (var depSpec in addon.OptionalDependsOn)
            {
                tw.WriteLine($"\"{depSpec}\" -> \"{addon.Name}\" [color=blue];");
                var satisfyingAddon = addons.Items.Where(ao => ao.SatisfiesVersion(depSpec)).FirstOrDefault();
                if (satisfyingAddon != null)
                    tw.WriteLine($"\"{satisfyingAddon.Name}\" -> \"{addon.Name}\" [color=blue];");
            }
        }

        tw.WriteLine("}");
        tw.Flush();
    }

    [TestMethod]
    public async Task TestGenerateReport()
    {
        var addonCollection = await AddOnCollectionParser.ParseFolderAsync(testdataFolder);
        var analyzer = new AddonMetadataAnalyzer(new AnalyzerSettings());
        var issues = analyzer.Analyze(addonCollection);
        var reportFile = "addons_report.txt";
        using var tw = new StreamWriter(reportFile);
        tw.WriteLine("# Issues found");
        foreach (var issue in issues) tw.WriteLine("{0} - {1}: {2}", issue.AddOnRef, issue.Severity, issue.Message);
        tw.WriteLine("- End of Report -");
    }
}
