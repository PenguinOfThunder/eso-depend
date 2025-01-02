using System.Collections.Generic;
using EsoAdv.Metadata.Util;

namespace EsoAdv.Metadata.Model;

/// <summary>Settings from AddOnSettings.txt</summary>
/// Some settings are exposed for convenience.
public class AddOnSettings
{
    #region Global Settings

    private readonly Dictionary<string, string> _settings = new();

    private readonly IDictionary<string, HashSet<string>>
        _realmCharacterMap = new Dictionary<string, HashSet<string>>();

    public string this[string key]
    {
        get => _settings.GetValueOrDefault(key);
        set => _settings[key] = value;
    }


    /// <summary>Current API version</summary>
    public int? Version => this["Version"].SafeParseInt();

    /// <summary>Last version to acknowledge outdated addons</summary>
    public int? AcknowledgedOutOfDateAddonsVersion => this["AcknowledgedOutOfDateAddonsVersion"].SafeParseInt();

    /// <summary>Addons are enabled globally</summary>
    public bool AddOnsEnabled => this["AddOnsEnabled"].SafeParseInt() == 1;

    #endregion

    #region Realm-Character settings

    private readonly Dictionary<string, int?> _realmCharSettings = new();

    private static string GetPerCharacterAddonKey(string realm, string character, string addon)
    {
        return $"{realm}-{character}:{addon}";
    }

    public int? this[string realm, string character, string addon]
    {
        get => _realmCharSettings[GetPerCharacterAddonKey(realm, character, addon)];
        set
        {
            _realmCharacterMap.TryAdd(realm, new HashSet<string>());
            _realmCharacterMap[realm].Add(character);
            _realmCharSettings[GetPerCharacterAddonKey(realm, character, addon)] = value;
        }
    }

    public IEnumerable<string> GetRealms()
    {
        return _realmCharacterMap.Keys;
    }

    public IEnumerable<string> GetRealmCharacters(string realm)
    {
        return _realmCharacterMap[realm];
    }

    public bool IsAddonEnabledAnywhere(string addon)
    {
        var seen = false;
        // These include the empty realm and character key
        foreach (var realm in GetRealms())
        foreach (var character in GetRealmCharacters(realm))
            if (_realmCharSettings.TryGetValue(GetPerCharacterAddonKey(realm, character, addon), out var value))
            {
                seen = true;
                if (value == 1)
                    // definitively used (true)
                    return true;
            }

        // If we did not see it mentioned, consider it enabled (true).
        // If we did see it and still ended up here, it was disabled everywhere (false)
        return !seen;
    }

    #endregion
}