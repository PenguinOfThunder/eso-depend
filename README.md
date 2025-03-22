# Elder Scrolls Online Add-On Dependency Visualizer (ESO-ADV)

## Description

**Note: You have have the [dotnet](https://dotnet.microsoft.com/) SDK installed and be comfortable with the command-line and how to use the dotnet command-line interface. There is no GUI component (yet).**

A command-line application that scans your Addons folder, then creates a dependency graph of the add-ons it found, allowing you to:

- create a list of your installed add-ons
- visualize the relationship between them to determine if you are missing a dependency, or if you have conflicts

TL;DR: It's a linter for your addons.

The application has been developed and tested on Windows and Linux.

## Running

To show all available options:

```shell
dotnet run --project EsoAdv.Cmd -- --help
```

See the scripts [publish.sh](./publish.sh) and [publish.ps1](./publish.ps1) for how to create a standalone executable for Linux and Windows, respectively.

## Development

(This project was developed with the dotnet SDK and Visual Studio Code, which is why some vscode settings are in the repo.)

Pull requests are welcome.

The application is divided into multiple parts:

- Metadata parser: module that parses metadata (.txt) files inside AddOns folder
- Dependency graph resolver: works on the metadata and creates a directed graph of dependencies from it. It also tries to find issues, like missing dependencies.
- Dependency reporter: app that reports on the dependencies and issues

## Dependencies

The application and libraries are written in .Net 8. The installer bundles the necessary libraries and runtime.

Third-party libraries used:

Main executable:

- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine)

Test project only:

- [MSTest.TestAdapter](https://www.nuget.org/packages/MSTest.TestAdapter)
- [MSTest.TestFramework](https://www.nuget.org/packages/MSTest.TestFramework)
- [coverlet.collector](https://github.com/coverlet-coverage/coverlet)

## Disclaimer

This is not related or provided with the support of Zenimax Online, Bethesda, MMOUI, or any other entity. It's just a personal project.

## License

LGPL. See [LICENSE](./LICENSE).
