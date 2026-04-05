# Library Template

## Overview

Template repository for creating C# libraries across the platform.

### What is contained here?

In here you will find common things such as NuGet targets for the internal
NuGet repository.

The Directory.Build.props also provides common properties that all
libraries should have.

### How to use?

To use this repository standalone (without rbx-dev):

```bash
# Clone the repository
$ git clone git@github.rbxlabs.dev:roblox/library-template.git test-library

# Initialize the solution
$ dotnet new sln --name Test

# Create projects
$ dotnet new classlib --name Roblox.Test.Library --output src
```

# Notice

## Usage of Roblox, or any of its assets.

# ***This project is not affiliated with Roblox Corporation.***

The usage of the name Roblox and any of its assets is purely for the purpose of providing a clear understanding of the project's purpose and functionality. This project is not endorsed by Roblox Corporation, and is not intended to be used for any commercial purposes.

Any code in this project was soley produced with or without the assistance of error traces and/or behaviour analysis of public facing APIs.
