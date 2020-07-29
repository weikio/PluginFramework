# Introduction 
Plugin Framework adds easy-to-use and lightweight support for Plugins into ASP.NET Core and Windows Forms / WPF / Console applications. 

Main features:

* Everything is a plugin! Deliver plugins as Nuget-packages, .NET assemblies, Roslyn scripts and more.
* Easy integration into a new or an existing .NET Core application.
* Automatic dependency management.
* MIT-licensed, commercial support available.

## Quickstart: Plugin Framework & ASP.NET Core

Plugin Framework is available from Nuget as a .NET Core 3.1 package. There's a separate package which makes it easier to work with plugins in an ASP.NET Core app:

```
Install-Package Weikio.PluginFramework.AspNetCore
```

Using Plugin Framework can be as easy as adding a single new line into ConfigureServices:

```
services.AddPluginFramework<IOperator>(@".\mypluginsFolder");
```

The code finds all the plugins (types that implement the custom IOperator-interface) from the mypluginsFolder. The plugins can be used in a controller using constructor injection:

```
public CalculatorController(IEnumerable<Plugin> plugins)
{
	_operators = operators;
}
```

Alternatively, you can provide multiple plugin locations using catalogs:

```
var folderPluginCatalog = new FolderPluginCatalog(@".\mypluginsFolder", type =>
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

Plugin Framework provides the following officially supported catalogs:

* Delegate (Action or a Func)
* Type
* Assembly
* Folder
* Roslyn script
* Nuget package
* Nuget feed


# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process
2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)