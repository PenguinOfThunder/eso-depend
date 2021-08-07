# Elder Scrolls Online Add-On Dependency Visualizer (ESO-ADV)

## Description

A desktop application that scans your Addons folder, then creates a dependency graph of the add-ons it found, allowing you to:

- create a list of your installed add-ons
- visualize the relationship between them to determine if you are missing a dependency, or if you have conflicts

The application is divided into multiple parts:

- Metadata parser: module that parses metadata (.txt) files inside AddOns folder
- Dependency graph resolver: works on the metadata and creates a directed graph of dependencies from it. It also tries to find issues, like missing dependencies.
- Dependency visualizer: desktop app that reports on the dependencies and issues.

## Dependencies

The application and libraries are written in .Net 5. The installer bundles the necessary libraries and runtime.

Third-party libraries used:

- ???

## Disclaimer

This is not a product of Zenimax Online, etc.
