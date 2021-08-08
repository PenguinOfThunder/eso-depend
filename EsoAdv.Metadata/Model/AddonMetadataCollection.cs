namespace EsoAdv.Metadata.Model
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public class AddonMetadataCollection
    {
        public string BasePath { get; set; }
        public string CurrentGameVersion
        {
            get
            {
                return AddOnSettings?.GetValueOrDefault("Version");
            }
        }
        public string ClientLanguage
        {
            get
            {
                return UserSettings?.GetValueOrDefault("LastValidLanguage") ?? "en";
            }
        }
        public Dictionary<string, string> UserSettings { get; set; }
        public Dictionary<string, string> AddOnSettings { get; set; }
        private readonly List<AddonMetadata> _addons = new();
        public int Count => _addons.Count;

        public void Add(AddonMetadata metadata)
        {
            _addons.Add(metadata);
        }

        public void AddRange(IEnumerable<AddonMetadata> addons)
        {
            _addons.AddRange(addons);
        }

        public IList<AddonMetadata> Items => _addons.AsReadOnly();

        public IList<AddonMetadata> GetAddonsByName(string addonName)
        {
            return _addons.Where(a => a.Name == addonName).ToList().AsReadOnly();
        }

        public AddonMetadata GetParentAddon(AddonMetadata origin)
        {
            string parentDir = Path.GetFullPath(origin.Directory);
            while (parentDir != null && parentDir != BasePath)
            {
                parentDir = Directory.GetParent(parentDir)?.FullName;
                if (parentDir == null) return null;
                var parentAddon = _addons
                    .Where(ao => parentDir.StartsWith(Path.GetFullPath(ao.Directory)))
                    .FirstOrDefault();
                if (parentAddon != null)
                {
                    return parentAddon;
                }
            }
            return null;
        }

        public IEnumerable<AddonMetadata> FindMatchingAddons(string addOnRef)
        {
            return _addons.Where(ao => ao.SatisfiesVersion(addOnRef));
        }

        /// <summary>Check this add-on collection for issues like missing dependencies</summary>
        public List<Issue> Analyze()
        {
            var issues = new List<Issue>();

            foreach (var addon in _addons.OrderBy(a => a.Path))
            {
                // Metadata problems
                if (addon.AddOnVersion == null)
                {
                    // This shouldn't happen, because we are using this to determine if a file is valid,
                    // but it's here in case that changes
                    issues.Add(new Issue()
                    {
                        AddOnRef = addon.Name,
                        Severity = IssueSeverity.Info,
                        Message = $"{addon.Name} is missing the AddOnVersion manifest field"
                    });
                }
                if (!addon.ProvidedFiles.Any())
                {
                    issues.Add(new Issue
                    {
                        AddOnRef = addon.Name,
                        Severity = IssueSeverity.Warning,
                        Message = $"{addon.Name} does not list any files to load"
                    });
                }

                // Find missing required dependencies
                foreach (var reqDep in addon.DependsOn)
                {
                    var matching = FindMatchingAddons(reqDep);
                    if (!matching.Any())
                    {
                        issues.Add(new Issue()
                        {
                            AddOnRef = addon.Name,
                            Message = "Missing required dependency: " + reqDep,
                            Severity = IssueSeverity.Error
                        });
                    }
                    else
                    {
                        var nonTopLevel = matching.Where(ao => !ao.IsTopLevel);
                        var nonBundled = nonTopLevel.Where(ao => GetParentAddon(ao) != addon);
                        if (nonBundled.Any())
                        {
                            issues.Add(new Issue
                            {
                                AddOnRef = addon.Name,
                                Severity = IssueSeverity.Warning,
                                Message = $"{addon.Name} depends on modules that are not bundled with it, and not installed as separate AddOns: " + string.Join(", ", nonBundled)
                            });
                        }
                    }
                }
                // Find missing optional dependencies
                foreach (var optDep in addon.OptionalDependsOn)
                {
                    var matching = FindMatchingAddons(optDep);
                    if (!matching.Any())
                    {
                        issues.Add(new Issue()
                        {
                            AddOnRef = addon.Name,
                            Message = "Missing optional dependency: " + optDep,
                            Severity = IssueSeverity.Info
                        });
                    }
                }
                // Find referenced files missing
                foreach (var file in addon.ProvidedFiles)
                {
                    var filePath = Path.Combine(BasePath, addon.Directory,
                     AddonMetadata.ExpandendFileName(file, ClientLanguage, CurrentGameVersion));
                    if (!File.Exists(filePath))
                    {
                        issues.Add(new Issue()
                        {
                            Severity = IssueSeverity.Warning,
                            AddOnRef = addon.Name,
                            Message = $"Could not find referenced file: {filePath}"
                        });
                    }
                }
            }

            var addonsByName = _addons
                .GroupBy(g => g.Name)
                .ToDictionary(g => g.Key, g => g.ToList());

            var duplicates = addonsByName
            .Where(g => g.Value.Count > 1)
            .Select(g => new Issue
            {
                AddOnRef = g.Key,
                Severity = IssueSeverity.Warning,
                Message = $"Multiple instances of {g.Key} was found in: " + string.Join(", ", g.Value.Select(a => a.Path))
            });
            issues.AddRange(duplicates);

            return issues;
        }
    }

}