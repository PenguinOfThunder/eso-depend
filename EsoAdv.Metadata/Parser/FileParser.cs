using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EsoAdv.Metadata.Parser
{
    using EsoAdv.Metadata.Model;

    /// Parse ESO module manifest format https://wiki.esoui.com/Addon_manifest_(.txt)_format
    public static class FileParser
    {
        // private static string[] _multivalued = new[] { "DependsOn", "OptionalDependsOn" };
        private static readonly Regex _directiveRe = new Regex(@"^##\s+(?<directive>[a-zA-Z0-9_]+):\s*(?<value>.*)", RegexOptions.Compiled);

        private static readonly Regex _commentRe = new Regex(@"^(\s*)$|(^\s*#[^#;].*)", RegexOptions.Compiled);

        private static readonly Regex _fileRe = new Regex(@"^\s*(?<file>[^#;].+)", RegexOptions.Compiled);

        public static AddonMetadata Parse(TextReader tr)
        {
            var metadata = new AddonMetadata();
            string line;
            while (null != (line = tr.ReadLine()))
            {
                // Console.WriteLine("Parse: " + line);
                if (_directiveRe.IsMatch(line))
                {
                    // Console.WriteLine("Match directive");
                    var mDirective = _directiveRe.Match(line);
                    var directive = mDirective.Groups["directive"]?.Value;
                    var value = mDirective.Groups["value"]?.Value;
                    if (!metadata.Metadata.TryAdd(directive, value))
                    {
                        // Directives are allowed to "wrap" by repeating them, so append with a space prepended
                        // Console.WriteLine($"Duplicate directive {directive}={value}");
                        metadata.Metadata[directive] += " " + value;
                    }
                }
                else if (_commentRe.IsMatch(line))
                {
                    // Skip comments and empty lines
                    continue;
                }
                else if (_fileRe.IsMatch(line))
                {
                    // Console.WriteLine("Match file");
                    var mFile = _fileRe.Match(line);
                    var filename = mFile.Groups["file"]?.Value;
                    metadata.ProvidedFiles.Add(filename);
                }
            }
            return metadata.IsValid ? metadata : null;
        }

        public static AddonMetadata Parse(string basePath, string filepath)
        {
            using var st = new FileStream(filepath, FileMode.Open);
            using var sr = new StreamReader(st);
            var md = Parse(sr);
            if (md != null)
            {
                md.Path = Path.GetRelativePath(basePath, filepath);
            }
            return md;
        }

        public static AddonMetadataCollection ParseFolder(string folderPath)
        {
            var addonCollection = new AddonMetadataCollection
            {
                BasePath = folderPath
            };
            var oldCwd = Environment.CurrentDirectory;
            Environment.CurrentDirectory = folderPath;
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
                var metadata = Parse(folderPath, txtfile);
                if (metadata != null)
                {
                    addonCollection.Add(metadata);
                }
            }
            Environment.CurrentDirectory = oldCwd;
            return addonCollection;
        }
    }
}
