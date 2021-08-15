using System;
using System.IO;

namespace EsoAdv.Metadata.Parser
{
    using EsoAdv.Metadata.Model;

    /// Parse ESO module manifest format https://wiki.esoui.com/Addon_manifest_(.txt)_format
    public static class AddOnCollectionParser
    {
        public static AddonMetadataCollection ParseFolder(string esoPath)
        {
            var userSettingsFile = Path.Combine(esoPath, "UserSettings.txt");
            var userSettings = UserSettingsParser.ParseUserSettings(userSettingsFile);
            var addOnSettingsFile = Path.Combine(esoPath, "AddOnSettings.txt");
            var addOnSettings = AddOnSettingsParser.ParseAddOnSettings(addOnSettingsFile);
            var addOnsPath = Path.Combine(esoPath, "AddOns");

            // Scan for addon folders
            var addonCollection = new AddonMetadataCollection
            {
                BasePath = addOnsPath,
                UserSettings = userSettings,
                AddOnSettings = addOnSettings
            };
            var oldCwd = Environment.CurrentDirectory;
            Environment.CurrentDirectory = addOnsPath;
            foreach (var txtfile in Directory.EnumerateFiles(".", "*.txt",
                new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple
                })
            )
            {
                var fileBasename = Path.GetFileNameWithoutExtension(txtfile);
                var dirBasename = Path.GetFileName(Path.GetDirectoryName(txtfile));
                var parentDir = Path.GetDirectoryName(dirBasename);
                // Only manifests where the filename matches the parent directory name are used
                if (0 == string.Compare(fileBasename, dirBasename, StringComparison.InvariantCultureIgnoreCase))
                {
                    var metadata = ManifestParser.ParseManifestFile(txtfile);
                    if (metadata != null)
                    {
                        metadata.Path = Path.GetRelativePath(addOnsPath, txtfile);
                        metadata.IsTopLevel = parentDir == string.Empty;
                        addonCollection.Add(metadata);
                    }
                }
            }
            Environment.CurrentDirectory = oldCwd;
            return addonCollection;
        }
    }
}
