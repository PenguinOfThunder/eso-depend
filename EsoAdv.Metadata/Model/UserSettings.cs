using System.Collections.Generic;

namespace EsoAdv.Metadata.Model;

/// <summary>Stores settings found in UserSettings.txt</summary>
/// Some settings are exposed as properties for convenience.
public class UserSettings
{
    private readonly Dictionary<string, string> _settings = new();

    public string this[string key]
    {
        get => _settings.GetValueOrDefault(key);
        set => _settings[key] = value;
    }

    public string Language2 => this["Language.2"];

    /// <summary>Last Valid Language, e.g., "en"</summary>
    public string LastValidLanguage => this["LastValidLanguage"];

    /// <summary>Last Platform, e.g., "Live"</summary>
    public string LastPlatform => this["LastPlatform"];

    /// <summary>Last Realm, e.g., "NA Megaserver"</summary>
    public string LastRealm => this["LastRealm"];
}