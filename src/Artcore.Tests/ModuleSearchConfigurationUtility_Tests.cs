using System.Reflection;
using Art;
using Art.Common.IO;
using Artcore.Tests.TestSupport;

namespace Artcore.Tests;

public class ModuleSearchConfigurationUtility_Tests
{
    private const string TestModule1Name = "Artcore.ArtTestModule1";
    private const string TestModule2Name = "Artcore.ArtTestModule2";

    private static ModuleDefinition s_testModule1Definition = new(
        MainAssemblySimpleName: TestModule1Name,
        AssemblySimpleNames: ["Art", "Art.Common", TestModule1Name]
    );


    private static ModuleDefinition s_testModule2Definition = new(
        MainAssemblySimpleName: TestModule2Name,
        AssemblySimpleNames: ["Art", "Art.Common", TestModule2Name]
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
            var testModule1 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule1Definition);
            var testModule2 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule2Definition);
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
            using (var alcCache = ALCGroup.Create(3))
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
            var testModule1 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule1Definition);
            var testModule2 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule2Definition);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            ModuleSynthesizer.CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = ModuleSearchConfigurationUtility.GetModuleProvidersByPaths(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    moduleSearchFile
                ]);
            var moduleProvider = Assert.Single(moduleProviders);
            using (var alcCache = ALCGroup.Create(3))
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
            var testModule1 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule1Definition);
            var testModule2 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule2Definition);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            ModuleSynthesizer.CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = ModuleSearchConfigurationUtility.GetModuleProvidersByPaths(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    new ModuleSearchConfigurationSource(moduleSearchFile, modulesDir)
                ]);
            var moduleProvider = Assert.Single(moduleProviders);
            using (var alcCache = ALCGroup.Create(3))
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
            var testModule1 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule1Definition);
            var testModule2 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule2Definition);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            ModuleSynthesizer.CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = await ModuleSearchConfigurationUtility.GetModuleProvidersByPathsAsync(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    moduleSearchFile
                ],
                testCancellationToken
            );
            var moduleProvider = Assert.Single(moduleProviders);
            await using (var alcCache = ALCGroup.Create(3))
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
            var testModule1 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule1Definition);
            var testModule2 = ModuleSynthesizer.CreateModule(modulesDir, AppContext.BaseDirectory, s_testModule2Definition);
            string moduleSearchFile = Path.Join(modulesDir, "modules_search.json");
            ModuleSynthesizer.CreateModuleSearchFile(modulesDir, moduleSearchFile);
            var moduleProviders = await ModuleSearchConfigurationUtility.GetModuleProvidersByPathsAsync(
                new ModuleLoadConfiguration(IsCollectible: true, ["Art"]),
                [
                    new ModuleSearchConfigurationSource(moduleSearchFile, modulesDir)
                ],
                testCancellationToken
            );
            var moduleProvider = Assert.Single(moduleProviders);
            await using (var alcCache = ALCGroup.Create(3))
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

    private static void CheckModuleLoadability_MutualExclusion(ALCGroup alcGroup, IModuleProvider<ALCModule> moduleProvider, ModuleInfo[] createdModules)
    {
        List<Assembly> artAssemblies = [];
        List<Assembly> artCommonAssemblies = [];
        List<Assembly> selfAssemblies = [];
        try
        {
            foreach (var createdModule in createdModules)
            {
                var module = CheckModuleLoadability_GetModule(alcGroup, moduleProvider, createdModule);
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

    private static ALCModule CheckModuleLoadability_GetModule(ALCGroup alcGroup, IModuleProvider<ALCModule> moduleProvider, ModuleInfo moduleInfo)
    {
        bool gotModuleLocation = moduleProvider.TryLocateModule(moduleInfo.Info.AssemblySimpleName, out var moduleLocation);
        Assert.True(gotModuleLocation);
        Assert.True(moduleProvider.CanLoadModule(moduleLocation!));
        var module = moduleProvider.LoadModule(moduleLocation!);
        alcGroup.RegisterAssemblyLoadContext(module.AssemblyLoadContext);
        Assert.Equal(moduleInfo.Info.AssemblySimpleName, module.Assembly.GetName().Name);
        Assert.True(typeof(IArtifactTool).Assembly == module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName("Art")));
        foreach (var testAssembly in s_localTestModuleAssemblyReferences)
        {
            Assert.False(testAssembly == module.Assembly);
            Assert.False(testAssembly == module.AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName(moduleInfo.Info.AssemblySimpleName)));
        }
        foreach (var expectedAssemblyInfo in moduleInfo.IncludedAssemblies.Values)
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

    private static string CreateModulesDir()
    {
        string tmpParentDirectory = Path.Join(Path.GetTempPath(), "collectioneering_art_test_acmodule");
        Directory.CreateDirectory(tmpParentDirectory);
        string tempDirectory = ArtIOUtility.CreateRandomPath(tmpParentDirectory, ".tmpdir", pathCreationAction: PathCreationAction.CreateDirectory);
        Assert.EndsWith(".tmpdir", tempDirectory);
        return tempDirectory;
    }
}
