# NugetSolutionDependencyTree

**NugetSolutionDependencyTree** is a command-line tool that visualizes the dependency tree of NuGet packages across multiple projects in a solution. It parses the `packages.config` files in each project, fetches package dependencies, and generates a comprehensive dependency tree. The tool can output the tree in JSON format and display it in the console.

## Features

- Parses `packages.config` files from multiple projects in a solution.
- Fetches dependencies for each package from the NuGet API.
- Builds and prints a detailed dependency tree.
- Outputs the dependency tree to a local JSON file.

## Installation

1. **Clone the Repository**
   ```sh
   git clone https://github.com/yourusername/NugetSolutionDependencyTree.git
   cd NugetSolutionDependencyTree
   
2. **Install Dependencies**
   - Ensure you have the .NET SDK installed.
   - Install the required NuGet packages:
     ```sh
     dotnet add package Microsoft.Build.Locator
     dotnet add package Microsoft.Build
     dotnet add package Microsoft.Build.Framework
     dotnet add package Microsoft.Build.Utilities.Core
     dotnet add package NuGet.Protocol
     dotnet add package System.Text.Json
     ```
## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## Acknowledgements

- NuGet Protocol API
- Microsoft Build Tools

