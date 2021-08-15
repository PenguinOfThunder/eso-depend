using System.Collections.Generic;

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


        public int? Version => SafeParseInt(this["Version"]);
        public int? AcknowledgedOutOfDateAddonsVersion => SafeParseInt(this["AcknowledgedOutOfDateAddonsVersion"]);

        #endregion

        #region Realm-Character settings

        private readonly Dictionary<string, int?> _realmCharSettings = new();

        public int? this[string realm, string character, string addon]
        {
            get { return _realmCharSettings[$"{realm}-{character}:{addon}"]; }
            set { _realmCharSettings[$"{realm}-{character}:{addon}"] = value; }
        }

        #endregion

        #region Helpers
        private static int? SafeParseInt(string value)
        {
            if (int.TryParse(value, out int intval))
            {
                return intval;
            }
            return null;
        }

        #endregion
    }
}