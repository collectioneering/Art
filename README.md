[![.NET](https://github.com/collectioneering/Art/actions/workflows/dotnet.yml/badge.svg)](https://github.com/collectioneering/Art/actions/workflows/dotnet.yml)

## Art

| Package              | Release                                                                                                               |
|----------------------|-----------------------------------------------------------------------------------------------------------------------|
| `Art`                | [![NuGet](https://img.shields.io/nuget/v/Art.svg)](https://www.nuget.org/packages/Art/)                               |
| `Art.BrowserCookies` | [![NuGet](https://img.shields.io/nuget/v/Art.BrowserCookies.svg)](https://www.nuget.org/packages/Art.BrowserCookies/) |
| `Art.Common`         | [![NuGet](https://img.shields.io/nuget/v/Art.Common.svg)](https://www.nuget.org/packages/Art.Common/)                 |
| `Art.EF`             | [![NuGet](https://img.shields.io/nuget/v/Art.EF.svg)](https://www.nuget.org/packages/Art.EF/)                         |
| `Art.EF.Sqlite`      | [![NuGet](https://img.shields.io/nuget/v/Art.EF.Sqlite.svg)](https://www.nuget.org/packages/Art.EF.Sqlite/)           |
| `Art.Html`           | [![NuGet](https://img.shields.io/nuget/v/Art.Html.svg)](https://www.nuget.org/packages/Art.Html/)                     |
| `Art.Http`           | [![NuGet](https://img.shields.io/nuget/v/Art.Http.svg)](https://www.nuget.org/packages/Art.Http/)                     |
| `Art.M3U`            | [![NuGet](https://img.shields.io/nuget/v/Art.M3U.svg)](https://www.nuget.org/packages/Art.M3U/)                       |
| `Art.Tesler`         | [![NuGet](https://img.shields.io/nuget/v/Art.Tesler.svg)](https://www.nuget.org/packages/Art.Tesler/)                 |
| `Artcore`            | [![NuGet](https://img.shields.io/nuget/v/Artcore.svg)](https://www.nuget.org/packages/Artcore/)                       |

Art is a set of packages for streamlining targeted archival of artifacts such as web articles with associated resource streams.

Art is intended to be used in plugin architectures, where implementors of sub-interfaces of `Art.IArtifactTool` expose capabilities such as listing, finding, and dumping resources based on a configuration provided to the tool.

Interfaces for artifact registration management (e.g. Sqlite) and data management (e.g. a folder on disk) are supplied. Instances of these interfaces are to be supplied to tools, primarily for dump tools, and can also be used to archive retrieved artifacts to arbitrary backing stores.
