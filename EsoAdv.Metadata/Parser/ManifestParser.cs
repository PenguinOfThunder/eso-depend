using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EsoAdv.Metadata.Model;

namespace EsoAdv.Metadata.Parser;

public class ManifestParser
{
    private static readonly Regex _directiveRe =
        new(@"^##\s+(?<directive>[a-zA-Z0-9_]+):\s*(?<value>.*)", RegexOptions.Compiled);

    private static readonly Regex _commentRe = new(@"^(\s*)$|^\s*[#;]", RegexOptions.Compiled);

    private static readonly Regex _fileRe = new(@"^\s*(?<file>[^#;].+)", RegexOptions.Compiled);

    public static async Task<AddonMetadata> ParseManifestFileAsync(string filepath,
        CancellationToken cancellationToken = default)
    {
        var metadata = new AddonMetadata();
        foreach (var line in await File.ReadAllLinesAsync(filepath, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            // System.Console.WriteLine("Parse: " + line);
            if (_directiveRe.IsMatch(line))
            {
                // Console.WriteLine("Match directive");
                var mDirective = _directiveRe.Match(line);
                var directive = mDirective.Groups["directive"]?.Value;
                var value = mDirective.Groups["value"]?.Value;
                if (!metadata.Metadata.TryAdd(directive, value))
                    // Directives are allowed to "wrap" by repeating them, so append with a space prepended
                    // Console.WriteLine($"Duplicate directive {directive}={value}");
                    metadata.Metadata[directive] += " " + value;
            }
            else if (_commentRe.IsMatch(line))
            {
                // Skip comments and empty lines
            }
            else if (_fileRe.IsMatch(line))
            {
                // Console.WriteLine("Match file");
                var mFile = _fileRe.Match(line);
                var filename = mFile.Groups["file"]?.Value;
                metadata.ProvidedFiles.Add(filename);
            }
        }

        return metadata.IsValid ? metadata : null;
    }
}