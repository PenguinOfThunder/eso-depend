using System;
using System.IO;
using EsoAdv.Metadata.Model;

namespace EsoAdv.Metadata.Parser
{
    public class AddOnSettingsParser
    {
        public static AddOnSettings ParseAddOnSettings(string filename)
        {
            var settings = new AddOnSettings();
            using var st = new FileStream(filename, FileMode.Open);
            using var sr = new StreamReader(st);
            string line;
            string realm = string.Empty;
            string character = string.Empty;
            while (null != (line = sr.ReadLine()))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                else if (line.StartsWith("#"))
                {
                    var content = line[1..];
                    if (content.Contains("Megaserver"))
                    {
                        var rc = content.Split('-', 2);
                        realm = rc[0];
                        character = rc[1];
                    }
                    else
                    {
                        var kv = content.Split(" ", 2,
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var k = kv[0];
                        var v = kv[1];
                        settings[k] = v;
                    }
                }
                else
                {
                    var kv = line.Split(" ", 2,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var v = kv[1];
                    if (int.TryParse(kv[1], out int enabled))
                    {
                        settings[realm, character, kv[0]] = enabled;
                    }
                }
            }
            return settings;
        }
    }
}