using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
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
                    () => new[] {
                        new DirectoryInfo(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        @"Elder Scrolls Online\live"))
                    }.FirstOrDefault(di => di.Exists),
                    @"The ESO environment directory, e.g., My Documents\Elder Scrolls Online\live")
                    .ExistingOnly(),
                new Option<FileInfo>(
                    new[] { "--output", "-o" }
                    , "File to write report to")
                    .LegalFilePathsOnly(),
                new Option<bool>(
                    new[] { "--launch", "-L" }
                    , "Launch report on completion (requires --output)"),
                new Option<bool>(
                    new[] { "--missing-optional", "-O" }
                    , () => false
                    , "Report missing optional dependencies"),
                new Option<bool>(
                    new[] { "--missing-files", "-F" }
                    , () => false
                    , "Report files mentioned in manifest that don't exist"),
                new Option<bool>(
                    new[] { "--missing-version", "-V" }
                    , () => false
                    , "Report missing AddOnVersion manifest directive"),
                new Option<bool>(
                    new[] { "--multiple-instances", "-M" }
                    , () => false
                    , "Report multiple instances of addon-ons"),
                new Option<bool>(
                    "--dump", "Dump the information scanned from the discovered manifests"
                )
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
                , bool dump
                , CancellationToken cancellationToken) =>
            {
                try
                {
                    if (esoDir.Exists)
                    {
                        Console.WriteLine("Scanning ESO folder {0}...", esoDir.FullName);
                        var addonCollection = FileParser.ParseFolder(esoDir.FullName);
                        var issues = addonCollection.Analyze(new AnalyzerSettings()
                        {
                            CheckProvidedFiles = missingFiles,
                            CheckOptionalDependsOn = missingOptional,
                            CheckMultipleInstances = multipleInstances,
                            CheckAddOnVersion = missingVersion
                        });
                        if (dump)
                        {
                            WriteDump(Console.Out, addonCollection);
                        }

                        if (issues.Count > 0)
                        {
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
                        return 0;
                    }
                    else
                    {
                        Console.Error.WriteLine("Directory was not found: {0}", esoDir.FullName);
                        return 2;
                    }
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
            foreach (Issue issue in issues)
            {
                tw.WriteLine("{0} - {1}: {2}", issue.AddOnRef, issue.Severity, issue.Message);
            }
            tw.WriteLine("{0} issues were found", issues.Count);
        }

        public static void WriteDump(TextWriter tw, AddonMetadataCollection addons)
        {
            // TODO - serialize to JSON?
            foreach (AddonMetadata addon in addons.Items)
            {
                tw.WriteLine($"# {addon.Name}");
                var dependencies = addon.DependsOn;
                if (dependencies.Length > 0)
                {
                    tw.WriteLine("## Depends On");
                    foreach (var dep in dependencies)
                    {
                        tw.WriteLine($"- {dep}");
                    }
                }
                var optDependencies = addon.OptionalDependsOn;
                if (optDependencies.Length > 0)
                {
                    tw.WriteLine("## Optionally Depends On");
                    foreach (var dep in optDependencies)
                    {
                        tw.WriteLine($"- {dep}");
                    }
                }

            }
        }
    }
}
