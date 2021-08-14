using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
// using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EsoAdv.Metadata.Model;
using EsoAdv.Metadata.Parser;

namespace EsoAdv.Cmd
{

    public class Program
    {
        static async Task Main(string[] args)
        {
            // https://github.com/dotnet/command-line-api/blob/main/docs/Your-first-app-with-System-CommandLine.md
            // https://github.com/dotnet/command-line-api/blob/main/docs/model-binding.md
            var rootCommand = new RootCommand {
                new Option<DirectoryInfo>(
                    new [] { "--eso-dir", "-d" },
                    () => new DirectoryInfo(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        @"Elder Scrolls Online\live")),
                    "Elder Scrolls Online directory (not AddOns!) - must exist")
                    .ExistingOnly(),
                new Option<FileInfo>(
                    new [] { "--output", "-o" }
                    , "File to write report to")
                    .LegalFilePathsOnly(),
                new Option<bool>(
                    new[] { "--launch", "-L" }
                    , "Launch report on completion (requires --output)"),
                new Option<bool>(
                    new [] { "--missing-optional", "-O" }
                    , () => false
                    , "Report missing optional dependencies"),
                new Option<bool>(
                    new [] { "--missing-files", "-F" }
                    , () => false
                    , "Report files mentioned in manifest that don't exist"),
                new Option<bool>(
                    new [] { "--missing-version", "-V" }
                    , () => false
                    , "Report missing AddOnVersion manifest directive"),
                new Option<bool>(
                    new[] { "--multiple-instances", "-M" }
                    , () => false
                    , "Report multiple instances of addon-ons")
            };
            rootCommand.Description = "Elder Scrolls Online Add-Ons dependency scanner";
            rootCommand.Handler = CommandHandler.Create(async (
                DirectoryInfo esoDir
                , FileInfo output
                , bool launch
                , bool missingOptional
                , bool missingFiles
                , bool missingVersion
                , bool multipleInstances
                , CancellationToken cancellationToken) =>
            {
                try
                {
                    // Do it
                    Console.WriteLine($"esoDir={esoDir?.FullName}");
                    Console.WriteLine($"output={output?.FullName}");

                    if (esoDir.Exists)
                    {
                        var addonCollection = FileParser.ParseFolder(esoDir.FullName);
                        var issues = addonCollection.Analyze(new AnalyzerSettings()
                        {
                            CheckProvidedFiles = missingFiles,
                            CheckOptionalDependsOn = missingOptional,
                            CheckMultipleInstances = multipleInstances,
                            CheckAddOnVersion = missingVersion
                        });
                        if (issues.Count > 0)
                        {
                            Console.WriteLine($"{issues.Count} Issues were found");
                            if (output == null)
                            {
                                WriteReport(Console.Out, issues);
                            }
                            else
                            {
                                using (var tw = new StreamWriter(output.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)))
                                {
                                    WriteReport(tw, issues);
                                }
                                Console.WriteLine("Wrote report to {0}", output);

                                if (launch)
                                {
                                    System.Diagnostics.Process.Start("explorer.exe", $"\"{output.FullName}\"");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No issues found");
                        }
                    }
                    return 0;
                }
                catch (OperationCanceledException)
                {
                    Console.Error.WriteLine("Interrupted");
                    return 1;
                }
            });
            await rootCommand.InvokeAsync(args);
        }

        public static void WriteReport(TextWriter tw, List<Issue> issues)
        {
            tw.WriteLine("# Issues found");
            foreach (Issue issue in issues)
            {
                tw.WriteLine("{0} - {1}: {2}", issue.AddOnRef, issue.Severity, issue.Message);
            }
            tw.WriteLine("- End of Report -");
            tw.Flush();
        }
    }
}
