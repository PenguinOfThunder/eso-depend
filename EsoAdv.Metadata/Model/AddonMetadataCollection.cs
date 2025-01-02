using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EsoAdv.Metadata.Model;

public class AddonMetadataCollection
{
    public string BasePath { get; set; }
    public UserSettings UserSettings { get; set; }
    public AddOnSettings AddOnSettings { get; set; }
    public int Count => AddOns.Count;

    public List<AddonMetadata> AddOns { get; } = new();

    public IList<AddonMetadata> Items => AddOns.AsReadOnly();

    public void Add(AddonMetadata metadata)
    {
        AddOns.Add(metadata);
    }

    public void AddRange(IEnumerable<AddonMetadata> addons)
    {
        AddOns.AddRange(addons);
    }

    public IList<AddonMetadata> GetAddonsByName(string addonName)
    {
        return AddOns.Where(a => a.Name == addonName).ToList().AsReadOnly();
    }

    public AddonMetadata GetParentAddon(AddonMetadata origin)
    {
        var parentDir = Path.GetFullPath(origin.Directory);
        while (parentDir != null && parentDir != BasePath)
        {
            parentDir = Directory.GetParent(parentDir)?.FullName;
            if (parentDir == null) return null;

            var parentAddon = AddOns
                .FirstOrDefault(ao => parentDir.StartsWith(Path.GetFullPath(ao.Directory)));
            if (parentAddon != null) return parentAddon;
        }

        return null;
    }


    /// <summary>Get list of addons that depend on another</summary>
    public IEnumerable<string> GetDependents(AddonMetadata origin)
    {
        return AddOns.Where(ao =>
            ao.DependsOn
                .Concat(ao.OptionalDependsOn)
                .Any(dep => origin.SatisfiesVersion(dep))
        ).Select(ao => ao.Name);
    }

    public IEnumerable<AddonMetadata> FindMatchingAddons(string addOnRef)
    {
        return AddOns.Where(ao => ao.SatisfiesVersion(addOnRef));
    }
}