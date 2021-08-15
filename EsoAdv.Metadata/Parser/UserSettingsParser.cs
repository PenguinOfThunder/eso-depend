using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using EsoAdv.Metadata.Model;

namespace EsoAdv.Metadata.Parser
{
    public class UserSettingsParser
    {
        private static readonly Regex _userSettingRe = new Regex(@"^SET\s+(?<key>\S+)\s+(?<value>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static UserSettings ParseUserSettings(string filename)
        {
            var userSettings = new UserSettings();
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
    }
}