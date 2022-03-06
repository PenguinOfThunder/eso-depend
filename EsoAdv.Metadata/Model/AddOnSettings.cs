using System.Collections.Generic;
using EsoAdv.Metadata.Util;

namespace EsoAdv.Metadata.Model
{
    /// <summary>Settings from AddOnSettings.txt</summary>
    /// Some settings are exposed for convenience.
    public class AddOnSettings
    {

        #region Global Settings

        private readonly Dictionary<string, string> _settings = new();

        public string this[string key]
        {
            get { return _settings.GetValueOrDefault(key); }
            set { _settings[key] = value; }
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

        private static string GetPerCharacterAddonKey(string realm, string character, string addon) => $"{realm}-{character}:{addon}";

        public int? this[string realm, string character, string addon]
        {
            get { return _realmCharSettings[GetPerCharacterAddonKey(realm, character, addon)]; }
            set { _realmCharSettings[GetPerCharacterAddonKey(realm, character, addon)] = value; }
        }

        #endregion

    }
}