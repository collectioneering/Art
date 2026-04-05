using System.Reflection;
using System.Text.Json;
using Art;
using Art.Common.IO;

namespace Artcore.Tests;

public class ModuleSearchConfigurationUtility_Tests
{
    private const string TestModule1Name = "Artcore.ArtTestModule1";
    private const string TestModule2Name = "Artcore.ArtTestModule2";

    private const string ModuleDirectorySuffix = ".module";
    private const string ModuleManifestFileSuffix = ".module.json";

    private static ModuleCreationInput TestModule1CreationInput = new(
        ModuleAssemblySimpleName: TestModule1Name,
        IncludedAssemblySimpleNames: ["Art", "Art.Common", TestModule1Name]
    );


    private static ModuleCreationInput TestModule2CreationInput = new(
        ModuleAssemblySimpleName: TestModule2Name,
        IncludedAssemblySimpleNames: ["Art", "Art.Common", TestModule2Name]
    );

    private static Assembly[] s_localTestModuleAssemblyReferences =
    [
        typeof(TestTool1).Assembly,
        typeof(TestTool2).Assembly,
    ];


    [Fact]
    public void GetModuleProviders_LoadsModulesCorrectly()
    {
        string modulesDir = CreateModulesDir();
        try
        {
            var testModule1 = CreateModule(modulesDir, TestModule1CreationInput);
            var testModule2 = CreateModule(modulesDir, TestModule2CreationInput);
            var moduleProviders = ModuleSearchConfigurationUtility.GetModuleProviders(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    new ModuleSearchConfigurationFile(new ModuleSearchConfiguration([
                        new ModuleSearchConfigurationEntry(
                            Path: modulesDir,
                            Name: null,
                            DirectorySuffix: ".module",
                            FileNameSuffix: ".module.json"
                        )
                    ]), AppContext.BaseDirectory),
                ]
            );
            var moduleProvider = Assert.Single(moduleProviders);
            using (var alcCache = new AlcCache())
            {
                // even same module loaded twice should be loaded distinct
                CheckModuleLoadability_MutualExclusion(alcCache, moduleProvider, [testModule1, testModule1, testModule2]);
            }
        }
        finally
        {
            Directory.Delete(modulesDir, true);
        }
    }

    [Fact]
    public void GetModuleProvidersByPaths_string_LoadsModulesCorrectly()
    {
        string modulesDir = CreateModulesDir();
        try
        {
            var testModule1 = CreateModule(modulesDir, TestModule1CreationInput);
            var testModule2 = CreateModule(modulesDir, TestModule2CreationInput);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = ModuleSearchConfigurationUtility.GetModuleProvidersByPaths(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    moduleSearchFile
                ]);
            var moduleProvider = Assert.Single(moduleProviders);
            using (var alcCache = new AlcCache())
            {
                // even same module loaded twice should be loaded distinct
                CheckModuleLoadability_MutualExclusion(alcCache, moduleProvider, [testModule1, testModule1, testModule2]);
            }
        }
        finally
        {
            Directory.Delete(modulesDir, true);
        }
    }

    [Fact]
    public void GetModuleProvidersByPaths_ModuleSearchConfigurationSource_LoadsModulesCorrectly()
    {
        string modulesDir = CreateModulesDir();
        try
        {
            var testModule1 = CreateModule(modulesDir, TestModule1CreationInput);
            var testModule2 = CreateModule(modulesDir, TestModule2CreationInput);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = ModuleSearchConfigurationUtility.GetModuleProvidersByPaths(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    new ModuleSearchConfigurationSource(moduleSearchFile, modulesDir)
                ]);
            var moduleProvider = Assert.Single(moduleProviders);
            using (var alcCache = new AlcCache())
            {
                // even same module loaded twice should be loaded distinct
                CheckModuleLoadability_MutualExclusion(alcCache, moduleProvider, [testModule1, testModule1, testModule2]);
            }
        }
        finally
        {
            Directory.Delete(modulesDir, true);
        }
    }

    [Fact]
    public async Task GetModuleProvidersByPathsAsync_string_LoadsModulesCorrectly()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        string modulesDir = CreateModulesDir();
        try
        {
            var testModule1 = CreateModule(modulesDir, TestModule1CreationInput);
            var testModule2 = CreateModule(modulesDir, TestModule2CreationInput);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = await ModuleSearchConfigurationUtility.GetModuleProvidersByPathsAsync(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    moduleSearchFile
                ],
                testCancellationToken
            );
            var moduleProvider = Assert.Single(moduleProviders);
            await using (var alcCache = new AlcCache())
            {
                // even same module loaded twice should be loaded distinct
                CheckModuleLoadability_MutualExclusion(alcCache, moduleProvider, [testModule1, testModule1, testModule2]);
            }
        }
        finally
        {
            Directory.Delete(modulesDir, true);
        }
    }

    [Fact]
    public async Task GetModuleProvidersByPathsAsync_ModuleSearchConfigurationSource_LoadsModulesCorrectly()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        string modulesDir = CreateModulesDir();
        try
        {
            var testModule1 = CreateModule(modulesDir, TestModule1CreationInput);
            var testModule2 = CreateModule(modulesDir, TestModule2CreationInput);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = await ModuleSearchConfigurationUtility.GetModuleProvidersByPathsAsync(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    new ModuleSearchConfigurationSource(moduleSearchFile, modulesDir)
                ],
                testCancellationToken
            );
            var moduleProvider = Assert.Single(moduleProviders);
            await using (var alcCache = new AlcCache())
            {
                // even same module loaded twice should be loaded distinct
                CheckModuleLoadability_MutualExclusion(alcCache, moduleProvider, [testModule1, testModule1, testModule2]);
            }
        }
        finally
        {
            Directory.Delete(modulesDir, true);
        }
    }

    private static void CheckModuleLoadability_MutualExclusion(AlcCache alcCache, IModuleProvider<ALCModule> moduleProvider, CreatedModuleInfo[] createdModules)
    {
        List<Assembly> artAssemblies = [];
        List<Assembly> artCommonAssemblies = [];
        List<Assembly> selfAssemblies = [];
        try
        {
            foreach (var createdModule in createdModules)
            {
                var module = CheckModuleLoadability_GetModule(alcCache, moduleProvider, createdModule);
                artAssemblies.Add(module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName("Art")));
                artCommonAssemblies.Add(module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName("Art.Common")));
                selfAssemblies.Add(module.Assembly);
            }
            for (int i = 0; i < artAssemblies.Count; i++)
            {
                for (int j = i + 1; j < artAssemblies.Count; j++)
                {
                    Assert.True(artAssemblies[i] == artAssemblies[j]);
                }
            }
            for (int i = 0; i < artCommonAssemblies.Count; i++)
            {
                for (int j = i + 1; j < artCommonAssemblies.Count; j++)
                {
                    Assert.False(artCommonAssemblies[i] == artCommonAssemblies[j]);
                }
            }
            for (int i = 0; i < selfAssemblies.Count; i++)
            {
                for (int j = i + 1; j < selfAssemblies.Count; j++)
                {
                    Assert.False(selfAssemblies[i] == selfAssemblies[j]);
                }
            }
        }
        finally
        {
            for (int i = 0; i < artCommonAssemblies.Count; i++)
            {
                artCommonAssemblies[i] = null!;
            }
            artCommonAssemblies.Clear();
        }
    }

    private static ALCModule CheckModuleLoadability_GetModule(AlcCache alcCache, IModuleProvider<ALCModule> moduleProvider, CreatedModuleInfo createdModuleInfo)
    {
        bool gotModuleLocation = moduleProvider.TryLocateModule(createdModuleInfo.Info.AssemblySimpleName, out var moduleLocation);
        Assert.True(gotModuleLocation);
        Assert.True(moduleProvider.CanLoadModule(moduleLocation!));
        var module = moduleProvider.LoadModule(moduleLocation!);
        alcCache.RegisterAssemblyLoadContext(module.AssemblyLoadContext);
        Assert.Equal(createdModuleInfo.Info.AssemblySimpleName, module.Assembly.GetName().Name);
        Assert.True(typeof(IArtifactTool).Assembly == module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName("Art")));
        foreach (var testAssembly in s_localTestModuleAssemblyReferences)
        {
            Assert.False(testAssembly == module.Assembly);
            Assert.False(testAssembly == module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName(createdModuleInfo.Info.AssemblySimpleName)));
        }
        foreach (var expectedAssemblyInfo in createdModuleInfo.IncludedAssemblies.Values)
        {
            if (expectedAssemblyInfo.AssemblySimpleName == "Art")
            {
                continue;
            }
            string fullPathExpected = Path.GetFullPath(expectedAssemblyInfo.AssemblyPath);
            string fullPathCreated = Path.GetFullPath(module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName(expectedAssemblyInfo.AssemblySimpleName)).Location);
            Assert.Equal(fullPathExpected, fullPathCreated);
        }
        return module;
    }

    private record ModuleCreationInput(
        string ModuleAssemblySimpleName,
        string[] IncludedAssemblySimpleNames);

    private record CreatedModuleInfo(
        string ModulePath,
        ExpectedAssemblyInfo Info,
        Dictionary<string, ExpectedAssemblyInfo> IncludedAssemblies);

    private record ExpectedAssemblyInfo(
        string AssemblySimpleName,
        string AssemblyPath);

    private static string CreateModulesDir()
    {
        string tmpParentDirectory = Path.Join(Path.GetTempPath(), "collectioneering_art_test_acmodule");
        Directory.CreateDirectory(tmpParentDirectory);
        string tempDirectory = ArtIOUtility.CreateRandomPath(tmpParentDirectory, ".tmpdir", pathCreationAction: PathCreationAction.CreateDirectory);
        Assert.EndsWith(".tmpdir", tempDirectory);
        return tempDirectory;
    }

    private static void WriteModuleManifest(string manifestFilePath, string assemblySimpleName)
    {
        using var tw = File.CreateText(manifestFilePath);
        tw.Write($$"""
                   {
                     "Assembly": "{{assemblySimpleName}}"
                   }
                   """);
    }

    private static Dictionary<string, ExpectedAssemblyInfo> CopyAssemblies(string moduleDir, string[] assemblySimpleNames)
    {
        string baseDirectory = AppContext.BaseDirectory;
        var dict = new Dictionary<string, ExpectedAssemblyInfo>();
        foreach (string assemblySimpleName in assemblySimpleNames)
        {
            string assemblyFileName = $"{assemblySimpleName}.dll";
            string outputFilePath = Path.Join(moduleDir, assemblyFileName);
            File.Copy(Path.Join(baseDirectory, assemblyFileName), outputFilePath);
            dict[assemblySimpleName] = new ExpectedAssemblyInfo(assemblySimpleName, outputFilePath);
        }
        return dict;
    }

    private static CreatedModuleInfo CreateModule(string modulesDir, ModuleCreationInput moduleCreationInput)
    {
        return CreateModule(
            Path.Join(modulesDir, $"{moduleCreationInput.ModuleAssemblySimpleName}{ModuleDirectorySuffix}"),
            $"{moduleCreationInput.ModuleAssemblySimpleName}{ModuleManifestFileSuffix}",
            moduleCreationInput.ModuleAssemblySimpleName,
            moduleCreationInput.IncludedAssemblySimpleNames
        );
    }

    private static CreatedModuleInfo CreateModule(string moduleDir, string moduleManifestFileName, string assemblySimpleName, string[] assemblySimpleNames)
    {
        Directory.CreateDirectory(moduleDir);
        WriteModuleManifest(Path.Join(moduleDir, moduleManifestFileName), assemblySimpleName);
        var dict = CopyAssemblies(moduleDir, assemblySimpleNames);
        return new CreatedModuleInfo(moduleDir, dict[assemblySimpleName], dict);
    }

    private static void CreateModuleSearchFile(string modulesDir, string moduleSearchFile)
    {
        using (var fs = File.Create(moduleSearchFile))
        {
            JsonSerializer.Serialize(fs,
                new ModuleSearchConfiguration([
                    new ModuleSearchConfigurationEntry(
                        Path: modulesDir,
                        Name: null,
                        DirectorySuffix: ".module",
                        FileNameSuffix: ".module.json"
                    )
                ]));
        }
    }
}
