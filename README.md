# Typify [![Build status](https://ci.appveyor.com/api/projects/status/10whg3491rccpje9/branch/master?svg=true)](https://ci.appveyor.com/project/p-ob/typify/branch/master)

Convert .NET classes into properly mapped TypeScript definitions to support 
seamless integration of ASP.NET server-side and TypeScript client-side models

## Features
1. Map base .NET objects (int, string, etc.) into their TypeScript counterparts (number, string, etc.)
1. Generate namespaces for each unique namespace
	- Note that it converts "Some.DotNet.Namespace" to "some-dotnet-namespace"
1. Create complex interfaces and enums based on custom classes
1. Handle external imports appropriately
1. Support nullable (e.g. int?) and "readonly" properties based on DataAnnotations, lack of public setters, or readonly modifiers
1. Generate complex generic interfaces to support generic typings

## Requirements
* Typify.NET targets .NET Standard 1.6, and supports [these target frameworks](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard1.6.md).  
* Typify.NET generates TypeScript definitions for TypeScript 2.0 and later.  
