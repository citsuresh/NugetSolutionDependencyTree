using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Frameworks;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

class Program
{
    static void Main(string[] args)
    {

        string solutionPath = string.Empty;

        if (args.Length == 0)
        {
            solutionPath = @"C:\ItronProjects\SPMT_OsUpgrade\RNDTools.Network.Deploy\SBRBatchUpdateTool\SBRBatchUpdateTool.sln";
            //Console.WriteLine("Please provide the path to the .sln file.");
            //return;
        }
        else
        {
            solutionPath = args[0];
        }

        var projectCollection = new ProjectCollection();
        var solutionFile = SolutionFile.Parse(solutionPath);

        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = repository.GetResource<DependencyInfoResource>();

        var dependencyTree = new Dictionary<string, Dictionary<string, List<string>>>();

        foreach (var project in solutionFile.ProjectsInOrder)
        {
            var projectInstance = projectCollection.LoadProject(project.AbsolutePath);
            var packagesConfigPath = projectInstance.AllEvaluatedItems.FirstOrDefault(i => i.ItemType == "None" && i.EvaluatedInclude.EndsWith("packages.config"))?.EvaluatedInclude;

            if (packagesConfigPath != null)
            {
                var packages = ParsePackagesConfig(Path.Combine(projectInstance.DirectoryPath, packagesConfigPath));
                var framework = GetProjectFramework(projectInstance);

                foreach (var package in packages)
                {
                    if (!dependencyTree.ContainsKey(package.Id))
                    {
                        dependencyTree[package.Id] = new Dictionary<string, List<string>>();
                    }

                    var dependencies = GetPackageDependencies(resource, package.Id, package.Version, framework).Result;
                    if (dependencies == null)
                    {
                        Console.WriteLine($"Failed to resolve package: {package.Id} {package.Version}");
                    }
                    else
                    {
                        dependencyTree[package.Id][package.Version] = dependencies;
                    }
                }
            }
        }

        PrintDependencyTree(dependencyTree);
        WriteDependencyTreeToJson(dependencyTree, "dependencyTree.json");
    }

    static List<Package> ParsePackagesConfig(string filePath)
    {
        var xdoc = XDocument.Load(filePath);
        return xdoc.Root.Elements("package")
                        .Select(x => new Package
                        {
                            Id = x.Attribute("id")?.Value,
                            Version = x.Attribute("version")?.Value
                        })
                        .ToList();
    }

    static NuGetFramework GetProjectFramework(Project project)
    {
        var targetFrameworkMoniker = project.GetProperty("TargetFrameworkMoniker")?.EvaluatedValue;
        return NuGetFramework.Parse(targetFrameworkMoniker);
    }

    static async Task<List<string>> GetPackageDependencies(DependencyInfoResource resource, string packageId, string version, NuGetFramework framework)
    {
        var logger = NullLogger.Instance;
        var cache = new SourceCacheContext();
        var identity = new PackageIdentity(packageId, NuGetVersion.Parse(version));
        var packageInfo = await resource.ResolvePackage(identity, framework, cache, logger, CancellationToken.None);

        if (packageInfo == null)
        {
            return null;
        }

        return packageInfo.Dependencies.Select(d => d.Id).ToList();
    }

    static void PrintDependencyTree(Dictionary<string, Dictionary<string, List<string>>> tree, int level = 0)
    {
        foreach (var package in tree.Keys)
        {
            foreach (var version in tree[package].Keys)
            {
                PrintDependencySubTree(tree, package, version, level);
            }
        }
    }

    static void PrintDependencySubTree(Dictionary<string, Dictionary<string, List<string>>> tree, string root, string version, int level)
    {
        Console.WriteLine(new string(' ', level * 2) + root + " (v" + version + ")");
        if (tree.ContainsKey(root) && tree[root].ContainsKey(version) && tree[root][version] != null)
        {
            foreach (var dependency in tree[root][version])
            {
                PrintDependencySubTree(tree, dependency, "version", level + 1);
            }
        }
    }

    static void WriteDependencyTreeToJson(Dictionary<string, Dictionary<string, List<string>>> tree, string filePath)
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(tree, jsonOptions);
        File.WriteAllText(filePath, jsonString);
    }
}

class Package
{
    public string Id { get; set; }
    public string Version { get; set; }
}
