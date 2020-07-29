<img align="right" alt="Plugin Framework Logo" src="docs/logo_transparent_color_256.png">

# Plugin Framework for .NET Core [![NuGet Version](https://img.shields.io/nuget/v/Weikio.PluginFramework.svg?style=flat)](https://www.nuget.org/packages/Weikio.PluginFramework/)

With Plugin Framework for .NET Core, everything is a plugin! Plugin Framework is a **plugin platform** for .NET Core applications, including **ASP.NET Core, Blazor, WPF, Windows Forms and Console apps**. It is light-weight and easy to integrate and supports multiple different plugin catalogs, including .NET assemblies, **Nuget packages** and **Roslyn scripts**.

### Main Features 

Here are some of the main features of Plugin Framework: 

* Everything is a plugin! Deliver plugins as Nuget-packages, .NET assemblies, Roslyn scripts and more.
* Easy integration into a new or an existing .NET Core application.
* Automatic dependency management.
* MIT-licensed, commercial support available.

## Quickstart: Plugin Framework & ASP.NET Core

Plugin Framework is available from Nuget as a .NET Core 3.1 package. There's a separate package which makes it easier to work with plugins in an ASP.NET Core app:

[![NuGet Version](https://img.shields.io/nuget/v/Weikio.PluginFramework.AspNetCore.svg?style=flat)](https://www.nuget.org/packages/Weikio.PluginFramework.AspNetCore/)

```
Install-Package Weikio.PluginFramework.AspNetCore
```

Using Plugin Framework can be as easy as adding a single new line into ConfigureServices:

```
services.AddPluginFramework<IOperator>(@".\myplugins");
```

The code finds all the plugins (types that implement the custom IOperator-interface) from the myplugins-folder. The plugins can be used in a controller using constructor injection:

```
public CalculatorController(IEnumerable<IOperator> operator)
{
	_operators = operators;
}
```

Alternatively, you can provide multiple plugin locations using catalogs:

```
var folderPluginCatalog = new FolderPluginCatalog(@".\myplugins", type =>
{
    type.Implements<IOperator>();
});

var anotherPluginCatalog = new FolderPluginCatalog(@".\morePlugins", type =>
{
    type.Implements<IOperator>();
});

services.AddPluginFramework()
    .AddPluginCatalog(folderPluginCatalog)
    .AddPluginCatalog(anotherPluginCatalog)
    .AddPluginType<IOperator>();
```

## Plugin catalogs

Plugin Framework contains a concept called "Catalog". A single assembly can be a catalog. A .NET type can be a catalog. Folders with dlls are often used as catalogs and Plugin Framework also supports Nuget packages, Nuget feeds and Roslyn scripts as Plugin catalogs. Multiple catalogs are often combined into a singe Composite catalog.

Each catalog contains 0-n plugins. 

Plugin Framework provides the following officially supported catalogs:

* Type
* Assembly
* Folder
* Delegate (Action or a Func)
* Roslyn script
* Nuget package
* Nuget feed

## Source code

Source code for Plugin Framework is available from [GitHub](https://github.com/weikio/PluginFramework).

## Support

Commercial support for Plugin Framework is provided by [Adafy](https://adafy.com).

![Adafy Logo](docs/Adafy_logo_256.png)

Adafy is a Finnish software development house, focusing on Microsoft technologies.

## License

Plugin Framework is available as an open source, MIT-licensed project. 