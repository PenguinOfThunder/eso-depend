using System;
using System.Collections.Concurrent;
using System.IO;

namespace EsoAdv.Metadata.Parser
{
    using System.Threading;
    using System.Threading.Tasks;
    using EsoAdv.Metadata.Model;

    /// Parse ESO module manifest format https://wiki.esoui.com/Addon_manifest_(.txt)_format
    public static class AddOnCollectionParser
    {
        public static async Task<AddonMetadataCollection> ParseFolderAsync(string esoPath, CancellationToken cancellationToken = default)
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
            var paths = Directory.EnumerateFiles(".", "*.txt",
                new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple
                });
            var addonMetadataList = new ConcurrentBag<AddonMetadata>();
            // Scan the files in parallel
            await Parallel.ForEachAsync(
                paths,
                cancellationToken,
                async (path, token) =>
                {
                    var metadata = await ProcessFile(addOnsPath, path, token);
                    if (metadata != null)
                        addonMetadataList.Add(metadata);
                }
            );
            addonCollection.AddRange(addonMetadataList);
            Environment.CurrentDirectory = oldCwd;
            return addonCollection;
        }

        private static async Task<AddonMetadata> ProcessFile(string addOnsPath, string txtfile, CancellationToken cancellationToken)
        {
            // Console.WriteLine($"Parsing {txtfile}");
            var fileBasename = Path.GetFileNameWithoutExtension(txtfile);
            var dirBasename = Path.GetFileName(Path.GetDirectoryName(txtfile));
            var parentDir = Path.GetDirectoryName(dirBasename);
            // Only manifests where the filename matches the parent directory name are used
            if (0 == string.Compare(fileBasename, dirBasename, StringComparison.InvariantCultureIgnoreCase))
            {
                var metadata = await ManifestParser.ParseManifestFileAsync(txtfile, cancellationToken);
                if (metadata != null)
                {
                    metadata.Path = Path.GetRelativePath(addOnsPath, txtfile);
                    metadata.IsTopLevel = parentDir == string.Empty;
                    return metadata;
                }
            }
            return null;
        }

    }
}
