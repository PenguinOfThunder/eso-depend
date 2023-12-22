using System.Collections.Generic;
using System.IO;
using System.Linq;
using EsoAdv.Metadata.Model;

namespace EsoAdv.Metadata.Analyzer
{
    public class AddonMetadataAnalyzer
    {
        private readonly AnalyzerSettings _settings;

        public AddonMetadataAnalyzer(AnalyzerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Check this add-on collection for issues like missing dependencies</summary>
        /// <param name="settings">Configurable parameters for the analysis</param>
        public List<Issue> Analyze(AddonMetadataCollection addonCollection)
        {
            var addons = addonCollection.AddOns;
            var addOnSettings = addonCollection.AddOnSettings;
            List<Issue> issues = new();
            foreach (AddonMetadata addon in addons.OrderBy(a => a.Path))
            {
                issues.AddRange(ValidateAddOnMetadata(addon));
                issues.AddRange(ValidateAddOnOutdated(addonCollection, addon));
                issues.AddRange(ValidateAddOnDependencies(addonCollection, addon));
                issues.AddRange(ValidateAddOnOptionalDependencies(addonCollection, addon));
                issues.AddRange(ValidateAddOnFiles(addonCollection, addon));
                issues.AddRange(ValidateAddonDisabledEverywhere(addOnSettings, addon));
            }
            issues.AddRange(ValidateMultipleInstances(addonCollection));
            return issues;
        }

        private IEnumerable<Issue> ValidateAddOnFiles(AddonMetadataCollection addonCollection, AddonMetadata addon)
        {
            var addOnSettings = addonCollection.AddOnSettings;
            if (_settings.CheckProvidedFiles)
            {
                if (!addon.ProvidedFiles.Any())
                {
                    yield return new Issue
                    {
                        AddOnRef = addon.Name,
                        Severity = IssueSeverity.Warning,
                        Message = $"{addon.Name} does not list any files to load"
                    };
                }
                // Find referenced files missing
                foreach (string file in addon.ProvidedFiles)
                {
                    string filePath = Path.Combine(addonCollection.BasePath, addon.Directory,
                     AddonMetadata.ExpandFileName(file, addonCollection.UserSettings.LastValidLanguage, addOnSettings.Version));
                    if (!File.Exists(filePath))
                    {
                        yield return new Issue()
                        {
                            Severity = IssueSeverity.Warning,
                            AddOnRef = addon.Name,
                            Message = $"Could not find referenced file: {filePath}"
                        };
                    }
                }
            }
        }

        private IEnumerable<Issue> ValidateAddOnOptionalDependencies(AddonMetadataCollection addonCollection, AddonMetadata addon)
        {
            if (_settings.CheckOptionalDependsOn)
            {
                // Find missing optional dependencies
                foreach (string optDep in addon.OptionalDependsOn)
                {
                    IEnumerable<AddonMetadata> matching = addonCollection.FindMatchingAddons(optDep);
                    if (!matching.Any())
                    {
                        yield return new Issue()
                        {
                            AddOnRef = addon.Name,
                            Message = "Missing optional dependency: " + optDep,
                            Severity = IssueSeverity.Info
                        };
                    }
                }
            }
        }

        private IEnumerable<Issue> ValidateAddOnDependencies(AddonMetadataCollection addonCollection, AddonMetadata addon)
        {
            if (_settings.CheckDependsOn)
            {
                // Find missing required dependencies
                foreach (string reqDep in addon.DependsOn)
                {
                    IEnumerable<AddonMetadata> matching = addonCollection.FindMatchingAddons(reqDep);
                    if (!matching.Any())
                    {
                        yield return new Issue()
                        {
                            AddOnRef = addon.Name,
                            Message = "Missing required dependency: " + reqDep,
                            Severity = IssueSeverity.Error
                        };
                    }
                    else
                    {
                        IEnumerable<AddonMetadata> nonTopLevel = matching.Where(ao => !ao.IsTopLevel);
                        IEnumerable<AddonMetadata> nonBundled = nonTopLevel.Where(ao => addonCollection.GetParentAddon(ao) != addon);
                        if (nonBundled.Any())
                        {
                            yield return new Issue
                            {
                                AddOnRef = addon.Name,
                                Severity = IssueSeverity.Warning,
                                Message = $"{addon.Name} depends on modules that are not bundled with it, and not installed as separate AddOns: " + string.Join(", ", nonBundled)
                            };
                        }
                    }
                }
            }
        }

        private IEnumerable<Issue> ValidateMultipleInstances(AddonMetadataCollection addonCollection)
        {
            var addons = addonCollection.AddOns;
            if (_settings.CheckMultipleInstances)
            {
                Dictionary<string, List<AddonMetadata>> addonsByName = addons
                    .GroupBy(g => g.Name)
                    .ToDictionary(g => g.Key, g => g.ToList());

                IEnumerable<Issue> duplicates = addonsByName
                .Where(g => g.Value.Count > 1)
                .Select(g => new Issue
                {
                    AddOnRef = g.Key,
                    Severity = IssueSeverity.Warning,
                    Message = $"Multiple instances of {g.Key} was found in: " + string.Join(", ", g.Value.Select(a => a.Path))
                });
                // issues.AddRange(duplicates);
                foreach (var duplicate in duplicates)
                {
                    yield return duplicate;
                }
            }
        }

        private IEnumerable<Issue> ValidateAddOnOutdated(AddonMetadataCollection addonCollection, AddonMetadata addon)
        {
            if (_settings.CheckOutdated)
            {
                var addOnSettings = addonCollection.AddOnSettings;
                if (addon.APIVersion != null && addOnSettings.Version != null)
                {
                    if (!addon.APIVersion.Any(v => int.Parse(v) >= addOnSettings.Version))
                    {
                        yield return new Issue
                        {
                            AddOnRef = addon.Name,
                            Severity = IssueSeverity.Info,
                            Message = $"{addon.Name} is outdated - supported version(s): {string.Join('-', addon.APIVersion)}"
                        };
                    }
                }
            }
        }

        private IEnumerable<Issue> ValidateAddOnMetadata(AddonMetadata addon)
        {
            // Metadata problems
            if (_settings.CheckAddOnVersion)
            {
                if (addon.AddOnVersion == null)
                {
                    yield return new Issue()
                    {
                        AddOnRef = addon.Name,
                        Severity = IssueSeverity.Info,
                        Message = $"{addon.Name} is missing the AddOnVersion manifest field"
                    };
                }
            }
        }

        private IEnumerable<Issue> ValidateAddonDisabledEverywhere(AddOnSettings addOnSettings, AddonMetadata addon)
        {
            if (_settings.CheckUnused)
            {
                if (addon.IsTopLevel && !addOnSettings.IsAddonEnabledAnywhere(addon.Name))
                {
                    yield return new Issue()
                    {
                        AddOnRef = addon.Name,
                        Message = "Disabled by all characters",
                        Severity = IssueSeverity.Warning
                    };
                }
            }
        }
    }


}