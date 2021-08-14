﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EsoAdv.Metadata.Parser
{
    using System.Collections.Generic;
    using EsoAdv.Metadata.Model;

    /// Parse ESO module manifest format https://wiki.esoui.com/Addon_manifest_(.txt)_format
    public static class FileParser
    {
        private static readonly Regex _directiveRe = new Regex(@"^##\s+(?<directive>[a-zA-Z0-9_]+):\s*(?<value>.*)", RegexOptions.Compiled);

        private static readonly Regex _commentRe = new Regex(@"^(\s*)$|^\s*[#;]", RegexOptions.Compiled);

        private static readonly Regex _fileRe = new Regex(@"^\s*(?<file>[^#;].+)", RegexOptions.Compiled);

        private static readonly Regex _userSettingRe = new Regex(@"^SET\s+(?<key>\S+)\s+(?<value>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static AddonMetadata ParseManifestFile(string filepath)
        {
            using var st = new FileStream(filepath, FileMode.Open);
            using var sr = new StreamReader(st);
            var metadata = new AddonMetadata();
            string line;
            while (null != (line = sr.ReadLine()))
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

        public static Dictionary<string, string> ParseAddOnSettings(string filename)
        {
            var settings = new Dictionary<string, string>();
            using var st = new FileStream(filename, FileMode.Open);
            using var sr = new StreamReader(st);
            string line;
            while (null != (line = sr.ReadLine()))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else if (line.StartsWith("#"))
                {
                    var content = line[2..];
                    if (!content.Contains("Megaserver"))
                    {
                        var kv = content.Split(" ", 2,
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        settings.Add(kv[0], kv[1]);
                    }
                }
            }
            return settings;
        }

        public static Dictionary<string, string> ParseUserSettings(string filename)
        {
            var userSettings = new Dictionary<string, string>();
            using var st = new FileStream(filename, FileMode.Open);
            using var sr = new StreamReader(st);
            string line;
            while (null != (line = sr.ReadLine()))
            {
                var m = _userSettingRe.Match(line);
                if (m.Success)
                {
                    var key = m.Groups.GetValueOrDefault("key")?.Value;
                    var value = m.Groups.GetValueOrDefault("value")?.Value;
                    userSettings[key] = value?.Trim('"', ' ');
                }
            }
            return userSettings;
        }

        public static AddonMetadataCollection ParseFolder(string esoPath)
        {
            var userSettingsFile = Path.Combine(esoPath, "UserSettings.txt");
            var userSettings = ParseUserSettings(userSettingsFile);
            var addOnSettingsFile = Path.Combine(esoPath, "AddOnSettings.txt");
            var addOnSettings = ParseAddOnSettings(addOnSettingsFile);
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
                    var metadata = ParseManifestFile(txtfile);
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
