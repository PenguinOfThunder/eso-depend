using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EsoAdv.Metadata.Model
{
    public class AddonMetadataCollection
    {
        public string BasePath { get; set; }
        public UserSettings UserSettings { get; set; }
        public AddOnSettings AddOnSettings { get; set; }
        private readonly List<AddonMetadata> _addons = new();
        public int Count => _addons.Count;

        public List<AddonMetadata> AddOns
        {
            get
            {
                return _addons;
            }
        }
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
                if (parentDir == null)
                {
                    return null;
                }

                AddonMetadata parentAddon = _addons
.FirstOrDefault(ao => parentDir.StartsWith(Path.GetFullPath(ao.Directory)));
                if (parentAddon != null)
                {
                    return parentAddon;
                }
            }
            return null;
        }


        /// <summary>Get list of addons that depend on another</summary>    
        public IEnumerable<string> GetDependents(AddonMetadata origin)
        {
            return _addons.Where(ao =>
                    ao.DependsOn
                    .Concat(ao.OptionalDependsOn)
                    .Any(dep => origin.SatisfiesVersion(dep))
                ).Select(ao => ao.Name);
        }

        public IEnumerable<AddonMetadata> FindMatchingAddons(string addOnRef)
        {
            return _addons.Where(ao => ao.SatisfiesVersion(addOnRef));
        }

    }
}