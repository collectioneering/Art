using System.Text.Json;

namespace Artcore.Tests.TestSupport;

public static class ModuleSynthesizer
{
    private const string ModuleDirectorySuffix = ".module";
    private const string ModuleManifestFileSuffix = ".module.json";

    private static void WriteModuleManifest(string manifestFilePath, string assemblySimpleName)
    {
        using var tw = File.CreateText(manifestFilePath);
        tw.Write($$"""
                   {
                     "Assembly": "{{assemblySimpleName}}"
                   }
                   """);
    }

    private static Dictionary<string, ModuleAssemblyInfo> CopyAssemblies(
        string moduleDir,
        string assemblySourceDirectory,
        IReadOnlyCollection<string> assemblySimpleNames
    )
    {
        var dict = new Dictionary<string, ModuleAssemblyInfo>();
        foreach (string assemblySimpleName in assemblySimpleNames)
        {
            string assemblyFileName = $"{assemblySimpleName}.dll";
            string outputFilePath = Path.Join(moduleDir, assemblyFileName);
            File.Copy(Path.Join(assemblySourceDirectory, assemblyFileName), outputFilePath);
            // full path to handle symlink e.g. osx temp dir
            string resolvedAssemblyFilePath = LinkPathResolution.BuildRealPath(Path.Join(moduleDir, assemblyFileName));
            dict[assemblySimpleName] = new ModuleAssemblyInfo(assemblySimpleName, resolvedAssemblyFilePath);
        }
        return dict;
    }

    public static ModuleInfo CreateModule(
        string modulesDir,
        string assemblySourceDirectory,
        ModuleDefinition moduleDefinition)
    {
        return CreateModule(
            Path.Join(modulesDir, $"{moduleDefinition.MainAssemblySimpleName}{ModuleDirectorySuffix}"),
            assemblySourceDirectory,
            $"{moduleDefinition.MainAssemblySimpleName}{ModuleManifestFileSuffix}",
            moduleDefinition.MainAssemblySimpleName,
            moduleDefinition.AssemblySimpleNames
        );
    }

    private static ModuleInfo CreateModule(
        string moduleDir,
        string assemblySourceDirectory,
        string moduleManifestFileName,
        string assemblySimpleName,
        string[] assemblySimpleNames)
    {
        Directory.CreateDirectory(moduleDir);
        WriteModuleManifest(Path.Join(moduleDir, moduleManifestFileName), assemblySimpleName);
        var set = new HashSet<string>(assemblySimpleNames);
        set.Add(assemblySimpleName);
        var dict = CopyAssemblies(moduleDir, assemblySourceDirectory, set);
        return new ModuleInfo(moduleDir, dict[assemblySimpleName], dict);
    }

    public static void CreateModuleSearchFile(string modulesDir, string moduleSearchFile)
    {
        using var fs = File.Create(moduleSearchFile);
        JsonSerializer.Serialize(fs,
            new ModuleSearchConfiguration([
                new ModuleSearchConfigurationEntry(
                    Path: modulesDir,
                    Name: null,
                    DirectorySuffix: ModuleDirectorySuffix,
                    FileNameSuffix: ModuleManifestFileSuffix
                )
            ]));
    }
}

public record ModuleDefinition(
    string MainAssemblySimpleName,
    string[] AssemblySimpleNames);

public record ModuleInfo(
    string ModulePath,
    ModuleAssemblyInfo Info,
    Dictionary<string, ModuleAssemblyInfo> IncludedAssemblies);

public record ModuleAssemblyInfo(
    string AssemblySimpleName,
    string AssemblyPath);
