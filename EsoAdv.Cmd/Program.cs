using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EsoAdv.Metadata.Analyzer;
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
            var esoDirOption = new Option<DirectoryInfo>(
                    new[] { "--eso-dir", "-d" },
                    () => new[] {
                        // NA megaserver
                        new DirectoryInfo(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            @"Elder Scrolls Online\live")),
                        // EU Megaserver
                        new DirectoryInfo(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            @"Elder Scrolls Online\liveeu")),
                        // PTS
                        new DirectoryInfo(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            @"Elder Scrolls Online\pts")
                        )
                    }.FirstOrDefault(di => di.Exists),
                    "The ESO platform directory (where UserSettings.txt is), e.g., \"My Documents\\Elder Scrolls Online\\live\". (The default probably only works on Windows.)")
                    .ExistingOnly();
            var outputOpt = new Option<FileInfo>(
                            new[] { "--output", "-o" }
                            , "File to write report to")
                            .LegalFilePathsOnly();
            var launchOpt = new Option<bool>(
                                new[] { "--launch", "-L" }
                                , "Launch report on completion (requires --output)");
            var outdatedOpt = new Option<bool>(
                    new[] { "--outdated", "-O" },
                    () => true,
                    "Report outdated addons"
                );
            var missingOptionalOpt = new Option<bool>(
                new[] { "--missing-optional", "-D" }
                , () => false
                , "Report missing optional dependencies");

            var missingFilesOpt = new Option<bool>(
                    new[] { "--missing-files", "-F" }
                    , () => false
                    , "Report files mentioned in manifest that don't exist");

            var missingVersionOpt = new Option<bool>(
                    new[] { "--missing-version", "-V" }
                    , () => false
                    , "Report missing AddOnVersion manifest directive");
            var multipleInstancesOpt = new Option<bool>(
                                new[] { "--multiple-instances", "-M" }
                                , () => false
                                , "Report multiple instances of addon-ons");
            var dumpOpt = new Option<bool>(
                    "--dump", "Dump the information scanned from the discovered manifests"
                );
            // Build root command
            var rootCommand = new RootCommand("Elder Scrolls Online Add-Ons dependency scanner") {
                esoDirOption,
                outputOpt,
                launchOpt,
                outdatedOpt,
                missingOptionalOpt,
                missingFilesOpt,
                missingVersionOpt,
                multipleInstancesOpt,
                dumpOpt
            };

            var handler = new Action<DirectoryInfo, FileInfo, bool, bool, bool, bool, bool, bool, bool, InvocationContext, CancellationToken>(Run);
            rootCommand.SetHandler(handler
                , esoDirOption, outputOpt, launchOpt
                , missingOptionalOpt, missingFilesOpt, missingVersionOpt
                , multipleInstancesOpt, outdatedOpt, dumpOpt);

            await rootCommand.InvokeAsync(args);
        }

        public static void Run(
                         DirectoryInfo esoDir
                        , FileInfo output
                        , bool launch
                        , bool missingOptional
                        , bool missingFiles
                        , bool missingVersion
                        , bool multipleInstances
                        , bool outdated
                        , bool dump
                        , InvocationContext context
                        , CancellationToken cancellationToken)
        {
            try
            {
                if (esoDir.Exists)
                {
                    Console.WriteLine("Scanning ESO folder {0}...", esoDir.FullName);
                    var addonCollection = AddOnCollectionParser.ParseFolder(esoDir.FullName);
                    var analyzer = new AddonMetadataAnalyzer(new AnalyzerSettings()
                    {
                        CheckOutdated = outdated,
                        CheckProvidedFiles = missingFiles,
                        CheckOptionalDependsOn = missingOptional,
                        CheckMultipleInstances = multipleInstances,
                        CheckAddOnVersion = missingVersion
                    });
                    var issues = analyzer.Analyze(addonCollection);
                    if (dump)
                    {
                        WriteDump(Console.Out, addonCollection);
                    }

                    if (issues.Count() > 0)
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
                    context.ExitCode = 0;
                    // return 0;
                }
                else
                {
                    Console.Error.WriteLine("Directory was not found: {0}", esoDir.FullName);
                    // return 2;
                    context.ExitCode = 2;
                }
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Interrupted");
                // return 1;
                context.ExitCode = 1;
            }

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
                var dependents = addons.GetDependents(addon).ToList();
                if (dependents.Any())
                {
                    tw.WriteLine("## Used by");
                    foreach (var dep in dependents)
                    {
                        tw.WriteLine($"- {dep}");
                    }
                }
                tw.WriteLine();
            }
        }
    }
}
